using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class FinancieroLiqDeEmpleadoAnularRequest : RequestBase
	{
		public string le_compte { get; set; } = string.Empty;
	}
}
