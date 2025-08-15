using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class SeleccionValoresAPresentarModel
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_desc { get; set; } = string.Empty;
		public string titulo_col_1 { get; set; } = string.Empty;
		public string titulo_col_2 { get; set; } = string.Empty;
		public string titulo_col_3 { get; set; } = string.Empty;
		public GridCoreSmart<FinancieroCarteraDto> GrillaValoresAPresentar { get; set; }
		public decimal total { get; set; } = 0.00M;
	}
}
