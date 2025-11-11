
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.Tr
{
	public class TRConfirmaRequest : RequestBase
	{
		public string json { get; set; } = string.Empty;
		public string admId { get; set; } = string.Empty;
		public string usuId { get; set; } = string.Empty;
	}
}
