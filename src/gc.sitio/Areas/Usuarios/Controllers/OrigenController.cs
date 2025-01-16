using Microsoft.AspNetCore.Mvc;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class OrigenController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
