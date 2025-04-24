
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRNuevaAutDto : Dto
	{
        public GridCoreSmart<TRNuevaAutSucursalDto> Sucursales { get; set; }
        public GridCoreSmart<TRNuevaAutDetalleDto> Detalle { get; set; }
        public TRNuevaAutDto() {
            Sucursales = new GridCoreSmart<TRNuevaAutSucursalDto>();
            Detalle = new GridCoreSmart<TRNuevaAutDetalleDto>();
        }
    }
}
