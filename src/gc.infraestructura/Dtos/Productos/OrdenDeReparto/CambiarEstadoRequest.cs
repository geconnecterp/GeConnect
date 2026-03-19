using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class CambiarEstadoRequest : RequestBase
	{
		public string or_compte { get; set; } = string.Empty;
		public char ore_id { get; set; }
	}
}
