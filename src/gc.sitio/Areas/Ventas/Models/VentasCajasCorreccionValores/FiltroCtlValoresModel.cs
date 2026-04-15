using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	public class FiltroCtlValoresModel
	{
		public SelectList ListaSucursales { get; set; }
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public SelectList ListaDias { get; set; }
		public string DiaSeleccionado { get; set; } = string.Empty;
	}
}
