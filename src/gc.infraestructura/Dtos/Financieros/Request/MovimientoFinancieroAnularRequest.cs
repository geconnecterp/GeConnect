
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros
{
	public class MovimientoFinancieroAnularRequest : RequestBase
	{
		public string tra_compte { get; set; } = string.Empty;
	}
}
