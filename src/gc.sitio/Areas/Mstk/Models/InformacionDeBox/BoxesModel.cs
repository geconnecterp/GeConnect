using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class BoxesModel
	{
		public GridCoreSmart<BoxInfoExtendedDto> GrillaBoxes { get; set; }
		public string Leyenda { get; set; } = "Filtros aplicados";
		public string LeyendaDeposito { get; set; } = string.Empty;
		public string LeyendaGondola { get; set; } = string.Empty;
		public string LeyendaNivel { get; set; } = string.Empty;
		public string LeyendaRack { get; set; } = string.Empty;
		public string LeyendaZona { get; set; } = string.Empty;
		public string LeyendaSoloLibres { get; set; } = string.Empty;
		public SelectList ListaTipoMovimientos { get; set; }
		public string TipoMovimientoSeleccionado { get; set; } = string.Empty;
		public DateTime FechaDesde { get; set; } = DateTime.Now;
		public DateTime FechaHasta { get; set; } = DateTime.Now;
	}
}
