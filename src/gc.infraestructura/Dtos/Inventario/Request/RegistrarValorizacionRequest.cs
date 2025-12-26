using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Inventario.Request
{
	public class RegistrarValorizacionRequest : RequestBase
	{
		public string inv_nro { get; set; } = string.Empty;
	}
}
