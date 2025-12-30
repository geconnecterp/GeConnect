
namespace gc.infraestructura.Dtos
{
	public class InvRepoValPorSecDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public char invt_id { get; set; }
		public string invt_desc { get; set; } = string.Empty;
		public string inve_id { get; set; } = string.Empty;
		public string inve_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public decimal stk_cant { get; set; } = 0.000M;
		public decimal stk_val { get; set; } = 0.000M;
		public decimal plani_cant { get; set; } = 0.000M;
		public decimal plani_val { get; set; } = 0.000M;
		public int prod_sec { get; set; }
		public int prod_sec_cont { get; set; }
	}
}
