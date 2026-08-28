using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.ControlComun.Models
{
	public class DetalleDeCompteModel
	{
		public DetalleDeCompteCabModel Cab { get; set; } = new();
		public GridCoreSmart<DetalleDeComprobanteIvaDto> ListIva { get; set; } = new();
		public GridCoreSmart<DetalleDeComprobantePerDto> ListaPer { get; set; } = new();

	}
}
