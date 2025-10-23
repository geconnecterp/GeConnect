using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ProyFinanModel
	{
		public GridCoreSmart<ProyFinanDto> GrillaProyFinan { get; set; }
		public decimal SaldoBancarioDisponible { get; set; } = 0.00M;
		public decimal SaldoBancarioEnDescubierto { get; set; } = 0.00M;
		public decimal ValoresAlCobroNoAcreditados { get; set; } = 0.00M;
		public decimal DocumentosACobrarVencidos { get; set; } = 0.00M;
		public decimal ProyeccionDeVentasDiarias { get; set; } = 0.00M;

	}
}
