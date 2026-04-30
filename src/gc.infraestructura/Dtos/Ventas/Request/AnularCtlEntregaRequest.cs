using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Ventas
{
	public class AnularCtlEntregaRequest : RequestBase
	{
		public string ent_compte { get; set; } = string.Empty;
	}
}
