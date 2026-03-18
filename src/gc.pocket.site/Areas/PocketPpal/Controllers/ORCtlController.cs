using Azure;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ORCtlController : PocketControllerBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly IORServicio _orServicio;
        public ORCtlController(IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ILogger<TrIntController> logger,
            IORServicio oRServicio,
            IOptions<MenuSettings> options1) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _orServicio = oRServicio;
        }



        public IActionResult Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
            //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
            var sigla = "CTL-OR";
            string? volver = Url.Action("index", "home", new { area = "" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;

            return View();
        }


        [HttpGet]
        public IActionResult PresentaProductosOrCtl(string or_compte)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(or_compte))
            {
                TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                return RedirectToAction("index");
            }

            // ✅ REFACTORIZADO: Usar ORSession
            //la inicializacion con nuevo comprobante
            var session = new ORSessionDto();
            session.ORComprobanteActual = or_compte;
            session.UltimaActualizacion = DateTime.Now;
            ORSession = session;

            _logger?.LogInformation("📝 OR Seleccionada: {OrCompte}", or_compte);

            var sigla = "CTL-OR";
            string? volver = Url.Action("index", "orctl", new { area = "PocketPpal" });
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = session.ORComprobanteActual;

            return View();
        }


        //la idea es buscar los registros y cargarlos en el grid
        [HttpPost]
        public async Task<IActionResult> CargaProductosOrCtl(string or_compte)
        {
            try
            {
                if (string.IsNullOrEmpty(or_compte))
                {
                    TempData["error"] = "No se recepcionó el Nro de Comprobante de la OR.";
                    return RedirectToAction("index");
                }

                var prod = await _orServicio.ObtenerListaProductosOrCtl(or_compte, UserName, TokenCookie);

                if (!prod.Ok || prod == null)
                {
                    return Json(new { success = false, message = prod?.Mensaje ?? " No se encontraron los productos de la OR. Intente de nuevo más tarde." });
                }
                else
                {
                    return Json(new { success = true, message = "", data = prod });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al intentar obtener los productos");
                return Json(new { success = false, message = "Hubo algún tipo de error y no se pudo obtener los Productos. " });
            }
        }

        [HttpGet]
        public IActionResult ORValidaProducto(string or_compte, string p_id, bool nuevo = false)
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            if (string.IsNullOrEmpty(p_id) && !nuevo)
            {
                TempData["error"] = "No se recepcionó el ID del Producto.";
                return RedirectToAction("ORCargaCarrito");
            }

            // Configurar ViewBag
            var sigla = "CTL-OR";
            string? volver = Url.Action("PresentaProductosOrCtl", "orctl",
                new { area = "PocketPpal", or_compte });

            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x =>
                x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));

            if (modulo == null)
            {
                throw new NegocioException("No se logró encontrar la configuración del Módulo.");
            }

            modulo.VolverUrl = volver ?? "#";
            ViewBag.AppItem = modulo;
            ViewBag.Compte = or_compte;

            return View((string.Empty, or_compte));
        }

        public async Task<IActionResult> ResguardarProductoCarritoORCtl(string p_id, int up, int bulto, decimal unid, decimal cantidad, DateTime? fv)
        {

            if (string.IsNullOrEmpty(p_id))
            {
                TempData["error"] = "No se recepcionó el ID del Producto.";
                return RedirectToAction("ORValidaProducto");
            }
            // Aquí iría la lógica para resguardar el producto en el carrito, utilizando _orServicio.ResguardarProductoCarrito
            // Por ahora, redirigimos de vuelta a la vista de validación
            return RedirectToAction("ORValidaProducto", new { or_compte = ORSession?.ORComprobanteActual, p_id });
        }

        [HttpPost]
        public async Task<IActionResult> ResguardarProductoCarritoORCtl([FromBody] OrCtlCargaProductoDto request)
        {
            try
            {
                // Validación básica de datos recibidos
                if (request == null)
                {
                    return Json(new { error = true, warn = false, msg = "No se recibieron datos del producto." });
                }

                if (string.IsNullOrWhiteSpace(request.p_id))
                {
                    return Json(new { error = true, warn = false, msg = "No se especificó el producto a cargar." });
                }

                if (request.cantidad <= 0)
                {
                    return Json(new { error = false, warn = true, msg = "Las cantidades de los productos a cargar deben ser positivas, mayores a 0 (cero)." });
                }

                // Validar unidad de presentación para productos pesables (UP_ID != "07")
                if (!string.IsNullOrWhiteSpace(request.up_id) && !request.up_id.Equals("07") && request.unidad_pres != 1)
                {
                    return Json(new { error = false, warn = true, msg = "El producto no es por unidades. La unidad de presentación tiene que ser igual a 1 siempre." });
                }

                // Completar campos faltantes del request
                if (string.IsNullOrWhiteSpace(request.or_compte))
                {
                    // Obtener or_compte desde la sesión
                    request.or_compte = ORSession?.ORComprobanteActual ?? string.Empty;
                }

                if (string.IsNullOrWhiteSpace(request.or_compte))
                {
                    return Json(new { error = true, warn = false, msg = "No se especificó el número de comprobante de la OR." });
                }

                // Completar usuario
                if (string.IsNullOrWhiteSpace(request.usu_id))
                {
                    request.usu_id = UserName;
                }

                // Validar fecha de vencimiento (opcional según negocio)
                if (string.IsNullOrWhiteSpace(request.vto))
                {
                    request.vto = "19700101"; // Fecha por defecto si no tiene vencimiento
                }

                request.item = 1;

                // Serializar el request completo a JSON
                var jsonRequest = JsonConvert.SerializeObject(request);

                _logger?.LogInformation("📦 Cargando producto OR Control: {PId}, Cantidad: {Cantidad}", request.p_id, request.cantidad);

                // Invocar servicio de carga de producto controlado
                var resp = await _orServicio.CargaProductoORCtl(jsonRequest, TokenCookie);

                if (!resp.Ok)
                {
                    _logger?.LogWarning("⚠️ Error al cargar producto: {Mensaje}", resp.Mensaje);
                    return Json(new { error = resp.EsError, warn = resp.EsWarn, msg = resp.Mensaje ?? "Error al cargar el producto." });
                }

                // Respuesta exitosa
                var entidad = resp.Entidad;
                var mensaje = entidad?.resultado_msj ?? $"Producto {request.p_desc} fue cargado exitosamente";

                _logger?.LogInformation("✅ Producto cargado exitosamente: {PId}", request.p_id);

                return Json(new { error = false, warn = false, msg = $"✅ Producto cargado exitosamente: {request.p_id}" });
            }
            catch (NegocioException ex)
            {
                _logger?.LogWarning(ex, "❌ Error de negocio al cargar producto OR Control: {Message}", ex.Message);
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                _logger?.LogWarning(ex, "❌ Error de autorización al cargar producto OR Control");
                return Json(new { error = false, warn = true, msg = "No tiene permisos para realizar esta operación." });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "❌ Error inesperado al cargar producto OR Control");
                return Json(new { error = true, warn = false, msg = "Ocurrió un error inesperado. Intente nuevamente." });
            }
        }
    }
}
