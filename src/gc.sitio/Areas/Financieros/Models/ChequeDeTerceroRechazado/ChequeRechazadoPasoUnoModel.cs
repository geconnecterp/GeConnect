using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ChequeRechazadoPasoUnoModel
	{
		public SelectList ListaCuentasBancarias { get; set; }
		public string ctaSelected { get; set; } = string.Empty;
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
	}
}
