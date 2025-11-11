
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Financieros.Request
{
	public class SetExtractoBancarioConfirmaRequest : RequestBase
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string json_extracto { get; set; } = string.Empty;
		public string json_eliminado { get; set; } = string.Empty;
	}
}
