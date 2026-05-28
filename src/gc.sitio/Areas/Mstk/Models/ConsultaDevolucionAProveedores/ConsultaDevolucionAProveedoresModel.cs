using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ConsultaDevolucionAProveedoresModel
	{
		public SelectList ListaSucursales { get; set; }
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
