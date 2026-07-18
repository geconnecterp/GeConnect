using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ListadoRankingModel
	{
		public GridCoreSmart<RepRkgRentabVtasDto> GrillaProductoRnk { get; set; }
		public int AgrupadoPor { get; set; } = 0;
		public string Leyenda { get; set; }
	}
}
