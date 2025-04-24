using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRDetallePedidoDto : Dto
	{
		public string Titulo { get; set; } = string.Empty;
		public GridCoreSmart<TRAutPIDetalleDto> Detalle { get; set; }
		public TRDetallePedidoDto()
		{
			Detalle = new GridCoreSmart<TRAutPIDetalleDto>();
		}
	}
}
