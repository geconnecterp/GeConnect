using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class CGUsuariosController : ControladorUsuariosBase
    {
        private readonly AppSettings _settings;
        private readonly ILogger<CGUsuariosController> _logger;


        public CGUsuariosController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<CGUsuariosController> logger) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }


            return View();
        }
    }
}
