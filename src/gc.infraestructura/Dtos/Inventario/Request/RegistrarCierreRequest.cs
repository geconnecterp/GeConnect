using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Inventario.Request
{
	public class RegistrarCierreRequest : RequestBase
	{
		public string inv_nro { get; set; } = string.Empty;
		public string json_p { get; set; } = string.Empty;	
	}
}
 