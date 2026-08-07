using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class AnulacionCobranzaController : ControladorBaseCaja
    {
        private const string MODULO = "AnulacionCobranza";
        private const string MODULO_DESC = "Anulacion de Cobranza";
        private const string SESSION_COBRANZAS = "AnulacionCobranza_Cobranzas";

        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly IAnulacionCobranzaServicio _anulacionCobranzaServicio;

        public AnulacionCobranzaController(
            IOptions<AppSettings> options,
            ILogger<AnulacionCobranzaController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            IAnulacionCobranzaServicio anulacionCobranzaServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _anulacionCobranzaServicio = anulacionCobranzaServicio;
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
                TempData["Error"] = "No se encontraron datos validos de caja para iniciar anulacion de cobranza.";
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

            ViewBag.Modulo = MODULO;
            ViewBag.ModuloDesc = MODULO_DESC;
            ViewBag.Usuario = UserName;
            ViewBag.CajaId = caja.CajaId;
            ViewBag.CajaNombre = caja.Caja.caja_nombre ?? string.Empty;
            ViewBag.Proceso = caja.Caja.caja_nro_proceso ?? string.Empty;
            ViewBag.Cierre = caja.Caja.caja_nro_cierre ?? string.Empty;

            CobranzasAnulacion = [];
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
                        "Anulacion cobranza: validacion inicial fallida. CajaId={CajaId}; Usuario={Usuario}; Motivo={Motivo}",
                        caja.CajaId,
                        UserName,
                        mensajeValidacion);

                    return Json(new { success = false, message = mensajeValidacion });
                }

                _logger?.LogInformation(
                    "Anulacion cobranza: validacion inicial exitosa. CajaId={CajaId}; Usuario={Usuario}",
                    caja.CajaId,
                    UserName);

                return Json(new { success = true, message = "Caja validada correctamente." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Anulacion cobranza: error inesperado en ValidacionInicial. Usuario={Usuario}", UserName);
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

                if (string.IsNullOrWhiteSpace(ctaId))
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "Debe seleccionar un cliente registrado con identificador de cuenta." });
                }

                if (origen == "F")
                {
                    return Json(new { ok = false, hayDatos = false, mensaje = "La anulacion de cobranza requiere un Cliente Registrado. No se permite Consumidor Final." });
                }

                return Json(new { ok = true, hayDatos = true, mensaje = "Cliente registrado validado." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Anulacion cobranza: error validando cliente registrado. Usuario={Usuario}", UserName);
                return Json(new { ok = false, hayDatos = false, mensaje = "No se pudo validar el cliente seleccionado." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> BuscarCobranzas([FromBody] AnulacionCobranzaBuscarVistaRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                var cliente = ClienteActual;
                var ctaId = (request?.cta_id ?? cliente?.cta_id ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(ctaId))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un cliente registrado antes de buscar cobranzas." });
                }

                if (string.Equals(cliente?.Origen?.Trim(), "F", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { ok = false, mensaje = "La anulacion de cobranza requiere un Cliente Registrado." });
                }

                var caja = CajaActual;
                if (caja?.Caja == null)
                {
                    return Json(new { ok = false, mensaje = "Los datos de caja no estan disponibles." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var fecha = request?.fecha?.Date ?? DateTime.Today;
                _ = int.TryParse(caja.Caja.caja_nro_cierre ?? string.Empty, out var cierre);

                var requestApi = new AnulacionCobranzaBuscarRequestDto
                {
                    caja_nro_proceso = caja.Caja.caja_nro_proceso ?? string.Empty,
                    caja_nro_cierre = cierre,
                    cta_id = ctaId,
                    fecha = fecha,
                    adm_id = caja.AdmId ?? AdministracionId,
                    usu_id = UserName ?? string.Empty
                };

                _logger?.LogInformation(
                    "Anulacion cobranza: buscando cobranzas. Cta={Cta}; Fecha={Fecha}; Proceso={Proceso}; Cierre={Cierre}; Adm={Adm}; Usuario={Usuario}",
                    requestApi.cta_id,
                    requestApi.fecha,
                    requestApi.caja_nro_proceso,
                    requestApi.caja_nro_cierre,
                    requestApi.adm_id,
                    UserName);

                var respuesta = await _anulacionCobranzaServicio.BuscarCobranzas(requestApi, token);
                var lista = respuesta.ListaEntidad ?? [];
                CobranzasAnulacion = lista;

                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Ok ? "OK" : respuesta.Mensaje,
                    lista
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Anulacion cobranza: error buscando cobranzas. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron obtener las cobranzas del cliente." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Anular([FromBody] AnulacionCobranzaConfirmarVistaRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request == null)
                {
                    return Json(new { ok = false, mensaje = "No se recibieron los datos de la cobranza a anular." });
                }

                var cliente = ClienteActual;
                var ctaId = (request.cta_id ?? cliente?.cta_id ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(ctaId))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un cliente registrado antes de anular una cobranza." });
                }

                if (string.Equals(cliente?.Origen?.Trim(), "F", StringComparison.OrdinalIgnoreCase))
                {
                    return Json(new { ok = false, mensaje = "La anulacion de cobranza requiere un Cliente Registrado." });
                }

                var caja = CajaActual;
                if (caja?.Caja == null || string.IsNullOrWhiteSpace(caja.CajaId))
                {
                    return Json(new { ok = false, mensaje = "Los datos de caja no estan disponibles." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                var cobranzas = CobranzasAnulacion;
                var registro = cobranzas.FirstOrDefault(x =>
                    string.Equals((x.cta_id ?? string.Empty).Trim(), ctaId, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals((x.caja_nro_proceso ?? string.Empty).Trim(), (request.caja_nro_proceso ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase) &&
                    x.caja_nro_cierre == request.caja_nro_cierre &&
                    x.caja_nro_operacion == request.caja_nro_operacion);

                if (registro == null)
                {
                    return Json(new { ok = false, mensaje = "La cobranza seleccionada ya no esta disponible para anular. Vuelva a buscarla." });
                }

                var requestApi = new AnulacionCobranzaConfirmarRequestDto
                {
                    caja_id = caja.CajaId ?? string.Empty,
                    usu_id = UserName ?? string.Empty,
                    adm_id = caja.AdmId ?? AdministracionId,
                    caja_nro_proceso_anu = registro.caja_nro_proceso ?? string.Empty,
                    caja_nro_cierre_anu = registro.caja_nro_cierre,
                    caja_nro_operacion_anu = registro.caja_nro_operacion,
                    cta_id = registro.cta_id ?? ctaId,
                    usu_id_autoriza = UserName ?? string.Empty
                };

                _logger?.LogInformation(
                    "Anulacion cobranza: enviando confirmacion. Cta={Cta}; Recibo={Recibo}; Importe={Importe}; ProcesoAnula={ProcesoAnula}; CierreAnula={CierreAnula}; OperacionAnula={OperacionAnula}; Caja={Caja}; Usuario={Usuario}",
                    requestApi.cta_id,
                    registro.rb_compte,
                    registro.co_cobranza,
                    requestApi.caja_nro_proceso_anu,
                    requestApi.caja_nro_cierre_anu,
                    requestApi.caja_nro_operacion_anu,
                    requestApi.caja_id,
                    UserName);

                var respuesta = await _anulacionCobranzaServicio.AnularCobranza(requestApi, token);
                var entidad = respuesta.Entidad;

                _logger?.LogInformation(
                    "Anulacion cobranza: respuesta confirmacion. Cta={Cta}; Recibo={Recibo}; Resultado={Resultado}; Mensaje={Mensaje}",
                    requestApi.cta_id,
                    registro.rb_compte,
                    entidad?.resultado,
                    entidad?.resultado_msj ?? respuesta.Mensaje);

                if (entidad == null)
                {
                    return Json(new { ok = false, mensaje = respuesta.Mensaje ?? "No se recibio respuesta de la anulacion." });
                }

                var ok = respuesta.Ok && entidad.resultado == 0;
                if (ok)
                {
                    CobranzasAnulacion = cobranzas
                        .Where(x => !(string.Equals((x.cta_id ?? string.Empty).Trim(), requestApi.cta_id.Trim(), StringComparison.OrdinalIgnoreCase)
                            && string.Equals((x.caja_nro_proceso ?? string.Empty).Trim(), requestApi.caja_nro_proceso_anu.Trim(), StringComparison.OrdinalIgnoreCase)
                            && x.caja_nro_cierre == requestApi.caja_nro_cierre_anu
                            && x.caja_nro_operacion == requestApi.caja_nro_operacion_anu))
                        .ToList();
                }

                return Json(new
                {
                    ok,
                    resultado = entidad.resultado,
                    resultado_id = entidad.resultado_id,
                    mensaje = string.IsNullOrWhiteSpace(entidad.resultado_msj) ? respuesta.Mensaje : entidad.resultado_msj,
                    recibo = registro.rb_compte,
                    importe = registro.co_cobranza
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Anulacion cobranza: error confirmando anulacion. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudo confirmar la anulacion de cobranza." });
            }
        }

        private List<AnulacionCobranzaResponseDto> CobranzasAnulacion
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString(SESSION_COBRANZAS) ?? string.Empty;
                return string.IsNullOrWhiteSpace(json)
                    ? []
                    : JsonConvert.DeserializeObject<List<AnulacionCobranzaResponseDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value ?? []);
                _context.HttpContext?.Session.SetString(SESSION_COBRANZAS, json);
            }
        }

        public sealed class AnulacionCobranzaBuscarVistaRequest
        {
            public string cta_id { get; set; } = string.Empty;
            public DateTime? fecha { get; set; }
        }

        public sealed class AnulacionCobranzaConfirmarVistaRequest
        {
            public string cta_id { get; set; } = string.Empty;
            public string caja_nro_proceso { get; set; } = string.Empty;
            public int caja_nro_cierre { get; set; }
            public int caja_nro_operacion { get; set; }
        }
    }
}
