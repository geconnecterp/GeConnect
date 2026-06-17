
namespace gc.infraestructura.Dtos
{
	public class RepoVtaCtaCteDto : Dto
	{
		public string caja_nro_proceso { get; set; } = string.Empty;
		public int caja_nro_cierre { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public decimal co_ctacte { get; set; }
		public string cta_id { get; set; } = string.Empty;
		public string cta_denominacion { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string doc_compte { get; set; } = string.Empty;

	}
}
