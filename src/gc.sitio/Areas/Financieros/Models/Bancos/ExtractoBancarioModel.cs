using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ExtractoBancarioModel
	{
		public DateTime FechaDesde { get; set; } = DateTime.Today.AddMonths(-1);
		public DateTime FechaHasta { get; set; } = DateTime.Today;
		public GridCoreSmart<FinancieroBcoExtractoDto> GrillaExtracto { get; set; }
	}
}
