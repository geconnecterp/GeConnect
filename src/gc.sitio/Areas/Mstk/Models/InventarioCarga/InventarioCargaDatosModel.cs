using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InventarioCargaDatosModel
	{
		public SelectList ListaDepositos { get; set; }
		public string DepositoSeleccionado { get; set; } = string.Empty;
		public SelectList ListaConteos { get; set; }
		public string ConteoSeleccionado { get; set; } = string.Empty;
		public string Descripcion { get; set; } = string.Empty;
		public string Estado { get; set; } = string.Empty;
		//public SelectList ListaEstado { get; set; }
		public string AS_N { get; set; } = string.Empty;
		public DateTime AperturaDesde { get; set; }
		public DateTime AperturaHasta { get; set; }
		public string inv_nro { get; set; } = string.Empty;
	}
}
