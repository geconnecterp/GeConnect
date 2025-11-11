
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros
{
	public class FinancieroAnticipoAnularRequest : RequestBase
	{
		public string an_compte { get; set; } = string.Empty;
	}
}
