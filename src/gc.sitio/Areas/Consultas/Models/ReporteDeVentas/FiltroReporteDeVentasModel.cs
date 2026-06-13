using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas
{
	public class FiltroReporteDeVentasModel
	{
		public SelectList ListaSucursales { get; set; }
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public bool HabilitarCambioDeSucursalSeleccionada { get; set; }
	}
}
