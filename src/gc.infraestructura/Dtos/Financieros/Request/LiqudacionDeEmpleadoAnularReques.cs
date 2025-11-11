
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class LiqudacionDeEmpleadoAnularReques : RequestBase
	{
		public string le_compte { get; set; } = string.Empty;
	}
}
