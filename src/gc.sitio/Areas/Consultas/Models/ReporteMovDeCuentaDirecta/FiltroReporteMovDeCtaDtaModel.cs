using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas
{
	public class FiltroReporteMovDeCtaDtaModel
	{
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
