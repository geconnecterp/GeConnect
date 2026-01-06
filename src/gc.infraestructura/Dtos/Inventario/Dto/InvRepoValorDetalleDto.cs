
namespace gc.infraestructura.Dtos
{
	public class InvRepoValorDetalleDto : Dto
	{
		public string inv_nro { get; set; } = string.Empty;
		public string p_id { get; set; } = string.Empty;
		public string p_des { get; set; } = string.Empty;
		public string box_desc { get; set; } = string.Empty;
		public string rub_id { get; set; } = string.Empty;
		public string rub_desc { get; set; } = string.Empty;
		public string rubg_id { get; set; } = string.Empty;
		public string rubg_desc { get; set; } = string.Empty;
		public string sec_id { get; set; } = string.Empty;
		public string sec_desc { get; set; } = string.Empty;
		public string up_id { get; set; } = string.Empty;
		public decimal ps_stk { get; set; } = 0.000M;
		public decimal p_costo { get; set; } = 0.000M;
		public decimal conteo1 { get; set; } = 0.000M;
		public decimal conteo2 { get; set; } = 0.000M;
		public decimal ps_conteo { get; set; } = 0.000M;
		public decimal ajuste { get; set; } = 0.000M;
		public char ps_ajuste { get; set; }
		public char invt_id { get; set; }
		public string invt_desc { get; set; } = string.Empty;
		public char inve_id { get; set; }
		public string inve_desc { get; set; } = string.Empty;
	}
}
