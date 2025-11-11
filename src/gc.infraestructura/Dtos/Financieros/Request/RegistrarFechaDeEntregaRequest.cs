
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class RegistrarFechaDeEntregaRequest : RequestBase
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
	}
}
