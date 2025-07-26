using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models.AnulacionDeComprobante
{
	public class AnulacionDeComprobanteModel
	{
		public GridCoreSmart<ComprobanteParaAnularDto> GrillaComprobantes { get; set; }
	}
}
