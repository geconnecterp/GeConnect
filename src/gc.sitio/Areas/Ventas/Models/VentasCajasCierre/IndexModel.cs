using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCierre
{
	public class IndexModel
	{
		public GridCoreSmart<CajaPVAbiertosDto> ListaCajasAbiertas { get; set; }
	}
}
