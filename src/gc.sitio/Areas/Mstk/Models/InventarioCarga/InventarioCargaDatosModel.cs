using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InventarioCargaDatosModel
	{
		public SelectList ListaDepositos { get; set; }
		public SelectList ListaConteos { get; set; }
		public string Descripcion { get; set; } = string.Empty;
		public SelectList ListaEstado { get; set; }
		public string AS_N { get; set; } = string.Empty;
		public DateTime AperturaDesde { get; set; }
		public DateTime AperturaHasta { get; set; }
	}
}
