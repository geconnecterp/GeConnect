using gc.infraestructura.Dtos.Almacen.RemitoExterno;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class RemitosExternosModel
	{
		public GridCoreSmart<RemitoExternoListaDto> GrillaRemitos { get; set; }
		public string Leyenda { get; set; }
	}
}
