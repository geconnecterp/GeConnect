using gc.caja.Controllers;
using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.caja.Areas.Facturacion.Controllers
{
    [Area("Facturacion")]
    public class ProductoFactController : ControladorBaseCaja
    {
        private readonly ICajaServicio _cajaServicio;

        public ProductoFactController(
            IOptions<AppSettings> options,
            ICajaServicio cajaServicio, 
            IHttpContextAccessor httpContext,
            ILogger<InicioController> logger) : base(options, httpContext, logger)
        {
            _cajaServicio = cajaServicio; // ✅ ASIGNAR
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
