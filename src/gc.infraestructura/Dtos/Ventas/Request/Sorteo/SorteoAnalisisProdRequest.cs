using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas.Request
{
	public class SorteoAnalisisProdRequest : RequestBase
	{
		public string so_sorteo { get; set; } = string.Empty;
	}
}
