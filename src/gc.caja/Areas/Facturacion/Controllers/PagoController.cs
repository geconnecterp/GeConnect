using Microsoft.AspNetCore.Mvc;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class PagoController : Controller
    {
        /// <summary>
        /// Vista principal del módulo de pagos
        /// </summary>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Renderiza la vista parcial del modal de pago
        /// </summary>
        public IActionResult GetPagoModal()
        {
            return PartialView("_pagoModal");
        }
    }
}
