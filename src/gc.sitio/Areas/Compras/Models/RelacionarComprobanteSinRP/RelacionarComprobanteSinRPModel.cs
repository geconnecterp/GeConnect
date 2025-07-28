using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class RelacionarComprobanteSinRPModel
	{
		public GridCoreSmart<CompteJbiDto> GrillaComprobantes { get; set; }
		public GridCoreSmart<CompteRPDto> GrillaRP { get; set; }
	}
}
