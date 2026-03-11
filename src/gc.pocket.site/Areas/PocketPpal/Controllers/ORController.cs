using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.OrdenReparto;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class ORController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly IORServicio _orServicio;
        private readonly AppSettings _appSettings;

        public ORController(IOptions<AppSettings> options,
            IHttpContextAccessor context,
            ILogger<TrIntController> logger,
            IOptions<MenuSettings> options1,
            IORServicio oRServicio,
            IOptions<AppSettings> options2) : base(options, context, logger)
        {
            _menuSettings = options1.Value;
            _orServicio = oRServicio;
            _appSettings = options2.Value;  
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
            var sigla = "OR";
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

        [HttpPost]
        public async Task<JsonResult> ObtenerOrdenesReparto()
        {
            try
            {
                var resp = await _orServicio.ObtenerOrdenesReparto(new ORRequestDto
                {
                    HasFecha = false,
                    Desde = new DateTime(1900, 01, 01),
                    Hasta = new DateTime(2900, 01, 01),
                    HasEstado = true,
                    Ore_list = "O,",
                    HasRepartidor = false,
                    RP_List = string.Empty,
                    HasId = false,
                    OR_Compte = string.Empty,
                    Registros = _appSettings.NroRegistrosPagina,
                    Pagina = 1
                }, TokenCookie);

                if(resp.Ok)
                {
                    return Json(new { success = true, data = resp.ListaEntidad });
                }
                else
                {
                    return Json(new { success = false, message = resp.Mensaje });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// ✅ NUEVA ACTION: Valida si el usuario puede procesar la orden de reparto
        /// </summary>
        /// <param name="orCompte">ID del comprobante de orden de reparto</param>
        /// <param name="usuId">ID del usuario a validar</param>
        /// <returns>Resultado de la validación</returns>
        [HttpPost]
        public async Task<JsonResult> ValidarUsuario(string orCompte, string usuId)
        {
            try
            {
                // Validación de entrada
                if (string.IsNullOrWhiteSpace(orCompte))
                {
                    _logger?.LogWarning("⚠️ Validación fallida: ID de orden vacío");
                    return Json(new { 
                        success = false, 
                        message = "ID de orden de reparto requerido" 
                    });
                }

                if (string.IsNullOrWhiteSpace(usuId))
                {
                    _logger?.LogWarning("⚠️ Validación fallida: ID de usuario vacío");
                    return Json(new { 
                        success = false, 
                        message = "ID de usuario requerido" 
                    });
                }

                _logger?.LogInformation("📡 Validando usuario {UsuId} para orden {OrCompte}", 
                    usuId, orCompte);

                // Invocar servicio de validación
                var resultado = await _orServicio.ValidarUsuario(
                    orCompte, 
                    usuId, 
                    TokenCookie
                );

                if (resultado == null)
                {
                    _logger?.LogError("❌ Respuesta nula del servicio de validación");
                    return Json(new { 
                        success = false, 
                        message = "Error al validar usuario" 
                    });
                }

                if (!resultado.Ok)
                {
                    _logger?.LogWarning("⚠️ Validación de usuario fallida: {Mensaje}", 
                        resultado.Mensaje);
                    return Json(new { 
                        success = false, 
                        message = resultado.Mensaje ?? "Validación de usuario fallida" 
                    });
                }

                _logger?.LogInformation("✅ Usuario validado correctamente para orden {OrCompte}", 
                    orCompte);

                return Json(new { 
                    success = true, 
                    message = "Usuario validado correctamente",
                    data = resultado.Entidad
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, 
                    "❌ Error al validar usuario {UsuId} para orden {OrCompte}", 
                    usuId, orCompte);
                
                return Json(new { 
                    success = false, 
                    message = $"Error al validar usuario: {ex.Message}" 
                });
            }
        }
    }
}
