
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.OrdenDePago.Request
{
	public class AnularCertificadoDeOrdenDePagoRequest : RequestBase
	{
		public string op_compte { get; set; } = string.Empty;
		public string imp_id { get; set; } = string.Empty;
	}
}
