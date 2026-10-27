using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Rpr
{
	public class CambiarTcoEnRprRequest : RequestBase
	{
		public string rp_compte { get; set; } = string.Empty;
	}
}
