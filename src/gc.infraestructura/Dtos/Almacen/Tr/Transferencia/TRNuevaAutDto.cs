
using gc.infraestructura.Dtos.Gen;

namespace gc.infraestructura.Dtos.Almacen.Tr.Transferencia
{
	public class TRNuevaAutDto : Dto
	{
        public GridCore<TRNuevaAutSucursalDto> Sucursales { get; set; }
        public GridCore<TRNuevaAutDetalleDto> Detalle { get; set; }
        public TRNuevaAutDto() {
            Sucursales = new GridCore<TRNuevaAutSucursalDto>();
            Detalle = new GridCore<TRNuevaAutDetalleDto>();
        }
    }
}
