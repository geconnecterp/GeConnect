using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ResumenOCModel
	{
		public DateTime FechaEntrega { get; set; }
		public string AdmId { get; set; }
		public SelectList SucursalEntrega { get; set; }
		public bool PagoAnticipado { get; set; }
		public DateTime PagoPlazo { get; set; }
		public string Obs { get; set; }
		public bool DejarOCActiva { get; set; }
		public GridCoreSmart<OrdenDeCompraConceptoDto> ResumenGrilla { get; set; }
	}
}
