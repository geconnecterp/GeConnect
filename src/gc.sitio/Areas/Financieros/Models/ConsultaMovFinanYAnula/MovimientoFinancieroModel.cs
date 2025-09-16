using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class MovimientoFinancieroModel
	{
		public GridCoreSmart<MovimientoFinancieroListaDto> GrillaMovimientoFinanciero { get; set; }
		public decimal Totales { get; set; }
	}
}
