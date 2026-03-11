using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Productos.OrdenDeReparto
{
	public class APonerEnCursoOrdenDeRepartoRequest : RequestBase
	{
		public string or_compte { get; set; } = string.Empty;
		public string json { get; set; } = string.Empty;
	}
}
