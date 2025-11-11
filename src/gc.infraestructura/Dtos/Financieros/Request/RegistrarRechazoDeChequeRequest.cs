
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class RegistrarRechazoDeChequeRequest : RequestBase
	{
		public string ctaf_id { get; set; } = string.Empty;
		public int che_emision { get; set; }
		public DateTime fecha_rechazo { get; set; }
	}
}
