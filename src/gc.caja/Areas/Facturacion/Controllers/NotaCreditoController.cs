using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.Models.NotaCredito;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    //[Authorize]
    public class NotaCreditoController : ControladorBaseCaja
    {
        private readonly INotaCreditoServicio _ncServicio;
        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly AppSettings _appSettings;

        private const string SessionKeyNcDevolucionCandidatos = "NCDEV_CANDIDATOS_COMPROBANTE";

        private const string SessionKeyNcDevolucionContexto = "NCDEV_CONTEXTO";

        public NotaCreditoController(
            IOptions<AppSettings> options,
            ILogger<NotaCreditoController> logger,
            IHttpContextAccessor httpContext,
            INotaCreditoServicio ncServicio,
            ICajaInitServicio cajaInitServicio)
            : base(options, httpContext, logger)
        {
            _ncServicio = ncServicio;
            _appSettings = options.Value;
            _cajaInitServicio = cajaInitServicio;
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
                    "No se encontraron datos válidos de caja para iniciar la Nota de Crédito por Devolución.";

                return RedirectToAction(
                    "Index",
                    "Home",
                    new { area = string.Empty }
                );
            }

            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;

            return View();
        }

        /// <summary>
        /// Valida que la caja tenga un contexto operativo válido antes
        /// de permitir el acceso al módulo de NC por Devolución.
        /// </summary>
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
                        message = "La sesión ha expirado. Vuelva a iniciar sesión."
                    });
                }

                var caja = CajaActual;

                if (caja == null)
                {
                    _logger?.LogWarning(
                        "NC Devolución: CajaActual es null. Usuario: {Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message =
                            "No se ha configurado una caja para esta estación."
                    });
                }

                if (string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    _logger?.LogWarning(
                        "NC Devolución: CajaId vacío. Usuario: {Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message =
                            "La caja no tiene un identificador válido."
                    });
                }

                if (caja.Caja == null)
                {
                    _logger?.LogWarning(
                        "NC Devolución: datos de Caja no disponibles. CajaId: {CajaId}, Usuario: {Usuario}",
                        caja.CajaId,
                        UserName
                    );

                    return Json(new
                    {
                        success = false,
                        message =
                            "Los datos de la caja no están disponibles. Cierre sesión y vuelva a abrir la caja."
                    });
                }

                var (esValido, mensajeValidacion) =
                    _cajaInitServicio.ValidarDatosIniciales(caja);

                if (!esValido)
                {
                    _logger?.LogWarning(
                        "NC Devolución: validación de caja fallida. CajaId: {CajaId}, Motivo: {Motivo}",
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
                    "NC Devolución: validación de caja exitosa. CajaId: {CajaId}, Usuario: {Usuario}",
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
                    "NC Devolución: error inesperado en ValidacionInicial. Usuario: {Usuario}",
                    UserName
                );

                return Json(new
                {
                    success = false,
                    message =
                        "Error interno al validar los datos de la caja. Contacte al administrador."
                });
            }
        }

        /// <summary>
        /// Obtiene los tipos de comprobante de venta que pueden utilizarse
        /// como comprobante origen de una Nota de Crédito por Devolución.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ObtenerTiposComprobante()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "Sesión expirada."
                });
            }

            var token = TokenCookie;

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger?.LogWarning(
                    "No se pudo obtener tipos de comprobante para NC por devolución: token inexistente."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión actual no posee un token válido."
                });
            }

            var afipId = string.IsNullOrWhiteSpace(
                _appSettings.NotaCreditoDevolucionAfipId)
                ? "%"
                : _appSettings.NotaCreditoDevolucionAfipId.Trim();

            var optId = string.IsNullOrWhiteSpace(
                _appSettings.NotaCreditoDevolucionOptId)
                ? "VE"
                : _appSettings.NotaCreditoDevolucionOptId.Trim();

            try
            {
                var resultado = await _ncServicio.GetTipoComprobante(
                    afipId,
                    optId,
                    token
                );

                if (resultado == null || !resultado.Ok)
                {
                    var mensaje = resultado?.Mensaje
                        ?? "No fue posible obtener los tipos de comprobante.";

                    _logger?.LogWarning(
                        "Error obteniendo tipos de comprobante para NC por devolución. Mensaje: {Mensaje}",
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
                        "No se encontraron tipos de comprobante para NC por devolución. afip_id={AfipId}, opt_id={OptId}",
                        afipId,
                        optId
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje = "No se encontraron tipos de comprobante habilitados para devolución."
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
                    "Error inesperado al obtener tipos de comprobante para NC por devolución."
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "Ocurrió un error al obtener los tipos de comprobante."
                });
            }
        }

        /// <summary>
        /// Valida el comprobante original ingresado por el cajero.
        ///
        /// El resultado se determina por cantidad de registros:
        /// 0 = no existe o no está habilitado.
        /// 1 = se evalúan bloqueos y se guarda contexto.
        /// >1 = se guardan candidatos en sesión para selección posterior.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> ValidarComprobanteOrigen(
            [FromBody] ValidarComprobanteOrigenRequest request)
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            if (request == null)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No se recibieron datos para validar el comprobante."
                });
            }

            LimpiarEstadoTemporalDevolucion();

            if (!TryObtenerDatosCajaParaValidacion(
                out var cajaNroProceso,
                out var cajaNroCierre,
                out var mensajeCaja))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = mensajeCaja
                });
            }

            var tcoId = request.TcoId?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(tcoId))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "Debe seleccionar un tipo de comprobante."
                });
            }

            if (!TryNormalizarComprobante(
                request.PuntoVenta,
                request.Numero,
                out var cmCompte,
                out var mensajeComprobante))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = mensajeComprobante
                });
            }

            var token = TokenCookie;

            if (string.IsNullOrWhiteSpace(token))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión actual no posee un token válido."
                });
            }

            try
            {
                var requestApi = new NCValidaRequestDto
                {
                    tco_id = tcoId,
                    cm_compte = cmCompte,
                    caja_nro_proceso = cajaNroProceso,
                    caja_nro_cierre = cajaNroCierre
                };

                _logger?.LogInformation(
                    "NC Devolución: validando comprobante origen. Tipo={Tipo}, Comprobante={Comprobante}, CajaProceso={Proceso}, CajaCierre={Cierre}",
                    requestApi.tco_id,
                    requestApi.cm_compte,
                    requestApi.caja_nro_proceso,
                    requestApi.caja_nro_cierre
                );

                var resultado = await _ncServicio.ValidarNC(
                    requestApi,
                    token
                );

                if (resultado == null || !resultado.Ok)
                {
                    var mensaje = resultado?.Mensaje
                        ?? "No fue posible validar el comprobante original.";

                    _logger?.LogWarning(
                        "NC Devolución: error validando comprobante. Tipo={Tipo}, Comprobante={Comprobante}, Mensaje={Mensaje}",
                        tcoId,
                        cmCompte,
                        mensaje
                    );

                    return Json(new
                    {
                        ok = false,
                        mensaje
                    });
                }

                var candidatos = resultado.ListaEntidad ?? [];

                if (candidatos.Count == 0)
                {
                    _logger?.LogInformation(
                        "NC Devolución: comprobante inexistente o no habilitado. Tipo={Tipo}, Comprobante={Comprobante}",
                        tcoId,
                        cmCompte
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "COMPROBANTE_NO_ENCONTRADO",
                        mensaje =
                            "No se encontró un comprobante habilitado para generar una Nota de Crédito por Devolución."
                    });
                }

                GuardarCandidatosComprobante(candidatos);

                if (candidatos.Count > 1)
                {
                    _logger?.LogInformation(
                        "NC Devolución: se encontraron {Cantidad} comprobantes repetidos. Tipo={Tipo}, Comprobante={Comprobante}",
                        candidatos.Count,
                        tcoId,
                        cmCompte
                    );

                    var resumenCandidatos = candidatos
                        .Select((item, indice) =>
                            CrearResumenCandidato(item, indice))
                        .ToList();

                    return Json(new
                    {
                        ok = true,
                        requiereSeleccion = true,
                        mensaje =
                            "Se encontraron varios comprobantes con el mismo número. Seleccione el comprobante correcto.",
                        candidatos = resumenCandidatos
                    });
                }

                return FinalizarSeleccionComprobante(candidatos[0]);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: error inesperado al validar comprobante origen."
                );

                return Json(new
                {
                    ok = false,
                    mensaje =
                        "Ocurrió un error al validar el comprobante original. Intente nuevamente."
                });
            }
        }

        /// <summary>
        /// Confirma la selección de un comprobante cuando existían repeticiones.
        ///
        /// El navegador solamente informa el índice mostrado.
        /// El comprobante real se recupera desde sesión.
        /// </summary>
        [HttpPost]
        public IActionResult SeleccionarComprobanteRepetido(
            [FromBody] SeleccionarComprobanteRepetidoRequest request)
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            if (request == null || request.Indice < 0)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La selección de comprobante no es válida."
                });
            }

            var candidatos = ObtenerCandidatosComprobante();

            if (candidatos.Count == 0)
            {
                return Json(new
                {
                    ok = false,
                    codigo = "CANDIDATOS_EXPIRADOS",
                    mensaje =
                        "La selección de comprobantes ha expirado. Vuelva a realizar la búsqueda."
                });
            }

            if (request.Indice >= candidatos.Count)
            {
                _logger?.LogWarning(
                    "NC Devolución: se recibió un índice inválido para comprobante repetido. Indice={Indice}, Cantidad={Cantidad}",
                    request.Indice,
                    candidatos.Count
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "La selección de comprobante no corresponde a la búsqueda actual."
                });
            }

            return FinalizarSeleccionComprobante(candidatos[request.Indice]);
        }

        /// <summary>
        /// Guarda la modalidad inicial de carga del detalle de devolución.
        ///
        /// true  = cargar todo el detalle original.
        /// false = carga manual de productos.
        ///
        /// No invoca todavía SPGECO_CAJA_NC_B_Producto.
        /// </summary>
        [HttpPost]
        public IActionResult DefinirModalidadCargaInicial(
            [FromBody] DefinirModalidadCargaInicialRequest request)
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            if (request == null)
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "No se recibió la modalidad de carga."
                });
            }

            try
            {
                var contexto = ObtenerContextoDevolucion();

                if (contexto == null ||
                    contexto.ComprobanteOrigen == null ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.tco_id) ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.cm_compte))
                {
                    _logger.LogWarning(
                        "NC Devolución: no existe contexto válido al definir modalidad de carga. Usuario={Usuario}",
                        UserName
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "CONTEXTO_NO_DISPONIBLE",
                        mensaje =
                            "No existe un comprobante original validado. Vuelva a realizar la búsqueda."
                    });
                }

                contexto.CargarTodoDetalle = request.CargarTodoDetalle;

                GuardarContextoDevolucion(contexto);

                var modalidad = request.CargarTodoDetalle
                    ? "TODOS"
                    : "MANUAL";

                _logger.LogInformation(
                    "NC Devolución: modalidad inicial definida. Modalidad={Modalidad}, Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, Usuario={Usuario}",
                    modalidad,
                    contexto.ComprobanteOrigen.tco_id,
                    contexto.ComprobanteOrigen.cm_compte,
                    contexto.ComprobanteOrigen.cm_repetido,
                    UserName
                );

                return Json(new
                {
                    ok = true,
                    modalidad,
                    cargarTodoDetalle = request.CargarTodoDetalle,
                    mensaje = request.CargarTodoDetalle
                        ? "Se cargará todo el detalle del comprobante original."
                        : "La devolución se realizará mediante carga manual de productos."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "NC Devolución: error al definir modalidad inicial de carga. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje =
                        "No fue posible guardar la modalidad de carga seleccionada."
                });
            }
        }

        /// <summary>
        /// Carga todo el detalle del comprobante original para una NC por Devolución.
        ///
        /// Sólo puede ejecutarse cuando la modalidad inicial fue definida como TODOS.
        /// Internamente utiliza:
        /// valor = "T"
        /// cantidad = 1
        /// json_p = "[]"
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CargarDetalleCompleto()
        {
            var correlationId = Guid.NewGuid().ToString("N");

            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                _logger.LogWarning(
                    "NC Devolución: sesión expirada al cargar detalle completo. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "SESION_EXPIRADA",
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            try
            {
                var contexto = ObtenerContextoDevolucion();

                if (contexto == null ||
                    contexto.ComprobanteOrigen == null ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.tco_id) ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.cm_compte))
                {
                    _logger.LogWarning(
                        "NC Devolución: no existe contexto válido para carga total. Usuario={Usuario}, CorrelationId={CorrelationId}",
                        UserName,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "CONTEXTO_NO_DISPONIBLE",
                        mensaje =
                            "No existe un comprobante original validado. Vuelva a realizar la búsqueda."
                    });
                }

                if (!contexto.CargarTodoDetalle.HasValue)
                {
                    _logger.LogWarning(
                        "NC Devolución: modalidad de carga no definida. Tipo={Tipo}, Comprobante={Comprobante}, CorrelationId={CorrelationId}",
                        contexto.ComprobanteOrigen.tco_id,
                        contexto.ComprobanteOrigen.cm_compte,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "MODALIDAD_NO_DEFINIDA",
                        mensaje =
                            "Debe definir si desea cargar todo el detalle o realizar una carga manual."
                    });
                }

                if (contexto.CargarTodoDetalle != true)
                {
                    _logger.LogWarning(
                        "NC Devolución: se intentó carga total con modalidad manual. Tipo={Tipo}, Comprobante={Comprobante}, CorrelationId={CorrelationId}",
                        contexto.ComprobanteOrigen.tco_id,
                        contexto.ComprobanteOrigen.cm_compte,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "MODALIDAD_MANUAL",
                        mensaje =
                            "La operación fue configurada para carga manual de productos."
                    });
                }

                var token = TokenCookie;

                if (string.IsNullOrWhiteSpace(token))
                {
                    _logger.LogWarning(
                        "NC Devolución: token inexistente al cargar detalle completo. CorrelationId={CorrelationId}",
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "TOKEN_INVALIDO",
                        mensaje = "La sesión actual no posee un token válido."
                    });
                }

                var comprobante = contexto.ComprobanteOrigen;

                var admId = comprobante.adm_id?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(admId))
                {
                    _logger.LogWarning(
                        "NC Devolución: comprobante origen sin adm_id. Tipo={Tipo}, Comprobante={Comprobante}, CorrelationId={CorrelationId}",
                        comprobante.tco_id,
                        comprobante.cm_compte,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "ADM_ORIGEN_NO_DISPONIBLE",
                        mensaje =
                            "El comprobante original no posee una administración válida para cargar su detalle."
                    });
                }

                var requestApi = new NCProductoBuscarRequestDto
                {
                    tco_id = comprobante.tco_id.Trim(),
                    cm_compte = comprobante.cm_compte.Trim(),
                    cm_repetido = (comprobante.cm_repetido ?? 0)
                        .ToString(CultureInfo.InvariantCulture),
                    adm_id = admId,
                    valor = "T",
                    cantidad = 1m,
                    json_p = "[]"
                };

                _logger.LogInformation(
                    "NC Devolución: iniciando carga total de detalle. Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, Adm={AdmId}, Valor={Valor}, CorrelationId={CorrelationId}",
                    requestApi.tco_id,
                    requestApi.cm_compte,
                    requestApi.cm_repetido,
                    requestApi.adm_id,
                    requestApi.valor,
                    correlationId
                );

                var resultado = await _ncServicio.BuscarProducto(
                    requestApi,
                    token
                );

                if (resultado == null || !resultado.Ok)
                {
                    var mensaje = resultado?.Mensaje
                        ?? "No fue posible cargar el detalle del comprobante original.";

                    _logger.LogWarning(
                        "NC Devolución: error de servicio al cargar detalle completo. Mensaje={Mensaje}, CorrelationId={CorrelationId}",
                        mensaje,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "ERROR_CARGA_DETALLE",
                        mensaje
                    });
                }

                var respuestaSp = resultado.ListaEntidad
                    ?? new List<NCProductoBuscarResponseDto>();

                var productosAceptados = new List<NCProductoBuscarResponseDto>();
                var advertencias = new List<object>();
                var rechazos = new List<object>();

                foreach (var producto in respuestaSp)
                {
                    if (producto.respuesta.HasValue &&
                        producto.respuesta.Value >= 0)
                    {
                        productosAceptados.Add(producto);

                        if (producto.respuesta.Value > 0)
                        {
                            advertencias.Add(
                                CrearResultadoProductoMensaje(
                                    producto,
                                    "El producto fue cargado con advertencia."
                                )
                            );
                        }

                        continue;
                    }

                    rechazos.Add(
                        CrearResultadoProductoMensaje(
                            producto,
                            "El producto no pudo agregarse a la devolución."
                        )
                    );
                }

                // La carga total reemplaza el detalle anterior para evitar duplicados.
                contexto.ProductosDevolucion = productosAceptados;
                contexto.FechaUltimaCargaProductosUtc = DateTime.UtcNow;

                GuardarContextoDevolucion(contexto);

                _logger.LogInformation(
                    "NC Devolución: carga total finalizada. Tipo={Tipo}, Comprobante={Comprobante}, RegistrosSP={RegistrosSP}, ProductosAceptados={ProductosAceptados}, Advertencias={Advertencias}, Rechazos={Rechazos}, CorrelationId={CorrelationId}",
                    comprobante.tco_id,
                    comprobante.cm_compte,
                    respuestaSp.Count,
                    productosAceptados.Count,
                    advertencias.Count,
                    rechazos.Count,
                    correlationId
                );

                return Json(new
                {
                    ok = true,
                    codigo = "DETALLE_CARGADO",
                    mensaje = productosAceptados.Count > 0
                        ? "El detalle del comprobante original fue cargado correctamente."
                        : "La consulta finalizó sin productos disponibles para cargar.",
                    modalidad = "TODOS",
                    productosCargados = productosAceptados.Count,
                    advertencias,
                    rechazos,
                    productos = productosAceptados
                        .Select(CrearResumenProductoDevolucion)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "NC Devolución: error inesperado al cargar detalle completo. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_INTERNO_CARGA_DETALLE",
                    mensaje =
                        "Ocurrió un error al cargar el detalle del comprobante original."
                });
            }
        }

        /// <summary>
        /// Obtiene los productos actualmente cargados para la NC por Devolución.
        /// Los productos se leen desde el contexto aislado de sesión.
        /// </summary>
        [HttpGet]
        public IActionResult ObtenerProductosDevolucion()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    codigo = "SESION_EXPIRADA",
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            try
            {
                var contexto = ObtenerContextoDevolucion();

                if (contexto == null ||
                    contexto.ComprobanteOrigen == null ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.tco_id))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "CONTEXTO_NO_DISPONIBLE",
                        mensaje =
                            "No existe una operación de Nota de Crédito por Devolución en curso."
                    });
                }

                var productos = contexto.ProductosDevolucion
                    ?? new List<NCProductoBuscarResponseDto>();

                return Json(new
                {
                    ok = true,
                    modalidad = contexto.CargarTodoDetalle == true
                        ? "TODOS"
                        : contexto.CargarTodoDetalle == false
                            ? "MANUAL"
                            : "SIN_DEFINIR",
                    productosCargados = productos.Count,
                    fechaUltimaCargaUtc = contexto.FechaUltimaCargaProductosUtc,
                    productos = productos
                        .Select(CrearResumenProductoDevolucion)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "NC Devolución: error al obtener productos de devolución."
                );

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_OBTENER_PRODUCTOS",
                    mensaje =
                        "No fue posible obtener los productos de la devolución."
                });
            }
        }

        /// <summary>
        /// Agrega un producto manualmente a la devolución actual.
        ///
        /// El navegador sólo informa valor y cantidad.
        /// El contexto del comprobante, la administración y los productos
        /// ya cargados se toman desde la sesión aislada de NC por Devolución.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> AgregarProductoManual(
            [FromBody] AgregarProductoManualRequest request)
        {
            var correlationId = Guid.NewGuid().ToString("N");

            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                _logger.LogWarning(
                    "NC Devolución: sesión expirada al agregar producto manual. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "SESION_EXPIRADA",
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "NC Devolución: request inválido al agregar producto manual. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "REQUEST_INVALIDO",
                    mensaje = "Los datos ingresados para el producto no son válidos."
                });
            }

            if (request == null)
            {
                return Json(new
                {
                    ok = false,
                    codigo = "REQUEST_VACIO",
                    mensaje = "No se recibieron datos para agregar el producto."
                });
            }

            try
            {
                var contexto = ObtenerContextoDevolucion();

                if (contexto == null ||
                    contexto.ComprobanteOrigen == null ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.tco_id) ||
                    string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.cm_compte))
                {
                    _logger.LogWarning(
                        "NC Devolución: contexto inexistente al agregar producto manual. Usuario={Usuario}, CorrelationId={CorrelationId}",
                        UserName,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "CONTEXTO_NO_DISPONIBLE",
                        mensaje =
                            "No existe un comprobante original validado. Vuelva a realizar la búsqueda."
                    });
                }

                if (contexto.CargarTodoDetalle != false)
                {
                    _logger.LogWarning(
                        "NC Devolución: intento de agregar producto manual fuera de modalidad manual. " +
                        "Modalidad={Modalidad}, CorrelationId={CorrelationId}",
                        contexto.CargarTodoDetalle.HasValue
                            ? contexto.CargarTodoDetalle.Value
                                ? "TODOS"
                                : "MANUAL"
                            : "SIN_DEFINIR",
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "MODALIDAD_NO_MANUAL",
                        mensaje =
                            "La carga manual sólo está disponible cuando se selecciona la modalidad manual."
                    });
                }

                var valor = request.Valor?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(valor))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "CODIGO_VACIO",
                        mensaje = "Debe ingresar un código de producto."
                    });
                }

                if (string.Equals(
                    valor,
                    "T",
                    StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning(
                        "NC Devolución: intento de usar valor reservado T en carga manual. CorrelationId={CorrelationId}",
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "VALOR_RESERVADO",
                        mensaje =
                            "El valor ingresado está reservado para la carga total del comprobante."
                    });
                }

                if (valor.Length > 30)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "CODIGO_LARGO_INVALIDO",
                        mensaje =
                            "El código ingresado supera la longitud máxima permitida."
                    });
                }

                if (request.Cantidad <= 0)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "CANTIDAD_INVALIDA",
                        mensaje = "La cantidad debe ser mayor a cero."
                    });
                }

                if (TieneMasDeTresDecimales(request.Cantidad))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "CANTIDAD_DECIMALES_INVALIDA",
                        mensaje =
                            "La cantidad puede tener como máximo tres decimales."
                    });
                }

                var token = TokenCookie;

                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "TOKEN_INVALIDO",
                        mensaje = "La sesión actual no posee un token válido."
                    });
                }

                var comprobante = contexto.ComprobanteOrigen;
                var admId = comprobante.adm_id?.Trim() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(admId))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "ADM_ORIGEN_NO_DISPONIBLE",
                        mensaje =
                            "El comprobante original no posee una administración válida."
                    });
                }

                contexto.ProductosDevolucion ??= new List<NCProductoBuscarResponseDto>();

                var jsonProductosActuales = JsonConvert.SerializeObject(
                    contexto.ProductosDevolucion
                );

                var requestApi = new NCProductoBuscarRequestDto
                {
                    tco_id = comprobante.tco_id.Trim(),
                    cm_compte = comprobante.cm_compte.Trim(),
                    cm_repetido = (comprobante.cm_repetido ?? 0)
                        .ToString(CultureInfo.InvariantCulture),
                    adm_id = admId,
                    valor = valor,
                    cantidad = request.Cantidad,
                    json_p = jsonProductosActuales
                };

                _logger.LogInformation(
                    "NC Devolución: agregando producto manual. " +
                    "Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, " +
                    "Adm={Adm}, Valor={Valor}, Cantidad={Cantidad}, ProductosActuales={ProductosActuales}, " +
                    "JsonProductosLength={JsonProductosLength}, CorrelationId={CorrelationId}",
                    requestApi.tco_id,
                    requestApi.cm_compte,
                    requestApi.cm_repetido,
                    requestApi.adm_id,
                    requestApi.valor,
                    requestApi.cantidad,
                    contexto.ProductosDevolucion.Count,
                    requestApi.json_p?.Length ?? 0,
                    correlationId
                );

                var resultado = await _ncServicio.BuscarProducto(
                    requestApi,
                    token
                );

                if (resultado == null || !resultado.Ok)
                {
                    var mensaje = !string.IsNullOrWhiteSpace(resultado?.Mensaje)
                        ? resultado.Mensaje.Trim()
                        : "No fue posible consultar el producto para la devolución.";

                    _logger.LogWarning(
                        "NC Devolución: error de servicio al agregar producto manual. " +
                        "Mensaje={Mensaje}, CorrelationId={CorrelationId}",
                        mensaje,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "ERROR_BUSCAR_PRODUCTO",
                        mensaje,
                        correlationId
                    });
                }

                var respuestaSp = resultado.ListaEntidad
                    ?? new List<NCProductoBuscarResponseDto>();

                var productosAceptados = new List<NCProductoBuscarResponseDto>();
                var advertencias = new List<object>();
                var rechazos = new List<object>();

                foreach (var producto in respuestaSp)
                {
                    if (producto == null)
                    {
                        continue;
                    }

                    var respuesta = producto.respuesta;

                    if (!respuesta.HasValue || respuesta.Value < 0)
                    {
                        rechazos.Add(
                            CrearResultadoProductoMensaje(
                                producto,
                                "El producto no pudo incorporarse a la devolución."
                            )
                        );

                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(producto.p_id))
                    {
                        rechazos.Add(new
                        {
                            p_id = string.Empty,
                            p_id_barrado = producto.p_id_barrado,
                            p_desc = producto.p_desc,
                            respuesta = producto.respuesta,
                            mensaje =
                                "La respuesta del producto no contiene un identificador válido."
                        });

                        continue;
                    }

                    var cantidadDelta = producto.cantidad_tot ?? 0m;

                    if (cantidadDelta <= 0)
                    {
                        rechazos.Add(new
                        {
                            p_id = producto.p_id,
                            p_id_barrado = producto.p_id_barrado,
                            p_desc = producto.p_desc,
                            respuesta = producto.respuesta,
                            mensaje =
                                "La respuesta del producto no contiene una cantidad válida."
                        });

                        continue;
                    }

                    var productoIntegrado = IntegrarProductoManual(
                        contexto.ProductosDevolucion,
                        producto
                    );

                    productosAceptados.Add(productoIntegrado);

                    if (respuesta.Value > 0)
                    {
                        advertencias.Add(
                            CrearResultadoProductoMensaje(
                                productoIntegrado,
                                string.IsNullOrWhiteSpace(producto.respuesta_msj)
                                    ? "El producto fue agregado con advertencia."
                                    : producto.respuesta_msj
                            )
                        );
                    }
                }

                if (productosAceptados.Count > 0)
                {
                    contexto.FechaUltimaCargaProductosUtc = DateTime.UtcNow;

                    GuardarContextoDevolucion(contexto);
                }

                var codigoRespuesta = productosAceptados.Count > 0
                    ? rechazos.Count > 0
                        ? "PRODUCTO_AGREGADO_PARCIAL"
                        : "PRODUCTO_AGREGADO"
                    : "PRODUCTO_RECHAZADO";

                var mensajeRespuesta = productosAceptados.Count > 0
                    ? productosAceptados.Count == 1
                        ? "Producto agregado correctamente."
                        : $"{productosAceptados.Count} productos fueron agregados correctamente."
                    : rechazos.Count > 0
                        ? "El producto no pudo agregarse a la devolución."
                        : "La búsqueda no devolvió productos para incorporar.";

                var respuestaHttp = new
                {
                    ok = true,
                    codigo = codigoRespuesta,
                    mensaje = mensajeRespuesta,
                    productosAgregados = productosAceptados.Count,
                    advertencias,
                    rechazos,
                    productos = contexto.ProductosDevolucion
                        .Select(CrearResumenProductoDevolucion)
                        .ToList()
                };

                var jsonRespuesta = JsonConvert.SerializeObject(
                    respuestaHttp,
                    Formatting.None
                );

                _logger.LogInformation(
                    "NC Devolución: respuesta manual preparada. " +
                    "Codigo={Codigo}, ProductosAgregados={ProductosAgregados}, " +
                    "Advertencias={Advertencias}, Rechazos={Rechazos}, " +
                    "JsonLength={JsonLength}, JsonPreview={JsonPreview}, " +
                    "CorrelationId={CorrelationId}",
                    codigoRespuesta,
                    productosAceptados.Count,
                    advertencias.Count,
                    rechazos.Count,
                    jsonRespuesta.Length,
                    jsonRespuesta.Substring(0, Math.Min(jsonRespuesta.Length, 800)),
                    correlationId
                );

                return Content(
                    jsonRespuesta,
                    "application/json; charset=utf-8"
                );
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: error inesperado al agregar producto manual. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_INTERNO_AGREGAR_PRODUCTO",
                    mensaje =
                        "Ocurrió un error al agregar el producto a la devolución."
                });
            }
        }

        /// <summary>
        /// Cancela la operación de NC por Devolución actualmente iniciada.
        ///
        /// Sólo elimina el estado aislado del módulo:
        /// - candidatos por repetición;
        /// - comprobante origen seleccionado.
        ///
        /// No afecta Facturación, Cobranza Diferida ni Cuenta Corriente.
        /// </summary>
        [HttpPost]
        public IActionResult CancelarOperacion()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                return Json(new
                {
                    ok = false,
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            try
            {
                LimpiarEstadoTemporalDevolucion();

                _logger?.LogInformation(
                    "NC Devolución: operación cancelada. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = true,
                    mensaje = "La operación fue cancelada correctamente."
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: error al cancelar la operación. Usuario={Usuario}",
                    UserName
                );

                return Json(new
                {
                    ok = false,
                    mensaje = "No fue posible cancelar la operación actual."
                });
            }
        }

        private IActionResult FinalizarSeleccionComprobante(
    NCValidaResponseDto comprobante)
        {
            var mensajeBloqueo = ObtenerMensajeBloqueo(comprobante);

            if (!string.IsNullOrWhiteSpace(mensajeBloqueo))
            {
                _logger?.LogWarning(
                    "NC Devolución: comprobante bloqueado. Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, Motivo={Motivo}",
                    comprobante.tco_id,
                    comprobante.cm_compte,
                    comprobante.cm_repetido,
                    mensajeBloqueo
                );

                return Json(new
                {
                    ok = false,
                    codigo = "COMPROBANTE_BLOQUEADO",
                    mensaje = mensajeBloqueo
                });
            }

            GuardarContextoDevolucion(comprobante);

            HttpContext.Session.Remove(SessionKeyNcDevolucionCandidatos);

            _logger?.LogInformation(
                "NC Devolución: comprobante origen seleccionado correctamente. Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, TipoNC={TipoNc}",
                comprobante.tco_id,
                comprobante.cm_compte,
                comprobante.cm_repetido,
                comprobante.nc_tco_id
            );

            return Json(new
            {
                ok = true,
                requiereSeleccion = false,
                mensaje = "Comprobante validado correctamente.",
                comprobante = CrearResumenComprobanteSeleccionado(comprobante)
            });
        }

        private static string ObtenerMensajeBloqueo(
            NCValidaResponseDto comprobante)
        {
            var motivos = new List<string>();

            if (comprobante.nc_sin_detalle == 1)
            {
                motivos.Add(
                    "El comprobante original no posee detalle de productos para devolver."
                );
            }

            if (comprobante.nc_fecha_supero_dias == 1)
            {
                motivos.Add(
                    "El comprobante original superó el plazo permitido para generar una Nota de Crédito."
                );
            }

            if (comprobante.nc_ya_emitida == 1)
            {
                motivos.Add(
                    "El comprobante original ya posee una Nota de Crédito emitida."
                );
            }

            if (motivos.Count == 0)
            {
                return string.Empty;
            }

            return string.Join(" ", motivos);
        }

        private static object CrearResumenCandidato(
            NCValidaResponseDto comprobante,
            int indice)
        {
            var mensajeBloqueo = ObtenerMensajeBloqueo(comprobante);

            return new
            {
                indice,
                tco_id = comprobante.tco_id,
                tco_desc = comprobante.tco_desc,
                cm_compte = comprobante.cm_compte,
                cm_repetido = comprobante.cm_repetido ?? 0,
                dia_movi = comprobante.dia_movi,
                cm_fecha = comprobante.cm_fecha?.ToString("dd/MM/yyyy HH:mm")
                    ?? string.Empty,
                cm_nombre = comprobante.cm_nombre,
                cm_cuit = comprobante.cm_cuit,
                cm_total = comprobante.cm_total ?? 0m,
                bloqueado = !string.IsNullOrWhiteSpace(mensajeBloqueo),
                motivo_bloqueo = mensajeBloqueo
            };
        }

        private static object CrearResumenComprobanteSeleccionado(
            NCValidaResponseDto comprobante)
        {
            return new
            {
                tco_id = comprobante.tco_id,
                tco_desc = comprobante.tco_desc,
                cm_compte = comprobante.cm_compte,
                cm_repetido = comprobante.cm_repetido ?? 0,
                dia_movi = comprobante.dia_movi,
                cm_fecha = comprobante.cm_fecha?.ToString("dd/MM/yyyy HH:mm")
                    ?? string.Empty,

                afip_id = comprobante.afip_id,
                afip_desc = comprobante.afip_desc,
                cta_id = comprobante.cta_id,
                cm_nombre = comprobante.cm_nombre,
                cm_cuit = comprobante.cm_cuit,
                cm_domicilio = comprobante.cm_domicilio,
                cm_total = comprobante.cm_total ?? 0m,

                nc_tco_letra = comprobante.nc_tco_letra,
                nc_tco_id = comprobante.nc_tco_id,
                nc_tco_desc = comprobante.nc_tco_desc,

                nc_ctacte = comprobante.nc_ctacte,
                nc_dv_dist = comprobante.nc_dv_dist,
                nc_dv_pago_diferido = comprobante.nc_dv_pago_diferido
            };
        }

        private bool TryObtenerDatosCajaParaValidacion(
            out string cajaNroProceso,
            out short cajaNroCierre,
            out string mensaje)
        {
            cajaNroProceso = string.Empty;
            cajaNroCierre = 0;
            mensaje = string.Empty;

            var cajaActual = CajaActual;

            if (cajaActual?.Caja == null)
            {
                mensaje =
                    "No se encontraron datos de caja para validar el comprobante.";
                return false;
            }

            cajaNroProceso = Convert.ToString(
                cajaActual.Caja.caja_nro_proceso,
                CultureInfo.InvariantCulture
            )?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cajaNroProceso))
            {
                mensaje =
                    "La caja actual no posee número de proceso configurado.";
                return false;
            }

            var cierreTexto = Convert.ToString(
                cajaActual.Caja.caja_nro_cierre,
                CultureInfo.InvariantCulture
            )?.Trim() ?? string.Empty;

            if (!short.TryParse(
                cierreTexto,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out cajaNroCierre))
            {
                mensaje =
                    "La caja actual no posee un número de cierre válido.";
                return false;
            }

            return true;
        }

        private static bool TryNormalizarComprobante(
            string? puntoVentaIngresado,
            string? numeroIngresado,
            out string cmCompte,
            out string mensaje)
        {
            cmCompte = string.Empty;
            mensaje = string.Empty;

            var puntoVenta = puntoVentaIngresado?.Trim() ?? string.Empty;
            var numero = numeroIngresado?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(puntoVenta))
            {
                mensaje = "Debe ingresar el punto de venta del comprobante.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(numero))
            {
                mensaje = "Debe ingresar el número del comprobante.";
                return false;
            }

            if (puntoVenta.Length > 4 || puntoVenta.Any(x => !char.IsDigit(x)))
            {
                mensaje =
                    "El punto de venta debe contener sólo números y hasta 4 dígitos.";
                return false;
            }

            if (numero.Length > 8 || numero.Any(x => !char.IsDigit(x)))
            {
                mensaje =
                    "El número de comprobante debe contener sólo números y hasta 8 dígitos.";
                return false;
            }

            cmCompte =
                $"{puntoVenta.PadLeft(4, '0')}-{numero.PadLeft(8, '0')}";

            return true;
        }

        private void GuardarCandidatosComprobante(
            List<NCValidaResponseDto> candidatos)
        {
            var json = JsonConvert.SerializeObject(candidatos);

            HttpContext.Session.SetString(
                SessionKeyNcDevolucionCandidatos,
                json
            );
        }

        private List<NCValidaResponseDto> ObtenerCandidatosComprobante()
        {
            var json = HttpContext.Session.GetString(
                SessionKeyNcDevolucionCandidatos
            );

            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            try
            {
                return JsonConvert.DeserializeObject<
                    List<NCValidaResponseDto>
                >(json) ?? [];
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: no fue posible recuperar candidatos desde sesión."
                );

                HttpContext.Session.Remove(
                    SessionKeyNcDevolucionCandidatos
                );

                return [];
            }
        }

        private void GuardarContextoDevolucion(
    NCValidaResponseDto comprobante)
        {
            var contexto = new NCDevolucionContextoSesion
            {
                ComprobanteOrigen = comprobante,
                CargarTodoDetalle = null,
                FechaCreacionUtc = DateTime.UtcNow
            };

            GuardarContextoDevolucion(contexto);
        }

        private void GuardarContextoDevolucion(
            NCDevolucionContextoSesion contexto)
        {
            var json = JsonConvert.SerializeObject(contexto);

            HttpContext.Session.SetString(
                SessionKeyNcDevolucionContexto,
                json
            );
        }

        private NCDevolucionContextoSesion? ObtenerContextoDevolucion()
        {
            var json = HttpContext.Session.GetString(
                SessionKeyNcDevolucionContexto
            );

            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject<NCDevolucionContextoSesion>(json);
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: no fue posible recuperar el contexto desde sesión."
                );

                HttpContext.Session.Remove(
                    SessionKeyNcDevolucionContexto
                );

                return null;
            }
        }

        private void LimpiarEstadoTemporalDevolucion()
        {
            HttpContext.Session.Remove(
                SessionKeyNcDevolucionCandidatos
            );

            HttpContext.Session.Remove(
                SessionKeyNcDevolucionContexto
            );
        }

        private static object CrearResultadoProductoMensaje(
    NCProductoBuscarResponseDto producto,
    string mensajePredeterminado)
        {
            return new
            {
                p_id = producto.p_id,
                p_id_barrado = producto.p_id_barrado,
                p_desc = producto.p_desc,
                respuesta = producto.respuesta,
                mensaje = string.IsNullOrWhiteSpace(producto.respuesta_msj)
                    ? mensajePredeterminado
                    : producto.respuesta_msj.Trim()
            };
        }

        private static object CrearResumenProductoDevolucion(
            NCProductoBuscarResponseDto producto)
        {
            return new
            {
                p_id = producto.p_id,
                p_id_barrado = producto.p_id_barrado,
                p_desc = producto.p_desc,

                cantidad_tot = producto.cantidad_tot ?? 0m,
                bultos = producto.bultos,

                p_pneto = producto.p_pneto ?? 0m,
                p_iva = producto.p_iva ?? 0m,
                p_pvta = producto.p_pvta ?? 0m,

                iva_situacion = producto.iva_situacion,
                iva_alicuota = producto.iva_alicuota ?? 0m,

                cmd_cmb = producto.cmd_cmb,
                cmd_cmb_id = producto.cmd_cmb_id,
                cmd_cmb_desc = producto.cmd_cmb_desc,

                respuesta = producto.respuesta,
                respuesta_msj = producto.respuesta_msj
            };
        }

        private static bool TieneMasDeTresDecimales(decimal valor)
        {
            return decimal.Round(
                valor,
                3,
                MidpointRounding.AwayFromZero
            ) != valor;
        }

        /// <summary>
        /// Integra un delta de producto manual a la lista de productos de devolución.
        ///
        /// La clave de integración es p_id porque un mismo producto puede llegar
        /// por EAN, código corto o código de balanza.
        /// </summary>
        private static NCProductoBuscarResponseDto IntegrarProductoManual(
            List<NCProductoBuscarResponseDto> productosDevolucion,
            NCProductoBuscarResponseDto productoNuevo)
        {
            var pId = productoNuevo.p_id.Trim();
            var cantidadDelta = productoNuevo.cantidad_tot ?? 0m;

            var indiceExistente = productosDevolucion.FindIndex(
                producto => string.Equals(
                    producto.p_id?.Trim(),
                    pId,
                    StringComparison.OrdinalIgnoreCase
                )
            );

            productoNuevo.p_id = pId;

            if (indiceExistente < 0)
            {
                productosDevolucion.Add(productoNuevo);

                return productoNuevo;
            }

            var productoExistente = productosDevolucion[indiceExistente];

            productoNuevo.cantidad_tot =
                (productoExistente.cantidad_tot ?? 0m) +
                cantidadDelta;

            productosDevolucion[indiceExistente] = productoNuevo;

            return productoNuevo;
        }
    }
}
