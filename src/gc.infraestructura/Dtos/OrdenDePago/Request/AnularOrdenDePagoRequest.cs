
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class AnularOrdenDePagoRequest : RequestBase
	{
		public string op_compte { get; set; } = string.Empty;
	}
}
