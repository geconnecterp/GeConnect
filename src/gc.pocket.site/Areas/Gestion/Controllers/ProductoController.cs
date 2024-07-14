using Microsoft.AspNetCore.Mvc;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    public class ProductoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
