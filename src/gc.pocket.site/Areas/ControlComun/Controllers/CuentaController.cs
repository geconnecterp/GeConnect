using Microsoft.AspNetCore.Mvc;

namespace gc.pocket.site.Areas.ControlComun.Controllers
{
    [Area("ControlComun")]
    public class CuentaController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
