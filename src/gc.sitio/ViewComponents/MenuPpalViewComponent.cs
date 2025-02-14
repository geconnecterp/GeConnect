using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace gc.sitio.ViewComponents
{
    public class MenuPpalViewComponent : ViewComponent
    {
        private readonly IHttpContextAccessor _context;
        private readonly ILogger<MenuPpalViewComponent> _logger;
        private readonly IMenuesServicio _mnSv;
        private readonly AppSettings _appSettings;

        public MenuPpalViewComponent(IHttpContextAccessor accessor, ILogger<MenuPpalViewComponent> logger, IMenuesServicio menuesServicio, IOptions<AppSettings> options)
        {
            _context = accessor;
            _logger = logger;
            _mnSv = menuesServicio;
            _appSettings = options.Value;
        }

        public IViewComponentResult Invoke()
        {
            RespuestaGenerica<MenuPpalDto> menu = new RespuestaGenerica<MenuPpalDto>() { Ok = false, Mensaje = "No se pudo generar el menu" };
            try
            {
                var p = _context.HttpContext.Session.GetString("UserPerfilSeleccionado");
                if (p == null)
                {
                    throw new Exception("No se localizó el perfil");
                }
                var perfil = JsonConvert.DeserializeObject<PerfilUserDto>(p);
                var adm = _context.HttpContext.Session.GetString("ADMID");
                var etiqueta = _context.HttpContext.Session.GetString("Etiqueta");
                var token = _context.HttpContext.Request.Cookies[etiqueta];


                menu = _mnSv.ObtenerMenu(perfil.perfil_id, perfil.usu_id, _appSettings.MenuId, adm, token).GetAwaiter().GetResult();
              

                    return View("Default", menu);
                
            }
            catch (Exception)
            {
                TempData["error"] = "Hubo un problema al intentar presentar el menu";
                return View("Default", menu);
            }
        }
    }
}
