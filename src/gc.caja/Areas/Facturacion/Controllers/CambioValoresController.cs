using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class CambioValoresController : ControladorBaseCaja
    {
        private const string MODULO = "CambioValores";
        private const string MODULO_DESC = "Cambios e Ingresos de Valores";
        private const string CO_TIPO_CONSULTA = "CV";

        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly ICheckoutServicio _checkoutServicio;
        private readonly ICajaServicio _cajaServicio;

        public CambioValoresController(
            IOptions<AppSettings> options,
            ILogger<CambioValoresController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            ICheckoutServicio checkoutServicio,
            ICajaServicio cajaServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _checkoutServicio = checkoutServicio;
            _cajaServicio = cajaServicio;
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
                TempData["Error"] = "No se encontraron datos validos de caja para iniciar cambios e ingresos de valores.";
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            ViewBag.Modulo = MODULO;
            ViewBag.ModuloDesc = MODULO_DESC;
            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;
            ViewBag.Proceso = caja.Caja.caja_nro_proceso ?? string.Empty;
            ViewBag.Cierre = caja.Caja.caja_nro_cierre ?? string.Empty;

            return View();
        }

        [HttpPost]
        public JsonResult ValidacionInicial()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { success = false, message = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var caja = CajaActual;
                if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    return Json(new { success = false, message = "Los datos de caja no estan disponibles." });
                }

                var (esValido, mensajeValidacion) = _cajaInitServicio.ValidarDatosIniciales(caja);
                if (!esValido)
                {
                    _logger?.LogWarning(
                        "Cambio valores: validacion inicial fallida. CajaId={CajaId}; Usuario={Usuario}; Motivo={Motivo}",
                        caja.CajaId,
                        UserName,
                        mensajeValidacion);

                    return Json(new { success = false, message = mensajeValidacion });
                }

                _logger?.LogInformation(
                    "Cambio valores: validacion inicial exitosa. CajaId={CajaId}; Usuario={Usuario}",
                    caja.CajaId,
                    UserName);

                return Json(new { success = true, message = "Caja validada correctamente." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error inesperado en ValidacionInicial. Usuario={Usuario}", UserName);
                return Json(new { success = false, message = "Error interno al validar los datos de caja." });
            }
        }

        [HttpPost]
        public JsonResult ValidarClienteRegistrado([FromBody] string cta_id)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var cliente = ClienteActual;
                var ctaId = (cta_id ?? string.Empty).Trim();
                var clienteCtaId = (cliente?.cta_id ?? string.Empty).Trim();
                var origen = (cliente?.Origen ?? string.Empty).Trim().ToUpperInvariant();

                if (string.IsNullOrWhiteSpace(ctaId) && !string.IsNullOrWhiteSpace(clienteCtaId))
                {
                    ctaId = clienteCtaId;
                }

                if (string.IsNullOrWhiteSpace(ctaId) || origen == "F")
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "Este modulo requiere un Cliente Registrado con identificador de cuenta valido." });
                }

                return Json(new { ok = true, hayDatos = true, mensaje = "Cliente registrado validado." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error validando cliente registrado. Usuario={Usuario}", UserName);
                return Json(new { ok = false, hayDatos = false, mensaje = "No se pudo validar el cliente seleccionado." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerMediosPago()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var request = new ValoresMPReqDto
                {
                    co_tipo = CO_TIPO_CONSULTA,
                    cta_id = ctaId,
                    adm_id = AdministracionId ?? string.Empty
                };

                _logger?.LogInformation(
                    "Cambio valores: consultando medios habilitados. Cta={Cta}; CoTipo={CoTipo}; Adm={Adm}; Usuario={Usuario}",
                    request.cta_id,
                    request.co_tipo,
                    request.adm_id,
                    UserName);

                var respuesta = await _checkoutServicio.ObtenerValoresMP(request, token);
                var lista = respuesta?.ListaEntidad ?? [];

                _logger?.LogInformation(
                    "Cambio valores: respuesta medios habilitados. Ok={Ok}; Registros={Registros}; Mensaje={Mensaje}",
                    respuesta?.Ok,
                    lista.Count,
                    respuesta?.Mensaje);

                return Json(new
                {
                    ok = respuesta?.Ok == true,
                    mensaje = respuesta?.Ok == true ? "OK" : respuesta?.Mensaje,
                    datos = lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error obteniendo medios habilitados. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener los medios de pago habilitados." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerInstrumentos([FromBody] ValoresInsReqDto request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request == null || string.IsNullOrWhiteSpace(request.tcf_id))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un medio de pago." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                request.co_tipo = CO_TIPO_CONSULTA;
                request.cta_id = ctaId;
                request.adm_id = AdministracionId ?? string.Empty;

                _logger?.LogInformation(
                    "Cambio valores: consultando instrumentos. Cta={Cta}; Tcf={Tcf}; CoTipo={CoTipo}; Adm={Adm}; Usuario={Usuario}",
                    request.cta_id,
                    request.tcf_id,
                    request.co_tipo,
                    request.adm_id,
                    UserName);

                var respuesta = await _checkoutServicio.ObtenerValoresIns(request, token);
                var lista = respuesta?.ListaEntidad ?? [];

                _logger?.LogInformation(
                    "Cambio valores: respuesta instrumentos. Ok={Ok}; Registros={Registros}; Mensaje={Mensaje}",
                    respuesta?.Ok,
                    lista.Count,
                    respuesta?.Mensaje);

                return Json(new
                {
                    ok = respuesta?.Ok == true,
                    mensaje = respuesta?.Ok == true ? "OK" : respuesta?.Mensaje,
                    datos = lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error obteniendo instrumentos. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener los instrumentos habilitados." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarOperacion([FromBody] CambioValoresConfirmarRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (!TryObtenerClienteRegistrado(out var ctaId, out var mensajeCliente))
                {
                    return Json(new { ok = false, mensaje = mensajeCliente });
                }

                var valores = request?.Valores ?? [];
                if (valores.Count == 0)
                {
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un valor antes de finalizar." });
                }

                var tipo = string.Equals(request?.Tipo, "IV", StringComparison.OrdinalIgnoreCase) ? "IV" : "CV";
                var valoresConfirmacion = PrepararValoresConfirmacion(valores, tipo);
                var totalValoresIngresados = valores.Sum(x => Math.Abs(x.rb_importe));
                var jsonValores = SerializarValoresConfirmacion(valoresConfirmacion);

                var cajaActual = CajaActual;
                if (cajaActual?.Caja == null || string.IsNullOrWhiteSpace(cajaActual.CajaId))
                {
                    return Json(new { ok = false, mensaje = "No se encontraron datos validos de caja para confirmar la operacion." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var requestConfirmacion = CrearRequestConfirmacion(ctaId, tipo, jsonValores);

                _logger?.LogInformation(
                    "Cambio valores: validando PV. Cta={Cta}; Tipo={Tipo}; Registros={Registros}; Total={Total}; Usuario={Usuario}",
                    ctaId,
                    tipo,
                    valoresConfirmacion.Count,
                    totalValoresIngresados,
                    UserName);

                var validacionPV = await ValidarEstadoPuntoVenta(
                    cajaServicio: _cajaServicio,
                    cajaId: cajaActual.CajaId ?? string.Empty,
                    ctrlId: cajaActual.Caja.ctrl_id ?? string.Empty,
                    nroProceso: requestConfirmacion.caja_nro_proceso,
                    nroCierre: requestConfirmacion.caja_nro_cierre,
                    tipoLlamada: "F");

                if (!validacionPV.PuedeContinuar)
                {
                    _logger?.LogError(
                        "Cambio valores: validacion PV bloqueada. Resultado={Resultado}; Mensaje={Mensaje}",
                        validacionPV.Resultado,
                        validacionPV.Mensaje);

                    return Json(new
                    {
                        ok = false,
                        mensaje = validacionPV.Mensaje,
                        error_tipo = "estado_pv",
                        ctrl_id = validacionPV.CtrlId,
                        resultado_pv = validacionPV.Resultado
                    });
                }

                requestConfirmacion.caea = cajaActual.Caja.ctrl_id == "-1" && validacionPV.Resultado == 1;

                _logger?.LogInformation(
                    "Cambio valores: request confirmacion. Cta={Cta}; Tipo={Tipo}; CAEA={Caea}; JsonValores={JsonValores}; Request={Request}",
                    ctaId,
                    tipo,
                    requestConfirmacion.caea,
                    jsonValores,
                    JsonConvert.SerializeObject(requestConfirmacion));

                var resultado = await _checkoutServicio.FinalizarCompra(requestConfirmacion, token);

                _logger?.LogInformation(
                    "Cambio valores: response confirmacion. Ok={Ok}; Mensaje={Mensaje}; Response={Response}",
                    resultado?.Ok,
                    resultado?.Mensaje,
                    JsonConvert.SerializeObject(resultado));

                if (resultado == null || !resultado.Ok || resultado.Entidad == null)
                {
                    return Json(new { ok = false, mensaje = resultado?.Mensaje ?? "No se pudo confirmar la operacion." });
                }

                var respuesta = resultado.Entidad;
                if (respuesta.resultado != 0)
                {
                    return Json(new
                    {
                        ok = false,
                        resultado = respuesta.resultado,
                        resultado_id = respuesta.resultado_id,
                        mensaje = string.IsNullOrWhiteSpace(respuesta.resultado_msj) ? resultado.Mensaje : respuesta.resultado_msj
                    });
                }

                var comprobanteConfirmacion = ObtenerComprobanteConfirmacion(respuesta.resultado_id);

                return Json(new
                {
                    ok = true,
                    resultado = respuesta.resultado,
                    resultado_id = respuesta.resultado_id,
                    mensaje = ConstruirMensajeExito(tipo, totalValoresIngresados, respuesta.resultado_msj, comprobanteConfirmacion),
                    comprobante = comprobanteConfirmacion,
                    tipo,
                    total = totalValoresIngresados
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error preparando confirmacion. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudo preparar la confirmacion de valores." });
            }
        }

        [HttpGet]
        public async Task<JsonResult> ObtenerBancos()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var respuesta = await _checkoutServicio.GetBancoChequeLista(token);
                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Ok ? "OK" : respuesta.Mensaje,
                    datos = respuesta.ListaEntidad ?? []
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Cambio valores: error obteniendo bancos. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener los bancos." });
            }
        }

        private CajaOpeConfirmarReq CrearRequestConfirmacion(string ctaId, string tipo, string jsonValores)
        {
            var cajaActual = CajaActual;
            var cliente = ClienteActual;

            return new CajaOpeConfirmarReq
            {
                caja_id = cajaActual?.CajaId ?? string.Empty,
                usu_id = UserName ?? string.Empty,
                adm_id = cajaActual?.AdmId ?? AdministracionId,
                lp_id = !string.IsNullOrWhiteSpace(LP_Id) ? LP_Id : cajaActual?.Caja?.lp_id_min ?? string.Empty,
                caja_nro_proceso = cajaActual?.Caja?.caja_nro_proceso ?? string.Empty,
                caja_nro_cierre = cajaActual?.Caja?.caja_nro_cierre,
                usu_id_autoriza = ObtenerUsuarioAutoriza(),
                cta_id = ctaId,
                ctac_dto = 0m,
                co_tipo = tipo,
                ctc_id = string.Empty,
                tco_letra = string.Empty,
                tco_id_ori = string.Empty,
                cm_compte_ori = string.Empty,
                cm_repetido_ori = string.Empty,
                afip_id = string.Empty,
                tdoc_id = string.Empty,
                cta_documento = string.Empty,
                cta_denominacion = string.Empty,
                cta_domicilio = string.Empty,
                ve_id = string.Empty,
                json_p = "[]",
                json_valores = jsonValores,
                json_cancela = "[]",
                json_union = "[]",
                json_subtotal = "[]",
                json_sorteo = "[]"
            };
        }

        private string ObtenerUsuarioAutoriza()
        {
            // Punto de extension: si se activa autorizacion remota, aqui se debe tomar el usuario autorizado.
            return UserName ?? string.Empty;
        }

        private static List<Json_Valor> PrepararValoresConfirmacion(List<Json_Valor> valores, string tipo)
        {
            var valoresPositivos = (valores ?? [])
                .Where(x => x.rb_importe > 0)
                .Select((x, index) => NormalizarValor(x, index + 1, x.rb_importe))
                .ToList();

            if (string.Equals(tipo, "CV", StringComparison.OrdinalIgnoreCase))
            {
                var total = valoresPositivos.Sum(x => x.rb_importe);
                valoresPositivos.Add(new Json_Valor
                {
                    rb_nro_valor = (valoresPositivos.Count + 1).ToString().PadLeft(3, '0'),
                    ins_id = "PES",
                    rb_dato1_valor = string.Empty,
                    rb_dato2_valor = string.Empty,
                    rb_dato3_valor = string.Empty,
                    rb_opcion_cuota = "0",
                    rb_cupon_manual = "N",
                    rb_ch_dif = "N",
                    rb_fecha_valor = DateTime.Today,
                    rb_importe = -Math.Abs(total),
                    rb_rec = 0m,
                    rb_aux = 0m,
                    rb_estado = "A",
                    id_externo = string.Empty
                });
            }

            return valoresPositivos;
        }

        private static Json_Valor NormalizarValor(Json_Valor valor, int indice, decimal importe)
        {
            return new Json_Valor
            {
                rb_nro_valor = indice.ToString().PadLeft(3, '0'),
                ins_id = valor.ins_id ?? string.Empty,
                rb_dato1_valor = valor.rb_dato1_valor ?? string.Empty,
                rb_dato2_valor = valor.rb_dato2_valor ?? string.Empty,
                rb_dato3_valor = valor.rb_dato3_valor ?? string.Empty,
                rb_opcion_cuota = string.IsNullOrWhiteSpace(valor.rb_opcion_cuota) ? "0" : valor.rb_opcion_cuota,
                rb_cupon_manual = string.IsNullOrWhiteSpace(valor.rb_cupon_manual) ? "N" : valor.rb_cupon_manual,
                rb_ch_dif = string.IsNullOrWhiteSpace(valor.rb_ch_dif) ? "N" : valor.rb_ch_dif,
                rb_fecha_valor = valor.rb_fecha_valor == default ? DateTime.Today : valor.rb_fecha_valor,
                rb_importe = Math.Abs(importe),
                rb_rec = valor.rb_rec,
                rb_aux = valor.rb_aux,
                rb_estado = "A",
                id_externo = valor.id_externo ?? string.Empty
            };
        }

        private static string SerializarValoresConfirmacion(List<Json_Valor> valores)
        {
            var valoresSp = (valores ?? [])
                .Select(valor => new
                {
                    rb_nro_valor = valor.rb_nro_valor,
                    ins_id = valor.ins_id,
                    rb_dato1_valor = valor.rb_dato1_valor ?? string.Empty,
                    rb_dato2_valor = valor.rb_dato2_valor ?? string.Empty,
                    rb_dato3_valor = valor.rb_dato3_valor ?? string.Empty,
                    rb_opcion_cuota = string.IsNullOrWhiteSpace(valor.rb_opcion_cuota) ? "0" : valor.rb_opcion_cuota,
                    rb_cupon_manual = string.IsNullOrWhiteSpace(valor.rb_cupon_manual) ? "N" : valor.rb_cupon_manual,
                    rb_ch_dif = string.IsNullOrWhiteSpace(valor.rb_ch_dif) ? "N" : valor.rb_ch_dif,
                    rb_fecha_valor = (valor.rb_fecha_valor == default ? DateTime.Today : valor.rb_fecha_valor).ToString("yyyy-MM-dd"),
                    rb_importe = valor.rb_importe,
                    rb_rec = valor.rb_rec,
                    rb_aux = valor.rb_aux,
                    rb_estado = string.IsNullOrWhiteSpace(valor.rb_estado) ? "A" : valor.rb_estado,
                    id_externo = valor.id_externo ?? string.Empty
                })
                .ToList();

            return JsonConvert.SerializeObject(valoresSp);
        }
        private static string ObtenerComprobanteConfirmacion(string? resultadoId)
        {
            if (string.IsNullOrWhiteSpace(resultadoId))
            {
                return string.Empty;
            }

            try
            {
                var comprobantes = JsonConvert.DeserializeObject<List<CambioValoresComprobanteResultado>>(resultadoId);
                return comprobantes?.FirstOrDefault()?.rb_compte?.Trim() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string ConstruirMensajeExito(string tipo, decimal total, string? mensajeSp, string? comprobante)
        {
            var comprobanteTexto = string.IsNullOrWhiteSpace(comprobante)
                ? string.Empty
                : $" Comprobante: {comprobante.Trim()}.";

            if (!string.IsNullOrWhiteSpace(mensajeSp) && !string.Equals(mensajeSp.Trim(), "OK", StringComparison.OrdinalIgnoreCase))
            {
                return $"{mensajeSp.Trim()}{comprobanteTexto}";
            }

            var descripcion = string.Equals(tipo, "IV", StringComparison.OrdinalIgnoreCase)
                ? "Ingreso de valores confirmado correctamente."
                : "Cambio de valores confirmado correctamente.";

            return $"{descripcion}{comprobanteTexto} Total operado: $ {total:N2}.";
        }

        private sealed class CambioValoresComprobanteResultado
        {
            public string rb_compte { get; set; } = string.Empty;
        }

        private bool TryObtenerClienteRegistrado(out string ctaId, out string mensaje)
        {
            var cliente = ClienteActual;
            ctaId = (cliente?.cta_id ?? string.Empty).Trim();
            var origen = (cliente?.Origen ?? string.Empty).Trim().ToUpperInvariant();

            if (cliente == null || string.IsNullOrWhiteSpace(ctaId) || origen == "F")
            {
                mensaje = "Debe seleccionar un Cliente Registrado con identificador de cuenta valido.";
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

        public sealed class CambioValoresConfirmarRequest
        {
            public string Tipo { get; set; } = "CV";
            public List<Json_Valor> Valores { get; set; } = [];
        }
    }
}



