using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models.ConsultaPlanillaDeElabFraccion
{
	public class ConsultaPlanillaDeElabFraccionModel
	{
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public SelectList ListaSucursales { get; set; }
		public string SucursalSeleccionada { get; set; } = string.Empty;
	}
}
