
namespace gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante.Request
{
	public class ConfirmarAnulacionRequest
	{
		public string ctaId { get; set; } = string.Empty;
		public string diaMovi { get; set; } = string.Empty;
		public string tcoId { get; set; } = string.Empty;
		public string cmCompte { get; set; } = string.Empty;
		public string opcion { get; set; } = string.Empty;
		public string admId { get; set; } = string.Empty;
		public string usuId { get; set; } = string.Empty;
	}
}
