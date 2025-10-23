using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class FlujoDeIngresoModel
	{
		public GridCoreSmart<FlujoDeIngresoDto> GrillaProyFinan { get; set; }
	}
}
