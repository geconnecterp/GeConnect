using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class FiltroConciliacionExtractoModel
	{
		public SelectList CuentaBanco { get; set; }
		public string selectedValue { get; set; } = string.Empty;
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public bool CargarRegistrosNoConciliados { get; set; }
		public bool CargarConciliacionAutomatica { get; set; }
	}
}
