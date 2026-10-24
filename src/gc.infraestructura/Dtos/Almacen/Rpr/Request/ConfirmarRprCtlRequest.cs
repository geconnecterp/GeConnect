using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class ConfirmarRprCtlRequest : RequestBase
	{
		public string rp_compte { get; set; } = string.Empty;
		public string json_p { get; set; } = string.Empty;
	}
}
