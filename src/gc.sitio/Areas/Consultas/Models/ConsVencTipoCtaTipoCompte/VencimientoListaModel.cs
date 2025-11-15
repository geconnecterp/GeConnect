using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class VencimientoListaModel
	{
		public GridCoreSmart<VencimientoListaDto> GrillaVencimientos { get; set; }
	}
}
