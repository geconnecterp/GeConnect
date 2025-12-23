using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ConteosEnValorizacionModel
	{
		public GridCoreSmart<ConteoEnValorizacionDto> GrillaConteos { get; set; }
		public int Conteo { get; set; } = 0;
	}
}
