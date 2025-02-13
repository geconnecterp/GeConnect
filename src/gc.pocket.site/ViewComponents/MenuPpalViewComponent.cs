using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace gc.pocket.site.ViewComponents
{
    public class MenuPpalViewComponent : ViewComponent
    {
        private readonly MenuSettings _settings;
        private readonly IHttpContextAccessor _context;
        private readonly ILogger<MenuPpalViewComponent> _logger;

        public MenuPpalViewComponent(IHttpContextAccessor context, IOptions<MenuSettings> options, ILogger<MenuPpalViewComponent> logger)
        {
            _context = context;
            _settings = options.Value;
            _logger = logger;
        }


        public IViewComponentResult Invoke()
        {
            try
            {
                ///"DEBO CREAR EL COMPONENTE PARA PRESENTAR LOS BOTONES DEL MENU EN PANTALLA EN LA VISTA DEFAULT DE ESTE VIEWCOMPONENT";
                 return View("Default", _settings);
            }
            catch (Exception)
            {
                TempData["error"] = "Hubo un problema al intentar presentar el menu";
                return View("Default", new MenuSettings() { Aplicaciones = [] });
            }
        }
    }
}
