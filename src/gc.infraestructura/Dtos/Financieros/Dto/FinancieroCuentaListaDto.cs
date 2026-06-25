
namespace gc.infraestructura.Dtos
{
	public class FinancieroCuentaListaDto : Dto
	{
		public string ctaf_id { get; set; } = string.Empty;
		public string ctaf_denominacion { get; set; } = string.Empty;
		public string ctaf_lista { get; set; } = string.Empty;
		public char ctaf_activo { get; set; }
		public string ctaf_estado { get; set; } = string.Empty;
		public string ctaf_estado_des { get; set; } = string.Empty;
		public decimal? ctaf_saldo { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string tcf_id { get; set; } = string.Empty;
		public string tcf_desc { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
		public string ins_desc { get; set; } = string.Empty;
		public string ccb_id { get; set; } = string.Empty;
		public string ccb_id_diferido { get; set; } = string.Empty;
		public string ctag_id { get; set; } = string.Empty;
		public string mon_codigo { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string ccb_desc { get; set; } = string.Empty;
		public string ccb_desc_diferido { get; set; } = string.Empty;
		public bool cartera { get; set; }
	}
}
