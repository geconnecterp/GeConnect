using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeStockValorizadoController : Controller
	{
		private readonly AppSettings _setting;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IConsultasServicio _consultaServicio;


		public IActionResult Index()
		{
			return View();
		}
	}
}
