using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models.PedidoInternoConsulta
{
	public class PedidoInternoRTRDetalleModel
	{
		public GridCoreSmart<PIDetalleDto> DetalleRTR { get; set; }
		public string Leyenda { get; set; }
	}
}
