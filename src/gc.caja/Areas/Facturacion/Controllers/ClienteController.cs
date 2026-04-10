using Microsoft.AspNetCore.Mvc;

namespace gc.caja.Areas.Facturacion.Controllers
{
    public class ClienteController : Controller
    {
        public IActionResult Presenta()
        {
            return View();
        }
    }
}
