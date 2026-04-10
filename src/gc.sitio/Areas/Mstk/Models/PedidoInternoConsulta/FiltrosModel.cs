using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models.PedidoInternoConsulta
{
	public class FiltrosModel
	{
		public SelectList ListaSucursales { get; set; }
		public SelectList ListaEstados { get; set; }
		public DateTime FechaDesde { get; set; } = DateTime.Today.AddDays(-30);
		public DateTime FechaHasta { get; set; } = DateTime.Today;
		public string SucursalSeleccionada { get; set; } = string.Empty;
		public string EstadoSeleccionado { get; set; } = string.Empty;
	}
}
