using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class InventarioCargaController : InventarioCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IInventarioServicio _inventarioServicio;
		public InventarioCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioCargaController> logger,
										 IInventarioServicio inventarioServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
		}

		public IActionResult Index()
		{
			return View();
		}
	}
}
