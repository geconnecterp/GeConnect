using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Cajas.Request;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class RendicionController : ControladorBaseCaja
    {
        private readonly ICajaInitServicio _cajaInitServicio;
        private readonly IRendicionServicio _rendicionServicio;

        public RendicionController(
            IOptions<AppSettings> options,
            ILogger<RendicionController> logger,
            IHttpContextAccessor httpContext,
            ICajaInitServicio cajaInitServicio,
            IRendicionServicio rendicionServicio)
            : base(options, httpContext, logger)
        {
            _cajaInitServicio = cajaInitServicio;
            _rendicionServicio = rendicionServicio;
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
                TempData["Error"] = "No se encontraron datos validos de caja para iniciar rendiciones parciales.";
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }

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
                        "Rendiciones: validacion inicial fallida. CajaId={CajaId}; Usuario={Usuario}; Motivo={Motivo}",
                        caja.CajaId,
                        UserName,
                        mensajeValidacion);

                    return Json(new { success = false, message = mensajeValidacion });
                }

                _logger?.LogInformation(
                    "Rendiciones: validacion inicial exitosa. CajaId={CajaId}; Usuario={Usuario}",
                    caja.CajaId,
                    UserName);

                return Json(new
                {
                    success = true,
                    message = "Caja validada correctamente.",
                    caja_id = caja.CajaId,
                    caja_nombre = caja.Caja.caja_nombre ?? string.Empty,
                    usuario = UserName
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Rendiciones: error inesperado en ValidacionInicial. Usuario={Usuario}", UserName);
                return Json(new { success = false, message = "Error interno al validar los datos de caja." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarRendiciones()
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

                var request = new RendicionRequestDto
                {
                    adm_id = CajaActual?.AdmId ?? AdministracionId,
                    tipo = 'P'
                };

                _logger?.LogInformation(
                    "Rendiciones: cargando instrumentos. Adm={Adm}; Tipo={Tipo}; Usuario={Usuario}",
                    request.adm_id,
                    request.tipo,
                    UserName);

                var respuesta = await _rendicionServicio.CargarRendiciones(request, token);
                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Mensaje,
                    lista = respuesta.ListaEntidad ?? []
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Rendiciones: error cargando instrumentos. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron cargar los instrumentos de rendicion." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarNominaciones([FromBody] RendicionNominalRequestDto request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request == null || string.IsNullOrWhiteSpace(request.ins_id))
                {
                    return Json(new { ok = false, mensaje = "Debe seleccionar un instrumento para cargar nominaciones." });
                }

                var token = TokenCookie;
                if (string.IsNullOrWhiteSpace(token))
                {
                    return Json(new { ok = false, mensaje = "La sesion actual no posee un token valido." });
                }

                request.adm_id = CajaActual?.AdmId ?? AdministracionId;

                _logger?.LogInformation(
                    "Rendiciones: cargando nominaciones. Adm={Adm}; Instrumento={Instrumento}; Usuario={Usuario}",
                    request.adm_id,
                    request.ins_id,
                    UserName);

                var respuesta = await _rendicionServicio.CargarNominaciones(request, token);
                return Json(new
                {
                    ok = respuesta.Ok,
                    mensaje = respuesta.Mensaje,
                    lista = respuesta.ListaEntidad ?? []
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Rendiciones: error cargando nominaciones. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "No se pudieron cargar las nominaciones." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> Confirmar([FromBody] RendicionConfirmarRequest request)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult _))
                {
                    return Json(new { ok = false, mensaje = "La sesion ha expirado. Vuelva a iniciar sesion." });
                }

                if (request?.Rendiciones == null || request.Rendiciones.Count == 0)
                {
                    return Json(new { ok = false, mensaje = "No hay rendiciones para confirmar." });
                }

                var total = request.Rendiciones.Sum(x => x.ins_importe);
                if (total <= 0)
                {
                    return Json(new { ok = false, mensaje = "Debe cargar al menos un importe para confirmar la rendicion." });
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

                var cierreTexto = caja.Caja.caja_nro_cierre ?? string.Empty;
                _ = int.TryParse(cierreTexto, out var cierre);

                var requestApi = new RendicionCargaRequestDto
                {
                    caja_nro_proceso = caja.Caja.caja_nro_proceso ?? string.Empty,
                    caja_nro_cierre = cierre,
                    caja_id = caja.CajaId ?? string.Empty,
                    usu_id = UserName ?? string.Empty,
                    adm_id = caja.AdmId ?? AdministracionId,
                    json_rendiciones = JsonConvert.SerializeObject(request.Rendiciones)
                };

                _logger?.LogInformation(
                    "Rendiciones: confirmando rendicion. Caja={Caja}; Proceso={Proceso}; Cierre={Cierre}; Total={Total}; Registros={Registros}; Usuario={Usuario}",
                    requestApi.caja_id,
                    requestApi.caja_nro_proceso,
                    requestApi.caja_nro_cierre,
                    total,
                    request.Rendiciones.Count,
                    UserName);

                var respuesta = await _rendicionServicio.ConfirmarRendicion(requestApi, token);
                var entidad = respuesta.Entidad;

                if (entidad == null)
                {
                    return Json(new { ok = false, mensaje = respuesta.Mensaje ?? "No se recibio respuesta al confirmar la rendicion." });
                }

                return Json(new
                {
                    ok = respuesta.Ok && entidad.resultado == 0,
                    mensaje = entidad.resultado_msj ?? respuesta.Mensaje,
                    resultado = entidad.resultado,
                    resultado_id = entidad.resultado_id
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Rendiciones: error confirmando rendicion. Usuario={Usuario}", UserName);
                return Json(new { ok = false, mensaje = "Error interno al confirmar la rendicion parcial." });
            }
        }

        public sealed class RendicionConfirmarRequest
        {
            public List<RendicionConfirmarItem> Rendiciones { get; set; } = [];
        }

        public sealed class RendicionConfirmarItem
        {
            public string ins_id { get; set; } = string.Empty;
            public string ins_desc { get; set; } = string.Empty;
            public decimal ins_importe { get; set; }
        }
    }
}
