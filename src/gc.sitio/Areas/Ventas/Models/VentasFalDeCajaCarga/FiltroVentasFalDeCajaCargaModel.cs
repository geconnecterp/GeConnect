using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasFalDeCajaCarga
{
	public class FiltroVentasFalDeCajaCargaModel
	{
		public SelectList ListaSucursales { get; set; }
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public DateTime UltFaltanteProcesado { get; set; }
		public DateTime ProcesarHasta { get; set; }
		public bool MostrarUltFaltanteProcesado { get; set; } = false;
	}
}
