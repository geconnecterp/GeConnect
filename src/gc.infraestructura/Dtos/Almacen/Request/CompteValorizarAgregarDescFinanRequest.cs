
namespace gc.infraestructura.Dtos.Almacen.Request
{
	public class CompteValorizarAgregarDescFinanRequest
	{
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public bool dto_fijo { get; set; }
		public bool dto_sobre_total { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public decimal dto { get; set; } = 0.00M;
		public decimal dto_importe { get; set; } = 0.00M;
		public char dtoc_id { get; set; }
		public string dtoc_desc { get; set; } = string.Empty;
		public int item { get; set; } = 0;
	}
}
