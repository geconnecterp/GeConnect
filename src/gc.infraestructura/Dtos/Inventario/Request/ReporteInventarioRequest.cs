using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Inventario.Request
{
	public class ReporteInventarioRequest : RequestBase
	{
		public string inv_nro { get; set; } = string.Empty;
	}
}
