using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.caja.Models.NotaCredito;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    //[Authorize]
    public class NotaCreditoController : ControladorBaseCaja
    {
        private readonly INotaCreditoServicio _ncServicio;
        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly ICajaServicio _cajaServicio;
        private readonly AppSettings _appSettings;

        private const string SessionKeyNcDevolucionCandidatos = "NCDEV_CANDIDATOS_COMPROBANTE";

        private const string SessionKeyNcDevolucionContexto = "NCDEV_CONTEXTO";

        public NotaCreditoController(
            IOptions<AppSettings> options,
            ILogger<NotaCreditoController> logger,
            IHttpContextAccessor httpContext,
            INotaCreditoServicio ncServicio,
            ICajaInitServicio cajaInitServicio,
            ICajaServicio cajaServicio)
            : base(options, httpContext, logger)
        {
            _ncServicio = ncServicio;
            _appSettings = options.Value;
            _cajaInitServicio = cajaInitServicio;
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
                    _logger?.LogWarning(
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

                _logger?.LogInformation(
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
                _logger?.LogError(
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
                _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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

                _logger?.LogInformation(
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

                    _logger?.LogWarning(
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

                _logger?.LogInformation(
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
                _logger?.LogError(
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
                _logger?.LogError(
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
        /// Quita un producto de la grilla de devolucion actual.
        /// La lista se actualiza en el contexto aislado de sesion para que
        /// el calculo y la confirmacion trabajen con el mismo detalle.
        /// </summary>
        [HttpPost]
        public IActionResult QuitarProductoDevolucion(
            [FromBody] QuitarProductoDevolucionRequest request)
        {
            var correlationId = Guid.NewGuid().ToString("N");

            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                _logger?.LogWarning(
                    "NC Devolución: sesión expirada al quitar producto. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "SESION_EXPIRADA",
                    mensaje = "La sesión ha expirado. Vuelva a iniciar sesión."
                });
            }

            if (request == null || request.Indice < 0)
            {
                _logger?.LogWarning(
                    "NC Devolución: request inválido al quitar producto. Indice={Indice}, CorrelationId={CorrelationId}",
                    request?.Indice,
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "REQUEST_INVALIDO",
                    mensaje = "No se recibió un producto válido para quitar."
                });
            }

            try
            {
                var contexto = ObtenerContextoDevolucion();
                var validacion = ValidarContextoConProductos(contexto);

                if (!validacion.Ok)
                {
                    _logger?.LogWarning(
                        "NC Devolución: no se pudo quitar producto por contexto inválido. Codigo={Codigo}, Mensaje={Mensaje}, CorrelationId={CorrelationId}",
                        validacion.Codigo,
                        validacion.Mensaje,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = validacion.Codigo,
                        mensaje = validacion.Mensaje
                    });
                }

                var productos = contexto!.ProductosDevolucion
                    ?? new List<NCProductoBuscarResponseDto>();

                if (request.Indice >= productos.Count)
                {
                    _logger?.LogWarning(
                        "NC Devolución: índice fuera de rango al quitar producto. Indice={Indice}, Productos={Productos}, CorrelationId={CorrelationId}",
                        request.Indice,
                        productos.Count,
                        correlationId
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "INDICE_INVALIDO",
                        mensaje = "El producto seleccionado ya no se encuentra en la grilla."
                    });
                }

                var productoQuitado = productos[request.Indice];
                productos.RemoveAt(request.Indice);

                contexto.ProductosDevolucion = productos;
                contexto.CoTipo = string.Empty;
                contexto.JsonProductosCalculado = string.Empty;
                contexto.JsonSubtotal = string.Empty;
                contexto.JsonSorteo = string.Empty;
                contexto.FechaUltimoCalculoUtc = null;
                contexto.FechaUltimaCargaProductosUtc = DateTime.UtcNow;

                GuardarContextoDevolucion(contexto);

                _logger?.LogInformation(
                    "NC Devolución: producto quitado de la grilla. Producto={Producto}, CodigoBarra={CodigoBarra}, Indice={Indice}, ProductosRestantes={ProductosRestantes}, CorrelationId={CorrelationId}",
                    productoQuitado.p_id,
                    productoQuitado.p_id_barrado,
                    request.Indice,
                    productos.Count,
                    correlationId
                );

                return Json(new
                {
                    ok = true,
                    codigo = "PRODUCTO_QUITADO",
                    mensaje = "El producto fue quitado de la Nota de Crédito.",
                    productosCargados = productos.Count,
                    productoQuitado = CrearResumenProductoDevolucion(productoQuitado),
                    productos = productos
                        .Select(CrearResumenProductoDevolucion)
                        .ToList()
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: error inesperado al quitar producto. CorrelationId={CorrelationId}",
                    correlationId
                );

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_INTERNO_QUITAR_PRODUCTO",
                    mensaje = "No fue posible quitar el producto de la devolución."
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
                _logger?.LogWarning(
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
                _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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
                    _logger?.LogWarning(
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

                _logger?.LogInformation(
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

                    _logger?.LogWarning(
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

                _logger?.LogInformation(
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

        [HttpPost]
        public async Task<IActionResult> SeguirCalculo(
            [FromBody] SeguirNotaCreditoRequest request)
        {
            var stopwatch = Stopwatch.StartNew();
            var correlationId = HttpContext?.TraceIdentifier ?? Guid.NewGuid().ToString("N");

            _logger?.LogInformation("═══════════════════════════════════════════════════");
            _logger?.LogInformation("➡️ NC DEVOLUCIÓN - SEGUIR / CÁLCULO DE FILAS - INICIO");
            _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
            _logger?.LogInformation(
                "   Request recibido: {Request}",
                JsonConvert.SerializeObject(request)
            );
            _logger?.LogInformation("═══════════════════════════════════════════════════");

            if (!VerificarAutenticacion(out IActionResult redirectResult))
            {
                stopwatch.Stop();
                _logger?.LogWarning(
                    "⚠️ NC Devolución Seguir: sesión expirada. CorrelationId={CorrelationId}. Tiempo={Elapsed}ms",
                    correlationId,
                    stopwatch.ElapsedMilliseconds
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
                var validacion = ValidarContextoConProductos(contexto);

                if (!validacion.Ok)
                {
                    stopwatch.Stop();
                    _logger?.LogWarning("═══════════════════════════════════════════════════");
                    _logger?.LogWarning("⚠️ NC DEVOLUCIÓN - SEGUIR BLOQUEADO POR CONTEXTO");
                    _logger?.LogWarning("   CorrelationId: {CorrelationId}", correlationId);
                    _logger?.LogWarning("   Código: {Codigo}", validacion.Codigo);
                    _logger?.LogWarning("   Mensaje: {Mensaje}", validacion.Mensaje);
                    _logger?.LogWarning("   Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                    _logger?.LogWarning("═══════════════════════════════════════════════════");

                    return Json(new
                    {
                        ok = false,
                        codigo = validacion.Codigo,
                        mensaje = validacion.Mensaje
                    });
                }

                var comprobante = contexto!.ComprobanteOrigen;
                var requiereDecisionCtaCte = RequiereDecisionCuentaCorriente(comprobante);

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📋 NC DEVOLUCIÓN - CONTEXTO RECUPERADO");
                _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
                _logger?.LogInformation(
                    "   Comprobante origen: {TcoId} {CmCompte}",
                    comprobante?.tco_id,
                    comprobante?.cm_compte
                );
                _logger?.LogInformation(
                    "   Cliente: {CtaId} - {Cliente}",
                    comprobante?.cta_id,
                    comprobante?.cm_nombre
                );
                _logger?.LogInformation(
                    "   Flags destino: nc_ctacte={NcCtaCte}, nc_dv_dist={NcDvDist}, nc_dv_pago_diferido={NcDvPagoDiferido}",
                    comprobante?.nc_ctacte,
                    comprobante?.nc_dv_dist,
                    comprobante?.nc_dv_pago_diferido
                );
                _logger?.LogInformation(
                    "   NC a emitir: {NcTcoLetra} {NcTcoId} - {NcTcoDesc}",
                    comprobante?.nc_tco_letra,
                    comprobante?.nc_tco_id,
                    comprobante?.nc_tco_desc
                );
                _logger?.LogInformation("   Requiere decisión CtaCte: {RequiereDecision}", requiereDecisionCtaCte);
                _logger?.LogInformation("   Productos en contexto: {CantidadProductos}", contexto.ProductosDevolucion?.Count ?? 0);
                _logger?.LogInformation("   json_p calculado previo longitud: {JsonProductosLength}", contexto.JsonProductosCalculado?.Length ?? 0);
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (requiereDecisionCtaCte &&
                    request?.DejarEnCuentaCorriente == null)
                {
                    stopwatch.Stop();
                    _logger?.LogWarning(
                        "⚠️ NC Devolución Seguir: falta decisión de CtaCte. CorrelationId={CorrelationId}. Tiempo={Elapsed}ms",
                        correlationId,
                        stopwatch.ElapsedMilliseconds
                    );

                    return Json(new
                    {
                        ok = true,
                        requiereDecisionCtaCte = true,
                        mensaje = "Debe indicar el destino del saldo de la Nota de Crédito."
                    });
                }

                if (requiereDecisionCtaCte &&
                    request?.DejarEnCuentaCorriente == true &&
                    request.ConfirmacionCuentaCorriente != true)
                {
                    stopwatch.Stop();
                    _logger?.LogWarning(
                        "⚠️ NC Devolución Seguir: falta confirmación de CtaCte. CorrelationId={CorrelationId}. Tiempo={Elapsed}ms",
                        correlationId,
                        stopwatch.ElapsedMilliseconds
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "CONFIRMACION_CTACTE_REQUERIDA",
                        mensaje = "Debe confirmar que la Nota de Crédito quedará en Cuenta Corriente."
                    });
                }

                var coTipo = DeterminarCoTipo(comprobante, request);
                var token = TokenCookie;

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("🧭 NC DEVOLUCIÓN - DESTINO DEFINIDO");
                _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
                _logger?.LogInformation("   DejarEnCuentaCorriente request: {DejarEnCuentaCorriente}", request?.DejarEnCuentaCorriente);
                _logger?.LogInformation("   Confirmación CtaCte request: {ConfirmacionCuentaCorriente}", request?.ConfirmacionCuentaCorriente);
                _logger?.LogInformation("   co_tipo calculado: {CoTipo}", coTipo);
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (string.IsNullOrWhiteSpace(token))
                {
                    stopwatch.Stop();
                    _logger?.LogError(
                        "❌ NC Devolución Seguir: token inválido. CorrelationId={CorrelationId}. Tiempo={Elapsed}ms",
                        correlationId,
                        stopwatch.ElapsedMilliseconds
                    );

                    return Json(new
                    {
                        ok = false,
                        codigo = "TOKEN_INVALIDO",
                        mensaje = "La sesión actual no posee un token válido."
                    });
                }

                var requestCalculo = CrearRequestCalculo(contexto, coTipo);

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📦 NC DEVOLUCIÓN - REQUEST CALCULAR FILAS CONSTRUIDO");
                _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
                _logger?.LogInformation("   co_tipo definido para el flujo: {CoTipo}", coTipo);
                _logger?.LogInformation("   cta_id: {CtaId}", requestCalculo.cta_id);
                _logger?.LogInformation("   tco_id: {TcoId}", requestCalculo.tco_id);
                _logger?.LogInformation("   tco_id_ori: {TcoIdOri}", requestCalculo.tco_id_ori);
                _logger?.LogInformation("   cm_compte_ori: {CmCompteOri}", requestCalculo.cm_compte_ori);
                _logger?.LogInformation("   json_p longitud: {JsonProductosLength}", requestCalculo.json_p?.Length ?? 0);
                _logger?.LogInformation("   json_p: {JsonProductos}", requestCalculo.json_p);
                _logger?.LogInformation(
                    "   Request CalcularFilas: {RequestCalculo}",
                    JsonConvert.SerializeObject(requestCalculo)
                );
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                _logger?.LogInformation(
                    "📡 NC Devolución: invocando servicio NotaCreditoServicio.CalcularFilas. CorrelationId={CorrelationId}",
                    correlationId
                );
                var resultado = await _ncServicio.CalcularFilas(requestCalculo, token);

                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("📥 NC DEVOLUCIÓN - RESPONSE CALCULAR FILAS");
                _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
                _logger?.LogInformation("   Resultado null: {ResultadoNull}", resultado == null);
                _logger?.LogInformation("   Ok: {Ok}", resultado?.Ok);
                _logger?.LogInformation("   Mensaje: {Mensaje}", resultado?.Mensaje);
                _logger?.LogInformation("   Entidad null: {EntidadNull}", resultado?.Entidad == null);
                _logger?.LogInformation("   json_subtotal longitud: {JsonSubtotalLength}", resultado?.Entidad?.json_subtotal?.Length ?? 0);
                _logger?.LogInformation("   json_p respuesta longitud: {JsonProductosLength}", resultado?.Entidad?.json_p?.Length ?? 0);
                _logger?.LogInformation(
                    "   Response CalcularFilas: {ResponseCalculo}",
                    JsonConvert.SerializeObject(resultado)
                );
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                if (resultado == null || !resultado.Ok || resultado.Entidad == null)
                {
                    stopwatch.Stop();
                    _logger?.LogWarning("═══════════════════════════════════════════════════");
                    _logger?.LogWarning("⚠️ NC DEVOLUCIÓN - CÁLCULO DE FILAS RECHAZADO");
                    _logger?.LogWarning("   CorrelationId: {CorrelationId}", correlationId);
                    _logger?.LogWarning("   Mensaje servicio: {Mensaje}", resultado?.Mensaje);
                    _logger?.LogWarning("   Tiempo: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                    _logger?.LogWarning("═══════════════════════════════════════════════════");

                    return Json(new
                    {
                        ok = false,
                        codigo = "ERROR_CALCULO_FILAS",
                        mensaje = resultado?.Mensaje ?? "No fue posible calcular los totales de la Nota de Crédito."
                    });
                }

                contexto.CoTipo = coTipo;
                contexto.JsonProductosCalculado = string.IsNullOrWhiteSpace(resultado.Entidad.json_p)
                    ? requestCalculo.json_p
                    : resultado.Entidad.json_p;
                contexto.JsonSubtotal = string.IsNullOrWhiteSpace(resultado.Entidad.json_subtotal)
                    ? "[]"
                    : resultado.Entidad.json_subtotal;
                contexto.JsonSorteo = string.Empty;
                contexto.FechaUltimoCalculoUtc = DateTime.UtcNow;

                GuardarContextoDevolucion(contexto);

                var subtotales = CrearResumenSubtotales(contexto.JsonSubtotal);
                var responseFinal = new
                {
                    ok = true,
                    codigo = "NC_CALCULADA",
                    mensaje = "Totales calculados correctamente.",
                    co_tipo = coTipo,
                    destino = coTipo == "DV"
                        ? "Cuenta Corriente"
                        : "Devolución de dinero",
                    calculo = new
                    {
                        json_subtotal = contexto.JsonSubtotal,
                        json_p = contexto.JsonProductosCalculado
                    },
                    subtotales
                };

                stopwatch.Stop();
                _logger?.LogInformation("═══════════════════════════════════════════════════");
                _logger?.LogInformation("✅ NC DEVOLUCIÓN - SEGUIR FINALIZADO CORRECTAMENTE");
                _logger?.LogInformation("   CorrelationId: {CorrelationId}", correlationId);
                _logger?.LogInformation("   co_tipo: {CoTipo}", coTipo);
                _logger?.LogInformation("   destino: {Destino}", responseFinal.destino);
                _logger?.LogInformation("   json_subtotal final longitud: {JsonSubtotalLength}", contexto.JsonSubtotal?.Length ?? 0);
                _logger?.LogInformation("   json_p final longitud: {JsonProductosLength}", contexto.JsonProductosCalculado?.Length ?? 0);
                _logger?.LogInformation("   subtotales resumidos: {CantidadSubtotales}", subtotales.Count);
                _logger?.LogInformation("   Response frontend: {ResponseFinal}", JsonConvert.SerializeObject(responseFinal));
                _logger?.LogInformation("   Tiempo total: {Elapsed}ms", stopwatch.ElapsedMilliseconds);
                _logger?.LogInformation("═══════════════════════════════════════════════════");

                return Json(responseFinal);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger?.LogError(
                    ex,
                    "❌ NC Devolución: error al avanzar a cálculo. CorrelationId={CorrelationId}. Request={Request}. Tiempo={Elapsed}ms",
                    correlationId,
                    JsonConvert.SerializeObject(request),
                    stopwatch.ElapsedMilliseconds
                );

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_INTERNO_SEGUIR",
                    mensaje = "Ocurrió un error al calcular la Nota de Crédito."
                });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Finalizar()
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
                var validacion = ValidarContextoConProductos(contexto);

                if (!validacion.Ok)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = validacion.Codigo,
                        mensaje = validacion.Mensaje
                    });
                }

                if (string.IsNullOrWhiteSpace(contexto!.CoTipo) ||
                    string.IsNullOrWhiteSpace(contexto.JsonProductosCalculado) ||
                    string.IsNullOrWhiteSpace(contexto.JsonSubtotal))
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "NC_SIN_CALCULO",
                        mensaje = "Debe presionar Seguir y calcular los totales antes de finalizar."
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

                var requestConfirmacion = CrearRequestConfirmacion(contexto);
                var resultado = await _ncServicio.ConfirmarOperacionCaja(requestConfirmacion, token);

                if (resultado == null || !resultado.Ok || resultado.Entidad == null)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "ERROR_CONFIRMAR_NC",
                        mensaje = resultado?.Mensaje ?? "No fue posible confirmar la Nota de Crédito."
                    });
                }

                var respuestaDto = resultado.Entidad;

                if (respuestaDto.resultado != 0)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "SP_CONFIRMAR_NC_RECHAZO",
                        mensaje = respuestaDto.resultado_msj ?? "La Nota de Crédito no pudo emitirse."
                    });
                }

                if (!TryParsearComprobanteJson(respuestaDto.resultado_id, out var comprobanteEmitido) ||
                    comprobanteEmitido == null)
                {
                    return Json(new
                    {
                        ok = false,
                        codigo = "COMPROBANTE_EMITIDO_INVALIDO",
                        mensaje = "La Nota de Crédito fue procesada, pero no se pudo interpretar el comprobante emitido.",
                        debug_resultado_id = respuestaDto.resultado_id
                    });
                }

                var mensajeStock = await RegistrarStockNotaCredito(comprobanteEmitido, token);

                LimpiarEstadoTemporalDevolucion();

                return Json(new
                {
                    ok = true,
                    codigo = "NC_EMITIDA",
                    mensaje = $"Nota de Crédito {comprobanteEmitido.tco_letra} Nro {comprobanteEmitido.cm_compte} emitida correctamente.",
                    resultado_completo = respuestaDto.resultado_msj,
                    stock = mensajeStock,
                    debe_imprimir = true,
                    data = new[]
                    {
                        new
                        {
                            tco_letra = comprobanteEmitido.tco_letra,
                            tco_id = comprobanteEmitido.tco_id,
                            cm_compte = comprobanteEmitido.cm_compte,
                            cm_repetido = comprobanteEmitido.cm_repetido,
                            co_tipo = contexto.CoTipo
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "NC Devolución: error al finalizar.");

                return Json(new
                {
                    ok = false,
                    codigo = "ERROR_INTERNO_FINALIZAR",
                    mensaje = "Ocurrió un error al finalizar la Nota de Crédito."
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
                cm_email = comprobante.cm_email,
                cm_movil = comprobante.cm_movil,
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

        private sealed class ValidacionContextoNcResult
        {
            public bool Ok { get; init; }
            public string Codigo { get; init; } = string.Empty;
            public string Mensaje { get; init; } = string.Empty;
        }

        private ValidacionContextoNcResult ValidarContextoConProductos(
            NCDevolucionContextoSesion? contexto)
        {
            if (contexto == null ||
                contexto.ComprobanteOrigen == null ||
                string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.tco_id) ||
                string.IsNullOrWhiteSpace(contexto.ComprobanteOrigen.cm_compte))
            {
                return new ValidacionContextoNcResult
                {
                    Ok = false,
                    Codigo = "CONTEXTO_NO_DISPONIBLE",
                    Mensaje = "No existe un comprobante original validado. Vuelva a realizar la búsqueda."
                };
            }

            if (contexto.ProductosDevolucion == null ||
                contexto.ProductosDevolucion.Count == 0)
            {
                return new ValidacionContextoNcResult
                {
                    Ok = false,
                    Codigo = "SIN_PRODUCTOS",
                    Mensaje = "Debe cargar al menos un producto para generar la Nota de Crédito."
                };
            }

            var cajaActual = CajaActual;

            if (cajaActual?.Caja == null ||
                string.IsNullOrWhiteSpace(cajaActual.CajaId))
            {
                return new ValidacionContextoNcResult
                {
                    Ok = false,
                    Codigo = "CAJA_NO_DISPONIBLE",
                    Mensaje = "No se encontraron datos válidos de caja para continuar."
                };
            }

            return new ValidacionContextoNcResult
            {
                Ok = true,
                Codigo = "OK",
                Mensaje = "OK"
            };
        }

        private static bool RequiereDecisionCuentaCorriente(
            NCValidaResponseDto comprobante)
        {
            return comprobante.nc_ctacte == 1 &&
                   comprobante.nc_dv_dist == 0 &&
                   comprobante.nc_dv_pago_diferido == 0;
        }

        private static string DeterminarCoTipo(
            NCValidaResponseDto comprobante,
            SeguirNotaCreditoRequest? request)
        {
            if (comprobante.nc_dv_dist == 1 ||
                comprobante.nc_dv_pago_diferido == 1)
            {
                return "DV";
            }

            if (RequiereDecisionCuentaCorriente(comprobante) &&
                request?.DejarEnCuentaCorriente == true)
            {
                return "DV";
            }

            return "AA";
        }

        private string CrearJsonProductosDevolucion(
            NCDevolucionContextoSesion contexto)
        {
            List<ProductoFactJsonDto> productos = MapearProductosModeloNC_aModeloFact(contexto.ProductosDevolucion);

            return JsonConvert.SerializeObject(
                productos ?? [],
                Formatting.None
            );
        }

        private List<ProductoFactJsonDto> MapearProductosModeloNC_aModeloFact(List<NCProductoBuscarResponseDto> pdev)
        {
            //realiza el mapeo campo a campo del modelo NCProductoBuscarResponseDto => ProductoFactJsonDto
            var prods = new List<ProductoFactJsonDto>();
            foreach (var item in pdev)
            {
                var p = new ProductoFactJsonDto(MapeaProdNC(item));
                prods.Add(p);
            }

            return prods;
        }

        private ProductoDatosResponseDto MapeaProdNC(NCProductoBuscarResponseDto item)
        {
            //traspasar los valores de item al tipo de dato resultante
            var p = new ProductoDatosResponseDto();
            p.p_id = item.p_id;
            p.p_id_barrado = item.p_id_barrado;
            //p.sin_scan_con_barrado = item.sin_scan_con_barrado;
            p.p_desc = item.p_desc;
            p.p_pcosto = item.p_pcosto ?? 0m;
            p.p_pcosto_repo = item.p_pcosto_repo ?? 0m;
            p.in_alicuota = item.in_alicuota ?? 0m;
            p.p_in = item.p_in ?? 0m;
            p.p_pvta = item.p_pvta ?? 0m;
            p.iva_situacion = item.iva_situacion;
            p.iva_alicuota= item.iva_alicuota ?? 0m;
            p.p_iva = item.p_iva ?? 0m;
            p.p_pneto= item.p_pneto ?? 0m;
            p.po = item.po;
            p.po_limite =  0;
            p.p_pvta = item.p_pvta ?? 0m;
            p.cantidad_tot = item.cantidad_tot ?? 0m;
            p.bultos = item.bultos ;
            p.p_activo = item.p_activo;
            p.rub_id = item.rub_id;
            p.rub_desc = item.rub_desc;
            p.cta_id = item.cta_id;
            p.pg_id = item.pg_id;
            p.up_id = item.up_id;
            p.up_tipo = item.up_tipo;
            p.up_desc = item.up_desc;
            p.p_unidad_pres = item.p_unidad_pres;
            p.p_peso = item.p_peso ?? 0m;
            p.cpf_nro = null;

            return p;
        }

        private CalcularFilasReqDto CrearRequestCalculo(
            NCDevolucionContextoSesion contexto,
            string coTipo)
        {
            var comprobante = contexto.ComprobanteOrigen;
            var cajaActual = CajaActual;
            var productos = contexto.ProductosDevolucion ?? [];

            return new CalcularFilasReqDto
            {
                caja_id = cajaActual?.CajaId ?? string.Empty,
                usu_id = UserName ?? string.Empty,
                adm_id = comprobante.adm_id ?? AdministracionId,
                lp_id = !string.IsNullOrWhiteSpace(comprobante.lp_id)
                    ? comprobante.lp_id
                    : cajaActual?.Caja?.lp_id_min ?? string.Empty,
                caja_nro_proceso = cajaActual?.Caja?.caja_nro_proceso ?? string.Empty,
                caja_nro_cierre = cajaActual?.Caja?.caja_nro_cierre ?? string.Empty,

                cta_id = comprobante.cta_id ?? string.Empty,
                ctac_dto = comprobante.cm_dto_porc ?? 0m,
                ctc_id = string.Empty,

                tco_letra = comprobante.nc_tco_letra ?? string.Empty,
                tco_id = comprobante.nc_tco_id ?? string.Empty,
                tco_id_ori = comprobante.tco_id ?? string.Empty,
                cm_compte_ori = comprobante.cm_compte ?? string.Empty,

                afip_id = comprobante.afip_id ?? string.Empty,
                afip_desc = comprobante.afip_desc ?? string.Empty,

                tot_rows = productos.Count > short.MaxValue
                    ? short.MaxValue
                    : (short)productos.Count,
                tot_cantidad = productos.Sum(x => x.cantidad_tot ?? 0m),
                tot_pvta = productos.Sum(x => (x.p_pvta ?? 0m) * (x.cantidad_tot ?? 0m)),

                json_p = CrearJsonProductosDevolucion(contexto)
            };
        }

        private CajaOpeConfirmarReq CrearRequestConfirmacion(
            NCDevolucionContextoSesion contexto)
        {
            var comprobante = contexto.ComprobanteOrigen;
            var cajaActual = CajaActual;

            return new CajaOpeConfirmarReq
            {
                caja_id = cajaActual?.CajaId ?? string.Empty,
                usu_id = UserName ?? string.Empty,
                adm_id = comprobante.adm_id ?? AdministracionId,
                lp_id = !string.IsNullOrWhiteSpace(comprobante.lp_id)
                    ? comprobante.lp_id
                    : cajaActual?.Caja?.lp_id_min ?? string.Empty,
                caja_nro_proceso = cajaActual?.Caja?.caja_nro_proceso ?? string.Empty,
                caja_nro_cierre = cajaActual?.Caja?.caja_nro_cierre,

                cta_id = comprobante.cta_id,
                ctac_dto = comprobante.cm_dto_porc ?? 0m,
                co_tipo = contexto.CoTipo,
                ctc_id = string.Empty,

                tco_letra = comprobante.nc_tco_letra ?? string.Empty,
                tco_id_ori = comprobante.tco_id ?? string.Empty,
                cm_compte_ori = comprobante.cm_compte ?? string.Empty,

                afip_id = comprobante.afip_id ?? string.Empty,
                tdoc_id = comprobante.tdoc_id ?? string.Empty,
                cta_documento = comprobante.cm_cuit ?? string.Empty,
                cta_denominacion = comprobante.cm_nombre ?? string.Empty,
                cta_domicilio = comprobante.cm_domicilio ?? string.Empty,
                ve_id = comprobante.ve_id ?? string.Empty,

                json_p = contexto.JsonProductosCalculado,
                json_subtotal = contexto.JsonSubtotal,
                json_sorteo = string.Empty,
                json_valores = "[]",
                json_cancela = "[]",
                json_union = "[]"
            };
        }

        private static List<object> CrearResumenSubtotales(string jsonSubtotal)
        {
            if (string.IsNullOrWhiteSpace(jsonSubtotal))
            {
                return [];
            }

            try
            {
                var subtotales = JsonConvert.DeserializeObject<List<FactSubtotalJsonDto>>(jsonSubtotal)
                    ?? [];

                return subtotales
                    .Select(x => new
                    {
                        orden = x.orden,
                        tipo = x.tipo,
                        concepto = x.concepto,
                        @base = x.@base,
                        alicuota = x.alicuota,
                        importe = x.importe,
                        id_aux = x.id_aux
                    })
                    .Cast<object>()
                    .ToList();
            }
            catch
            {
                return [];
            }
        }

        private async Task<object> RegistrarStockNotaCredito(
            ComprobanteInfoDto comprobante,
            string token)
        {
            try
            {
                var depoId = CajaActual?.Caja?.depo_id ?? string.Empty;

                if (string.IsNullOrWhiteSpace(depoId))
                {
                    _logger?.LogWarning(
                        "NC Devolución: no se encontró depo_id para cargar stock."
                    );

                    return new
                    {
                        ok = false,
                        mensaje = "La Nota de Crédito fue emitida, pero no se encontró depósito para actualizar stock."
                    };
                }

                var stockId = $"{comprobante.tco_id}{comprobante.cm_compte}{comprobante.cm_repetido}";
                var stockRequest = new CargaStkDto
                {
                    box_id = depoId,
                    tipo = "FV",
                    id = stockId
                };

                var stockResult = await _cajaServicio.CargaStkDeFactura(stockRequest, token);

                if (stockResult == null || !stockResult.Ok)
                {
                    _logger?.LogError(
                        "NC Devolución: error al actualizar stock. Mensaje={Mensaje}",
                        stockResult?.Mensaje ?? "Respuesta nula del servicio."
                    );

                    return new
                    {
                        ok = false,
                        mensaje = stockResult?.Mensaje ?? "La Nota de Crédito fue emitida, pero no se pudo actualizar stock."
                    };
                }

                return new
                {
                    ok = true,
                    mensaje = "Stock actualizado correctamente.",
                    id = stockId
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(
                    ex,
                    "NC Devolución: excepción al actualizar stock."
                );

                return new
                {
                    ok = false,
                    mensaje = "La Nota de Crédito fue emitida, pero ocurrió un error al actualizar stock."
                };
            }
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
