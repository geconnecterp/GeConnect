using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ConsultaOCDetalleModel
	{
		public GridCoreSmart<OrdenDeCompraDetalleDto> GrillaDetalle { get; set; }
		public GridCoreSmart<OrdenDeCompraConceptoDto> ResumenGrilla { get; set; }
		public DateTime FechaEntrega { get; set; }
		public string SucursalEntrega { get; set; } = string.Empty;
		public bool PagoAnticipado { get; set; }
		public DateTime PagoPlazo { get; set; }
		public string Observaciones { get; set; } = string.Empty;
	}
}
