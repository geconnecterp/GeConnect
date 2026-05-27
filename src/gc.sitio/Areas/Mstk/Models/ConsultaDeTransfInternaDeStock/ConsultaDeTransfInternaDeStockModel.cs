using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ConsultaDeTransfInternaDeStockModel
	{
		public SelectList ListaSucursalesEnvia { get; set; }
		public string SucursalEnviaSeleccionada { get; set; } = string.Empty;
		public SelectList ListaSucursalesRecibe { get; set; }
		public string SucursalRecibeSeleccionada { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public SelectList ListaTipos { get; set; }
		public string TipoSeleccionada { get; set; } = string.Empty;
	}
}
