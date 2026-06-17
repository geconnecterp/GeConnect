
namespace gc.infraestructura.Dtos
{
	public class RepoVtaNDDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public decimal co_nota_debito_prov { get; set; }
		public string rb_compte { get; set; } = string.Empty;
		public string rb_compte_cobro { get; set; } = string.Empty;
		public string co_tipo { get; set; } = string.Empty;
	}
}
