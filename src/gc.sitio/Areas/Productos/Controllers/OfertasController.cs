using Microsoft.AspNetCore.Mvc;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class OfertasController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
