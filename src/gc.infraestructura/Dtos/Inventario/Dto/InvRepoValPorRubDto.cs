
namespace gc.infraestructura.Dtos
{
	public class InvRepoValPorRubDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public char invt_id { get; set; }
		public string invt_desc { get; set; } = string.Empty;
		public string inve_id { get; set; } = string.Empty;
		public string inve_desc { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public decimal stk_cant { get; set; } = 0.000M;
		public decimal stk_val { get; set; } = 0.000M;
		public decimal plani_cant { get; set; } = 0.000M;
		public decimal plani_val { get; set; } = 0.000M;
		public int prod_rub { get; set; }
		public int prod_rub_cont { get; set; }
	}
}
