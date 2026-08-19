using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ConsultaDeMovDeStockModel
	{
		public DateTime FechaDesde { get; set; }
		public DateTime FechaHasta { get; set; }
		public SelectList ListaTipoMovimientos { get; set; }
		public string TipoMovimientoSeleccionado { get; set; } = string.Empty;
		public string Texto { get; set; } = string.Empty;
		public SelectList ListaBoxs { get; set; }
		public string BoxSeleccionado { get; set; } = string.Empty;
		public SelectList ListaDepositos { get; set; }
		public string DepositoSeleccionado { get; set; } = string.Empty;
	}
}
