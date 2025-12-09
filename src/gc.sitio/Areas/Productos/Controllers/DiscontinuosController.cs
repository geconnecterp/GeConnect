using DocumentFormat.OpenXml.Spreadsheet;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Discontinuo;
using gc.infraestructura.Dtos.Productos.Impositivo;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class DiscontinuosController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;
        private readonly IDiscontinuoServicio _discSv;

        public DiscontinuosController(IOptions<AppSettings> options,
            IHttpContextAccessor httpContext,
            ILogger<DiscontinuosController> logger,
            IDiscontinuoServicio servicio) : base(options, httpContext, logger)
        {
            _configuracion = options.Value;
            _discSv = servicio;
        }
        public IActionResult Index()
        {
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            string titulo = "Pasar a Discontinuos, pasar a Continuos.";
            ViewData["Titulo"] = titulo;

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ObtenerDatos([FromBody] DiscontinuoReqDto request)
        {
            // ✅ AGREGAR LOGGING PARA DEBUGGING
            _logger?.LogInformation("📥 ObtenerDatos - Inicio");

            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });

                var opcion = request.opcion.ToIntOrNull();
                if (opcion == null || (opcion > 3 || opcion < 1))
                {
                    _logger?.LogWarning("⚠️ JSON es null");
                    throw new NegocioException("Parámetros inválidos.");
                }

                if (opcion != 1 && request.lista is null)
                {
                    _logger?.LogWarning("⚠️ JSON es null");
                    throw new NegocioException("Parámetros inválidos.");
                }

                QueryFilters filters = new QueryFilters();

                filters.Id = request.opcion;
                if (request.lista != null && request.lista.Count > 0)
                {
                    filters.Rel01 = request.lista;
                }

                filters.Adm_id = AdministracionId;
                filters.Usu_id = UserName;

                RespuestaGenerica<DiscontinuoDetalleDto> resp = await _discSv.ObtenerProductosDiscontinuos(filters, TokenCookie);

                if (!resp.Ok)
                {
                    _logger?.LogError("❌ Error en servicio: {Mensaje}", resp.Mensaje);
                    throw new NegocioException(resp.Mensaje ?? "Error al obtener los productos discontinuos.");
                }

                var ordenada = resp.ListaEntidad?.OrderBy(x => x.p_desc).ToList();

                _logger?.LogInformation("✅ Registros obtenidos: {Count}", ordenada?.Count ?? 0);

                return Json(new { error = false, warn = false, lista = ordenada });
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Hubo un problema al intentar recepcionar los datos.");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Hubo un problema al intentar recepcionar los datos.");
                return Json(new { error = true, msg = "Hubo un problema al intentar recepcionar los datos. Si el problema persiste informe al administrador del sistema" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarDiscontinuos([FromBody] DiscontinuoReqDto request)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });

                var opcion = request.opcion.ToIntOrNull();
                if (opcion == null || (opcion > 3 || opcion < 1))
                {
                    _logger?.LogWarning("⚠️ JSON es null");
                    throw new NegocioException("Parámetros inválidos.");
                }

                if (request.lista is null || request.lista.Count == 0)
                {
                    _logger?.LogWarning("⚠️ JSON es null");
                    throw new NegocioException("Al menos debe seleccionar un Producto a Discontinuar.");
                }
                var lista = request.lista.Select(x => new { p_id = x }).ToList();
                var abm = new AbmGenDto
                {
                    Objeto = request.opcion,
                    Json = JsonConvert.SerializeObject(lista),
                    Administracion = AdministracionId,
                    Usuario = UserName
                };


                RespuestaGenerica<RespuestaDto> resp = await _discSv.ConfirmarDiscontinuo(abm, TokenCookie);

                if (!resp.Ok)
                {
                    if (resp.EsWarn)
                    {
                        _logger?.LogError("❌ Error en servicio: {Mensaje}", resp.Mensaje);
                        throw new NegocioException(resp.Mensaje ?? "Error al obtener los productos discontinuos.");
                    }
                    else
                    {
                        _logger?.LogError("❌ Error en servicio: {Mensaje}", resp.Mensaje);
                        throw new Exception(resp.Mensaje ?? "Error al obtener los productos discontinuos.");
                    }
                }
                else
                {
                    return Json(new { error = false, warn = false, msg="El proceso se ejecutó satifactoriamente."});
                }
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Hubo un problema al intentar recepcionar los datos.");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Hubo un problema al intentar recepcionar los datos.");
                return Json(new { error = true, msg = "Hubo un problema al intentar recepcionar los datos. Si el problema persiste informe al administrador del sistema" });
            }
        }
    }
}
