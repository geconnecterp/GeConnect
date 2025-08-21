using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.Areas.Financieros.Models
{
	public class CargarChequesDeTercerosEnCarteraModel
	{
		public GridCoreSmart<FinancieroCarteraDto> GrillaChequesEnCartera { get; set; }
		public bool mostrar_fecha { get; set; } = false;
		public DateTime fecha_valor { get; set; }
		public string titulo_col_1 { get; set; } = string.Empty;
		public string titulo_col_2 { get; set; } = string.Empty;
		public string titulo_col_3 { get; set; } = string.Empty;
	}
}
