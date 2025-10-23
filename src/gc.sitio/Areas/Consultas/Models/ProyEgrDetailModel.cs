using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ProyEgrDetailModel
	{
		public GridCoreSmart<ProyEgrDetailDto> GrillaProyEgrDetail { get; set; }
		public string fecha { get; set; }
	}
}
