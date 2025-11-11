
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Consultas.ReporteFinanciero
{
	public class BaseRequest : RequestBase
	{
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
