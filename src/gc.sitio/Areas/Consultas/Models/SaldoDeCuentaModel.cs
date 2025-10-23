using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class SaldoDeCuentaModel
	{
		public GridCoreSmart<SaldoDeCuentaDto> GrillaSaldoDeCuenta { get; set; }
	}
}
