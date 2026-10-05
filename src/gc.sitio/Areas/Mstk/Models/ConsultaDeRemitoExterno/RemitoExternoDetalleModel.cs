using gc.infraestructura.Dtos.Almacen.RemitoExterno;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class RemitoExternoDetalleModel
	{
		public GridCoreSmart<RemitoExternoDetalleDto> GrillaRemitoDetalle { get; set; }
		public string Leyenda { get; set; }
	}
}
