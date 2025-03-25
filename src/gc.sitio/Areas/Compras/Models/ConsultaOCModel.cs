using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Compras.Models
{
	public class ConsultaOCModel
	{
		public GridCore<OrdenDeCompraConsultaDto> GrillaOC { get; set; }
		public decimal Importe { get; set; }
	}
}
