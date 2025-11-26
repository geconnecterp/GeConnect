using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ImprEtController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<RPRController> _logger;
        private readonly IProductoServicio _productoServicio;
        private readonly AppSettings _settings;

        public ImprEtController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<RPRController> logger, IProductoServicio productoServicio, IDepositoServicio depositoServicio) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
            _settings = options.Value;
        }

        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            string volver = Url.Action("cprev", "almacen", new { area = "gestion" }) ?? "#";
            ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarCargaPrevia(string json)
        {
            string msg = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    throw new NegocioException("Para confirmar la Carga Previa, se debe enviar al menos un producto.");
                }

                var req = new AbmGenDto
                {
                    Json = json,
                    Usuario = UserName,
                    Administracion = AdministracionId
                };

                // Llamada al servicio
                RespuestaGenerica<RespuestaDto> respuesta = await _productoServicio.ConfirmarCargaPrevia(req, TokenCookie);

                // Procesamiento de respuesta
                if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
                {
                    msg = "La Carga previa se realizó exitosamente";
                    // Log y limpieza de datos temporales
                    _logger?.LogInformation(msg);

                    // Respuesta de éxito
                    return Json(new
                    {
                        ok = true,
                        error = false,
                        msg 
                    });
                }
                else
                {
                    if (respuesta.EsError)
                    {
                        msg = "Error al procesar la Carga Previa";
                    }
                    else if (respuesta.EsWarn)
                    {
                        msg = "Advertencia al procesar la Carga Previa";
                    }

                    _logger?.LogWarning(msg);
                    return Json(new
                    {
                        ok = false,
                        error = respuesta.EsError,
                        warn = respuesta.EsWarn,
                        msg = respuesta.Mensaje ?? msg
                    });
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones no esperadas
                _logger?.LogError(ex, "Error inesperado al confirmar combo/promoción");
                return Json(new
                {
                    ok = false,
                    error = true,
                    msg = "Error interno al procesar la solicitud"
                });
            }
        }

        //[HttpGet]
        //public IActionResult LabMenu()
        //{
        //    string volver = Url.Action("cprev", "almacen", new { area = "gestion" }) ?? "#";
        //    ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

        //    return View();
        //}
    }
}
