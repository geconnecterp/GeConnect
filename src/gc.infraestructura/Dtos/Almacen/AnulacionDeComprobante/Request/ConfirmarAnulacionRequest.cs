
using gc.infraestructura.EntidadesComunes;

namespace gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante.Request
{
	public class ConfirmarAnulacionRequest : RequestBase
	{
		public string ctaId { get; set; } = string.Empty;
		public string diaMovi { get; set; } = string.Empty;
		public string tcoId { get; set; } = string.Empty;
		public string cmCompte { get; set; } = string.Empty;
		public string opcion { get; set; } = string.Empty;
	}
}
