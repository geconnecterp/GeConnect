using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class NotaDebitoCreditoController : ControladorBaseCaja
    {
        private const string SessionKeyContexto = "NDCFS_CONTEXTO";
        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly IProductoFactServicio _productoFactServicio;
        private readonly INotaCreditoServicio _notaCreditoServicio;
        private readonly ICajaServicio _cajaServicio;
        private readonly AppSettings _appSettings;

        public NotaDebitoCreditoController(
            IOptions<AppSettings> options,
            ILogger<NotaDebitoCreditoController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            IProductoFactServicio productoFactServicio,
            INotaCreditoServicio notaCreditoServicio,
            ICajaServicio cajaServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _productoFactServicio = productoFactServicio;
            _notaCreditoServicio = notaCreditoServicio;
            _cajaServicio = cajaServicio;
            _appSettings = options.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return redirectResult;
            }

            var caja = CajaActual;

            if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
            {
                TempData["Error"] =
                    "No se encontraron datos validos de caja para iniciar el modulo de ND, NC y Factura de Servicio.";

                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTiposComprobante()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "Sesion expirada."
                });
            }

            var token = TokenCookie;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger?.LogWarning(
                    "ND/NC/FS: no se pudo obtener tipos de comprobante origen: token inexistente."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "La sesion actual no posee un token valido."
                });
            }

            var afipId = string.IsNullOrWhiteSpace(_appSettings.NotaCreditoDevolucionAfipId)
                ? "%"
                : _appSettings.NotaCreditoDevolucionAfipId.Trim();

            var optId = string.IsNullOrWhiteSpace(_appSettings.NotaCreditoDevolucionOptId)
                ? "VE"
                : _appSettings.NotaCreditoDevolucionOptId.Trim();

            try
            {
                var resultado = await _notaCreditoServicio.GetTipoComprobante(
                    afipId,
                    optId,
                    token
                );

                if (resultado == null || !resultado.Ok)
                {
                    var mensaje = resultado?.Mensaje
                        ?? "No fue posible obtener los tipos de comprobante.";

                    _logger?.LogWarning(
                        "ND/NC/FS: error obteniendo tipos de comprobante origen. Mensaje={Mensaje}",
                        mensaje
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje
                    });
                }

                var tipos = (resultado.ListaEntidad ?? [])
                    .Where(x => !string.IsNullOrWhiteSpace(x.tco_id))
                    .Select(x => new
                    {
                        tco_id = x.tco_id.Trim(),
                        tco_desc = x.tco_desc?.Trim() ?? string.Empty,
                        tco_letra = x.tco_letra?.Trim() ?? string.Empty,
                        tco_tipo = x.tco_tipo?.Trim() ?? string.Empty
                    })
                    .ToList();

                if (tipos.Count == 0)
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: no se encontraron tipos de comprobante origen. afip_id={AfipId}, opt_id={OptId}",
                        afipId,
                        optId
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se encontraron tipos de comprobante habilitados."
                    });
                }

                return Json(new
                {
                    ok = true,
                    datos = tipos
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error inesperado al obtener tipos de comprobante origen."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Ocurrio un error al obtener los tipos de comprobante."
                });
            }
        }
        [HttpPost]
        public JsonResult ValidacionInicial()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new
                    {
                        success = false,
                        message = "La sesion ha expirado. Vuelva a iniciar sesion."
                    });
                }

                var caja = CajaActual;

                if (caja == null)
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: CajaActual es null. Usuario={Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message = "No se ha configurado una caja para esta estacion."
                    });
                }

                if (string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: CajaId vacio. Usuario={Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message = "La caja no tiene un identificador valido."
                    });
                }

                if (caja.Caja == null)
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: datos de Caja no disponibles. CajaId={CajaId}, Usuario={Usuario}",
                        caja.CajaId,
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message = "Los datos de la caja no estan disponibles. Cierre sesion y vuelva a abrir la caja."
                    });
                }

                var (esValido, mensajeValidacion) =
                    _cajaInitServicio.ValidarDatosIniciales(caja);

                if (!esValido)
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: validacion de caja fallida. CajaId={CajaId}, Motivo={Motivo}",
                        caja.CajaId,
                        mensajeValidacion
                    );

                    return Json(new
                    {
                        success = false,
                        message = mensajeValidacion
                    });
                }

                _logger?.LogInformation(
                    "ND/NC/FS: validacion de caja exitosa. CajaId={CajaId}, Usuario={Usuario}",
                    caja.CajaId,
                    UserName
                );

                return Json(new
                {
                    success = true,
                    message = mensajeValidacion,
                    caja_id = caja.CajaId,
                    caja_nombre = caja.Caja.caja_nombre ?? string.Empty,
                    usuario = UserName
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error inesperado en ValidacionInicial. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    success = false,
                    message = "Error interno al validar los datos de la caja. Contacte al administrador."
                });
            }
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerIvaAlicuotas()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion ha expirado. Vuelva a iniciar sesion.",
                        lista = Array.Empty<object>()
                    });
                }

                _logger?.LogInformation(
                    "ND/NC/FS: solicitando alicuotas IVA. Usuario={Usuario}",
                    UserName);

                var respuesta = await _productoFactServicio.ObtenerIvaAlicuotas(TokenCookie);
                var lista = (respuesta.ListaEntidad ?? [])
                    .Select(x => new
                    {
                        ivaAlicuota = x.IVA_Alicuota,
                        ivaGrl = x.IVA_Grl,
                        ivaExtra = x.IVA_Extra,
                        ivaAfip = x.IVA_Afip
                    })
                    .ToList();

                _logger?.LogInformation(
                    "ND/NC/FS: alicuotas IVA recibidas. Ok={Ok}; Cantidad={Cantidad}",
                    respuesta.Ok,
                    lista.Count);

                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Mensaje,
                    lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error obteniendo alicuotas IVA. Usuario={Usuario}",
                    UserName);

                return Json(new
                {
                    ok = false,
                    mensaje = "No se pudieron obtener las alicuotas IVA.",
                    lista = Array.Empty<object>()
                });
            }
        }

        [HttpPost]
        public JsonResult RegistrarCuentaSeleccionada()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion ha expirado. Vuelva a iniciar sesion."
                    });
                }

                var cuenta = ClienteActual;
                if (cuenta == null)
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: intento de registrar cuenta sin ClienteActual en sesion. Usuario={Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "No hay una cuenta seleccionada para iniciar la operacion."
                    });
                }

                var origen = (cuenta.Origen ?? string.Empty).Trim().ToUpperInvariant();
                if (string.IsNullOrWhiteSpace(origen))
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: cuenta sin origen. Cuenta={CuentaId}, Usuario={Usuario}",
                        cuenta.cta_id,
                        UserName
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "La cuenta seleccionada no informa un origen valido."
                    });
                }

                if (origen is "F" or "N" or "Q")
                {
                    var mensaje = origen switch
                    {
                        "F" => "Consumidor final no esta habilitado para este modulo.",
                        "N" => "El cliente seleccionado no esta habilitado.",
                        "Q" => "El proveedor seleccionado no esta habilitado.",
                        _ => "La cuenta seleccionada no esta habilitada."
                    };

                    _logger?.LogInformation(
                        "ND/NC/FS: cuenta rechazada por origen. Cuenta={CuentaId}, Origen={Origen}, Usuario={Usuario}",
                        cuenta.cta_id,
                        origen,
                        UserName
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje
                    });
                }

                if (origen is not ("C" or "P"))
                {
                    _logger?.LogWarning(
                        "ND/NC/FS: origen no contemplado para la cuenta. Cuenta={CuentaId}, Origen={Origen}, Usuario={Usuario}",
                        cuenta.cta_id,
                        origen,
                        UserName
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "El origen de la cuenta no esta contemplado para este modulo."
                    });
                }

                var operacionesPermitidas = ObtenerOperacionesPermitidas(origen);
                var contexto = new NotaDebitoCreditoContextoSesion
                {
                    Cuenta = cuenta,
                    Origen = origen,
                    OperacionesPermitidas = operacionesPermitidas,
                    FechaCreacionUtc = DateTime.UtcNow
                };

                GuardarContexto(contexto);

                _logger?.LogInformation(
                    "ND/NC/FS: cuenta registrada en contexto. Cuenta={CuentaId}, Origen={Origen}, Operaciones={Operaciones}, Usuario={Usuario}",
                    cuenta.cta_id,
                    origen,
                    string.Join(",", operacionesPermitidas),
                    UserName
                );

                return Json(new
                {
                    ok = true,
                    mensaje = "Cuenta registrada correctamente.",
                    cuenta = CrearResumenCuenta(cuenta, origen),
                    operacionesPermitidas = operacionesPermitidas
                        .Select(CrearResumenOperacion)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error al registrar cuenta seleccionada. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Error interno al registrar la cuenta seleccionada."
                });
            }
        }

        [HttpPost]
        public JsonResult CancelarOperacion()
        {
            try
            {
                LimpiarContexto();
                ClienteActual = null;

                _logger?.LogInformation(
                    "ND/NC/FS: contexto de operacion cancelado. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = true,
                    mensaje = "Operacion cancelada."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error al cancelar contexto de operacion. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "No se pudo cancelar la operacion."
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CalcularConceptos([FromBody] NotaDebitoCreditoCalcularRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion ha expirado. Vuelva a iniciar sesion."
                    });
                }

                var contexto = ObtenerContexto();
                if (contexto == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No hay una cuenta registrada para calcular la operacion."
                    });
                }

                var validacion = ValidarSolicitudCalculo(contexto, request);
                if (!validacion.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = validacion.Mensaje
                    });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion actual no posee un token valido."
                    });
                }

                var productosOriginales = new JArray();
                var totalCantidad = 0m;
                var totalControl = 0m;
                var item = 1;

                foreach (var concepto in request.Conceptos)
                {
                    var cantidad = concepto.Cantidad <= 0 ? 1 : concepto.Cantidad;
                    var iva = CalcularIvaManual(concepto.NetoGravado, concepto.AlicuotaIva);
                    var totalConcepto = (concepto.NetoGravado + concepto.PercepcionIb + concepto.PercepcionIva + iva) * cantidad;
                    var filaJson = JObject.FromObject(CrearJsonConcepto(contexto, request.CoTipo, concepto, item));

                    productosOriginales.Add(filaJson);
                    totalCantidad += cantidad;
                    totalControl += totalConcepto;
                    item++;
                }

                var requestCalculo = CrearRequestCalculoFilas(contexto, request, productosOriginales, totalCantidad, totalControl);

                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                _logger?.LogInformation("ND/NC/FS - REQUEST CALCULAR FILAS");
                _logger?.LogInformation("   Operacion={Operacion}; Cuenta={Cuenta}; Conceptos={Conceptos}", request.CoTipo, contexto.Cuenta.cta_id, productosOriginales.Count);
                _logger?.LogInformation("   Request={Request}", JsonConvert.SerializeObject(requestCalculo));
                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

                var resultado = await _productoFactServicio.CalcularFilas(requestCalculo, token);

                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                _logger?.LogInformation("ND/NC/FS - RESPONSE CALCULAR FILAS");
                _logger?.LogInformation("   Resultado null={ResultadoNull}; json_subtotal_len={JsonSubtotalLen}; json_p_len={JsonProductosLen}",
                    resultado == null,
                    resultado?.json_subtotal?.Length ?? 0,
                    resultado?.json_p?.Length ?? 0);
                _logger?.LogInformation("   Response={Response}", JsonConvert.SerializeObject(resultado));
                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

                if (resultado == null || string.IsNullOrWhiteSpace(resultado.json_subtotal))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se pudo calcular la operacion."
                    });
                }

                contexto.CoTipo = request.CoTipo.Trim().ToUpperInvariant();
                contexto.TcoIdOri = contexto.CoTipo == "NC" ? (request.TcoIdOri ?? string.Empty).Trim() : string.Empty;
                contexto.CmCompteOri = contexto.CoTipo == "NC" ? (request.CmCompteOri ?? string.Empty).Trim() : string.Empty;
                contexto.CmRepetidoOri = contexto.CoTipo == "NC" ? (request.CmRepetidoOri ?? string.Empty).Trim() : string.Empty;
                contexto.JsonProductosOriginal = productosOriginales.ToString(Formatting.None);
                contexto.JsonProductosCalculado = !string.IsNullOrWhiteSpace(resultado.json_p)
                    ? resultado.json_p
                    : contexto.JsonProductosOriginal;
                contexto.JsonSubtotal = resultado.json_subtotal;
                contexto.JsonSorteo = "[]";
                contexto.Total = totalControl;
                contexto.FechaUltimoCalculoUtc = DateTime.UtcNow;

                GuardarContexto(contexto);

                return Json(new
                {
                    ok = true,
                    mensaje = "Conceptos calculados correctamente.",
                    operacion = CrearResumenOperacion(contexto.CoTipo),
                    cuenta = CrearResumenCuenta(contexto.Cuenta, contexto.Origen),
                    calculo = new
                    {
                        json_subtotal = contexto.JsonSubtotal,
                        json_p = contexto.JsonProductosCalculado,
                        total = contexto.Total
                    },
                    subtotales = CrearResumenSubtotales(contexto.JsonSubtotal, contexto.Total)
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error calculando conceptos. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Error interno al calcular los conceptos."
                });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarOperacion()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion ha expirado. Vuelva a iniciar sesion."
                    });
                }

                var contexto = ObtenerContexto();
                if (contexto == null ||
                    string.IsNullOrWhiteSpace(contexto.CoTipo) ||
                    string.IsNullOrWhiteSpace(contexto.JsonProductosCalculado) ||
                    string.IsNullOrWhiteSpace(contexto.JsonSubtotal))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No hay un calculo vigente para confirmar."
                    });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La sesion actual no posee un token valido."
                    });
                }

                var cajaActual = CajaActual;
                if (cajaActual?.Caja == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se encontraron datos validos de caja para confirmar la operacion."
                    });
                }

                var request = CrearRequestConfirmacion(contexto);

                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                _logger?.LogInformation("ND/NC/FS - VALIDANDO ESTADO DEL PUNTO DE VENTA");
                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

                var validacionPV = await ValidarEstadoPuntoVenta(
                    cajaServicio: _cajaServicio,
                    cajaId: cajaActual.CajaId ?? string.Empty,
                    ctrlId: cajaActual.Caja.ctrl_id ?? string.Empty,
                    nroProceso: request.caja_nro_proceso,
                    nroCierre: request.caja_nro_cierre,
                    tipoLlamada: "F"
                );

                if (!validacionPV.PuedeContinuar)
                {
                    _logger?.LogError("ND/NC/FS: validacion de PV fallÃ³ - Operacion bloqueada");
                    _logger?.LogError("   Resultado: {Resultado}", validacionPV.Resultado);
                    _logger?.LogError("   Mensaje: {Mensaje}", validacionPV.Mensaje);

                    return Json(new
                    {
                        ok = false,
                        mensaje = validacionPV.Mensaje,
                        error_tipo = "estado_pv",
                        ctrl_id = validacionPV.CtrlId,
                        resultado_pv = validacionPV.Resultado
                    });
                }

                if (validacionPV.EsAdvertencia)
                {
                    _logger?.LogWarning("ND/NC/FS: validacion de PV con advertencia - Operacion continua");
                    _logger?.LogWarning("   Resultado: {Resultado}", validacionPV.Resultado);
                    _logger?.LogWarning("   Mensaje: {Mensaje}", validacionPV.Mensaje);
                }
                else
                {
                    _logger?.LogInformation("ND/NC/FS: validacion de PV exitosa - Operacion autorizada");
                }

                request.caea = cajaActual.Caja.ctrl_id == "-1" && validacionPV.Resultado == 1;

                _logger?.LogInformation(
                    "ND/NC/FS: FormaPago={CtrlId} - ResultadoPV={ResultadoPV} - CAEA={Caea}",
                    cajaActual.Caja.ctrl_id,
                    validacionPV.Resultado,
                    request.caea
                );

                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                _logger?.LogInformation("âœ… ND/NC/FS - REQUEST CONFIRMAR OPERACION");
                _logger?.LogInformation("   Operacion={Operacion}; Cuenta={Cuenta}; Total={Total}", contexto.CoTipo, contexto.Cuenta.cta_id, contexto.Total);
                _logger?.LogInformation("   Request={Request}", JsonConvert.SerializeObject(request));
                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

                var resultado = await _notaCreditoServicio.ConfirmarOperacionCaja(request, token);

                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");
                _logger?.LogInformation("ðŸ“¥ ND/NC/FS - RESPONSE CONFIRMAR OPERACION");
                _logger?.LogInformation("   Ok={Ok}; Mensaje={Mensaje}", resultado?.Ok, resultado?.Mensaje);
                _logger?.LogInformation("   Response={Response}", JsonConvert.SerializeObject(resultado));
                _logger?.LogInformation("â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•");

                if (resultado == null || !resultado.Ok || resultado.Entidad == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = resultado?.Mensaje ?? "No se pudo confirmar la operacion."
                    });
                }

                var respuesta = resultado.Entidad;
                if (respuesta.resultado != 0)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = respuesta.resultado_msj ?? "No se pudo confirmar la operacion."
                    });
                }

                if (!TryParsearComprobanteJson(respuesta.resultado_id, out var comprobanteEmitido) ||
                    comprobanteEmitido == null)
                {
                    return Json(new
                    {
                        ok = false,
                        mensaje = "La operacion fue procesada, pero no se pudo interpretar el comprobante emitido.",
                        debug_resultado_id = respuesta.resultado_id
                    });
                }

                var debeImprimir = DebeImprimirComprobanteElectronico();
                var reporteModo = NormalizarModoReporte(_appSettings.NotaCreditoReporteModo);
                var mensajeFinal = CrearMensajeOperacionConfirmada(contexto.CoTipo, comprobanteEmitido.tco_letra, comprobanteEmitido.cm_compte);

                LimpiarContexto();

                return Json(new
                {
                    ok = true,
                    mensaje = mensajeFinal,
                    resultado = respuesta.resultado,
                    resultado_id = respuesta.resultado_id,
                    resultado_completo = respuesta.resultado_msj,
                    comprobante = respuesta.resultado_id,
                    operacion = contexto.CoTipo,
                    debe_imprimir = debeImprimir,
                    reporte_modo = reporteModo,
                    reporte = new
                    {
                        habilitado = debeImprimir,
                        modo = reporteModo,
                        motivo = debeImprimir
                            ? "Caja configurada para Factura Electronica."
                            : "La caja no esta configurada para Factura Electronica."
                    },
                    data = new[]
                    {
                        new
                        {
                            tco_letra = comprobanteEmitido.tco_letra,
                            tco_id = comprobanteEmitido.tco_id,
                            cm_compte = comprobanteEmitido.cm_compte,
                            cm_repetido = comprobanteEmitido.cm_repetido,
                            co_tipo = contexto.CoTipo,
                            debe_imprimir = debeImprimir,
                            reporte_modo = reporteModo
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "ND/NC/FS: error confirmando operacion. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Error interno al confirmar la operacion."
                });
            }
        }

        private void GuardarContexto(NotaDebitoCreditoContextoSesion contexto)
        {
            HttpContext.Session.SetString(
                SessionKeyContexto,
                JsonConvert.SerializeObject(contexto)
            );
        }

        private void LimpiarContexto()
        {
            HttpContext.Session.Remove(SessionKeyContexto);
        }

        private NotaDebitoCreditoContextoSesion? ObtenerContexto()
        {
            var json = HttpContext.Session.GetString(SessionKeyContexto);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonConvert.DeserializeObject<NotaDebitoCreditoContextoSesion>(json);
        }

        private static List<string> ObtenerOperacionesPermitidas(string origen)
        {
            return origen == "P"
                ? new List<string> { "ND", "NC", "FS" }
                : new List<string> { "NC", "FS" };
        }

        private static object CrearResumenOperacion(string codigo)
        {
            return new
            {
                codigo,
                descripcion = codigo switch
                {
                    "ND" => "Nota de Debito",
                    "NC" => "Nota de Credito",
                    "FS" => "Factura de Servicio",
                    _ => codigo
                }
            };
        }

        private static object CrearResumenCuenta(
            CuentaDatosResultadoDto cuenta,
            string origen
        )
        {
            return new
            {
                cuenta.cta_id,
                cuenta.cta_denominacion,
                cuenta.cta_domicilio,
                cuenta.cta_celu,
                cuenta.cta_email,
                cuenta.tdoc_id,
                cuenta.tdoc_desc,
                cuenta.cta_documento,
                cuenta.afip_id,
                cuenta.afip_desc,
                cuenta.tco_letra,
                origen,
                origen_desc = origen == "P" ? "Proveedor" : "Cliente"
            };
        }

        private (bool Ok, string Mensaje) ValidarSolicitudCalculo(
            NotaDebitoCreditoContextoSesion contexto,
            NotaDebitoCreditoCalcularRequest request)
        {
            if (request == null)
            {
                return (false, "No se recibieron datos para calcular.");
            }

            var coTipo = (request.CoTipo ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(coTipo))
            {
                return (false, "Debe seleccionar el tipo de operacion.");
            }

            if (!contexto.OperacionesPermitidas.Contains(coTipo))
            {
                return (false, "La cuenta seleccionada no permite emitir ese tipo de comprobante.");
            }

            if (coTipo == "NC" &&
                (string.IsNullOrWhiteSpace(request.TcoIdOri) || string.IsNullOrWhiteSpace(request.CmCompteOri)))
            {
                return (false, "Para Nota de Credito debe informar el comprobante origen.");
            }

            if (request.Conceptos == null || request.Conceptos.Count == 0)
            {
                return (false, "Debe cargar al menos un concepto.");
            }

            if (request.Conceptos.Any(x => string.IsNullOrWhiteSpace(x.Concepto)))
            {
                return (false, "Todos los conceptos deben tener una descripcion.");
            }

            if (request.Conceptos.Any(x => x.NetoGravado <= 0))
            {
                return (false, "El neto gravado debe ser mayor a cero.");
            }

            if (request.Conceptos.Any(x => x.Cantidad <= 0))
            {
                return (false, "La cantidad debe ser mayor a cero.");
            }

            if (request.Conceptos.Any(x => x.AlicuotaIva < 0))
            {
                return (false, "La alicuota de IVA no puede ser negativa.");
            }

            return (true, "OK");
        }

        private CalcularFilasReqDto CrearRequestCalculoFilas(
            NotaDebitoCreditoContextoSesion contexto,
            NotaDebitoCreditoCalcularRequest request,
            JArray productosJson,
            decimal totalCantidad,
            decimal totalControl)
        {
            var cajaActual = CajaActual;
            var cuenta = contexto.Cuenta;

            return new CalcularFilasReqDto
            {
                caja_id = cajaActual?.CajaId ?? string.Empty,
                usu_id = UserName ?? string.Empty,
                adm_id = cajaActual?.AdmId ?? AdministracionId,
                lp_id = !string.IsNullOrWhiteSpace(LP_Id)
                    ? LP_Id
                    : cajaActual?.Caja?.lp_id_min ?? string.Empty,
                caja_nro_proceso = cajaActual?.Caja?.caja_nro_proceso ?? string.Empty,
                caja_nro_cierre = cajaActual?.Caja?.caja_nro_cierre ?? string.Empty,
                cta_id = cuenta.cta_id,
                ctac_dto = cuenta.ctac_dto_operacion,
                ctc_id = cuenta.ctc_id ?? string.Empty,
                tco_letra = cuenta.tco_letra ?? string.Empty,
                tco_id = string.Empty,
                tco_id_ori = request.CoTipo?.Trim().ToUpperInvariant() == "NC"
                    ? (request.TcoIdOri ?? string.Empty).Trim()
                    : string.Empty,
                cm_compte_ori = request.CoTipo?.Trim().ToUpperInvariant() == "NC"
                    ? (request.CmCompteOri ?? string.Empty).Trim()
                    : string.Empty,
                afip_id = cuenta.afip_id ?? string.Empty,
                afip_desc = cuenta.afip_desc ?? string.Empty,
                tot_rows = (short)productosJson.Count,
                tot_cantidad = totalCantidad,
                tot_pvta = totalControl,
                json_p = productosJson.ToString(Formatting.None)
            };
        }

        private static object CrearJsonConcepto(
            NotaDebitoCreditoContextoSesion contexto,
            string coTipo,
            NotaDebitoCreditoConceptoRequest concepto,
            int item)
        {
            var cantidad = concepto.Cantidad <= 0 ? 1 : concepto.Cantidad;
            var condicionIva = concepto.AlicuotaIva > 0 ? "G" : "N";
            var iva = CalcularIvaManual(concepto.NetoGravado, concepto.AlicuotaIva);
            var total = (concepto.NetoGravado + iva + concepto.PercepcionIb + concepto.PercepcionIva) * cantidad;

            return new
            {
                p_id = string.Empty,
                p_id_barrado = string.Empty,
                p_desc = concepto.Concepto?.Trim() ?? string.Empty,
                p_pcosto = 0,
                p_pcosto_repo = 0,
                in_alicuota = 0,
                p_in = 0,
                iva_situacion = condicionIva,
                iva_alicuota = concepto.AlicuotaIva,
                ali_alicuota = concepto.AlicuotaIva,
                iva_consicion = condicionIva,
                p_iva = iva,
                po = false,
                po_limite = 0,
                p_pneto = concepto.NetoGravado,
                p_margen_imp = 0,
                p_margen_vig = 0,
                p_pvta = total,
                lp_prevision_tot = 0,
                lp_prevision_pin = 0,
                cantidad_tot = cantidad,
                p_pvta_tot = total,
                bultos = 0,
                cm_gravado = condicionIva == "G" ? concepto.NetoGravado * cantidad : 0,
                cm_no_gravado = condicionIva == "N" ? concepto.NetoGravado * cantidad : 0,
                cm_exento = 0,
                cm_iva = iva * cantidad,
                cm_ii = 0,
                cm_dto = 0,
                cm_dto_porc = 0,
                percepcion_ib = concepto.PercepcionIb * cantidad,
                percepcion_iva = concepto.PercepcionIva * cantidad,
                otros_campos = 0,
                cta_id = contexto.Cuenta.cta_id,
                pre_id = string.Empty,
                cpf_nro = string.Empty,
                cmb_p_id = string.Empty,
                cmd_cmb = string.Empty,
                cmd_cmb_id = string.Empty,
                cmd_cmb_dto = 0,
                cmd_cmb_cant = 0,
                cmd_cmb_desc = string.Empty,
                barre = string.Empty,
                co_tipo = coTipo?.Trim().ToUpperInvariant() ?? string.Empty,
                item
            };
        }

        private bool DebeImprimirComprobanteElectronico()
        {
            return string.Equals(
                CajaActual?.Facturacion.ToString(),
                "FE",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private static string NormalizarModoReporte(string? modo)
        {
            var valor = (modo ?? string.Empty).Trim().ToUpperInvariant();

            return valor == "IMPRESORA"
                ? "IMPRESORA"
                : "PANTALLA";
        }

        private static string CrearMensajeOperacionConfirmada(string coTipo, string tcoLetra, string cmCompte)
        {
            var descripcion = (coTipo ?? string.Empty).Trim().ToUpperInvariant() switch
            {
                "ND" => "Nota de Debito",
                "NC" => "Nota de Credito",
                "FS" => "Factura de Servicio",
                _ => "Operacion"
            };

            return $"{descripcion} {tcoLetra} Nro {cmCompte} emitida correctamente.";
        }
        private CajaOpeConfirmarReq CrearRequestConfirmacion(NotaDebitoCreditoContextoSesion contexto)
        {
            var cajaActual = CajaActual;
            var cuenta = contexto.Cuenta;

            return new CajaOpeConfirmarReq
            {
                caja_id = cajaActual?.CajaId ?? string.Empty,
                usu_id = UserName ?? string.Empty,
                adm_id = cajaActual?.AdmId ?? AdministracionId,
                lp_id = !string.IsNullOrWhiteSpace(LP_Id)
                    ? LP_Id
                    : cajaActual?.Caja?.lp_id_min ?? string.Empty,
                caja_nro_proceso = cajaActual?.Caja?.caja_nro_proceso ?? string.Empty,
                caja_nro_cierre = cajaActual?.Caja?.caja_nro_cierre,
                cta_id = cuenta.cta_id,
                ctac_dto = cuenta.ctac_dto_operacion,
                co_tipo = contexto.CoTipo,
                ctc_id = cuenta.ctc_id ?? string.Empty,
                tco_letra = cuenta.tco_letra ?? string.Empty,
                tco_id_ori = contexto.TcoIdOri,
                cm_compte_ori = contexto.CmCompteOri,
                cm_repetido_ori = contexto.CmRepetidoOri,
                afip_id = cuenta.afip_id ?? string.Empty,
                tdoc_id = cuenta.tdoc_id ?? string.Empty,
                cta_documento = cuenta.cta_documento ?? string.Empty,
                cta_denominacion = cuenta.cta_denominacion ?? string.Empty,
                cta_domicilio = cuenta.cta_domicilio ?? string.Empty,
                ve_id = cuenta.ve_id ?? string.Empty,
                json_p = contexto.JsonProductosCalculado,
                json_subtotal = contexto.JsonSubtotal,
                json_sorteo = "[]",
                json_valores = "[]",
                json_cancela = "[]",
                json_union = "[]"
            };
        }

        private static decimal CalcularIvaManual(decimal neto, decimal alicuota)
        {
            return Math.Round(neto * alicuota / 100m, 2, MidpointRounding.AwayFromZero);
        }

        private static void AgregarJson(JArray destino, string? json, object? fallback)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                if (fallback != null)
                {
                    destino.Add(JObject.FromObject(fallback));
                }

                return;
            }

            var token = JToken.Parse(json);
            if (token is JArray array)
            {
                foreach (var item in array)
                {
                    destino.Add(item);
                }
            }
            else
            {
                destino.Add(token);
            }
        }

        private static List<object> CrearResumenSubtotales(string jsonSubtotal, decimal totalManual)
        {
            var resumen = new List<object>();
            if (!string.IsNullOrWhiteSpace(jsonSubtotal))
            {
                try
                {
                    var subtotales = JsonConvert.DeserializeObject<List<FactSubtotalJsonDto>>(jsonSubtotal) ?? [];
                    resumen.AddRange(subtotales.Select(x => new
                    {
                        x.orden,
                        x.tipo,
                        x.concepto,
                        x.@base,
                        x.alicuota,
                        x.importe,
                        x.id_aux
                    }));
                }
                catch
                {
                    resumen.Clear();
                }
            }

            if (!resumen.Any())
            {
                resumen.Add(new
                {
                    orden = 1,
                    tipo = "TOTAL",
                    concepto = "TOTAL",
                    @base = 0m,
                    alicuota = 0m,
                    importe = totalManual,
                    id_aux = string.Empty
                });
            }

            return resumen;
        }

        private sealed class NotaDebitoCreditoContextoSesion
        {
            public CuentaDatosResultadoDto Cuenta { get; set; } = new();
            public string Origen { get; set; } = string.Empty;
            public List<string> OperacionesPermitidas { get; set; } = new();
            public DateTime FechaCreacionUtc { get; set; }
            public string CoTipo { get; set; } = string.Empty;
            public string TcoIdOri { get; set; } = string.Empty;
            public string CmCompteOri { get; set; } = string.Empty;
            public string CmRepetidoOri { get; set; } = string.Empty;
            public string JsonProductosOriginal { get; set; } = "[]";
            public string JsonProductosCalculado { get; set; } = "[]";
            public string JsonSubtotal { get; set; } = "[]";
            public string JsonSorteo { get; set; } = "[]";
            public decimal Total { get; set; }
            public DateTime? FechaUltimoCalculoUtc { get; set; }
        }

        public sealed class NotaDebitoCreditoCalcularRequest
        {
            public string CoTipo { get; set; } = string.Empty;
            public string TcoIdOri { get; set; } = string.Empty;
            public string CmCompteOri { get; set; } = string.Empty;
            public string CmRepetidoOri { get; set; } = string.Empty;
            public List<NotaDebitoCreditoConceptoRequest> Conceptos { get; set; } = [];
        }

        public sealed class NotaDebitoCreditoConceptoRequest
        {
            public string Concepto { get; set; } = string.Empty;
            public decimal NetoGravado { get; set; }
            public decimal AlicuotaIva { get; set; } = 21m;
            public decimal PercepcionIb { get; set; }
            public decimal PercepcionIva { get; set; }
            public decimal Cantidad { get; set; } = 1m;
        }
    }
}




