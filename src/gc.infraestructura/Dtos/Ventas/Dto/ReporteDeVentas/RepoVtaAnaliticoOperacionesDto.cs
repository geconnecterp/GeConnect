
namespace gc.infraestructura.Dtos
{
	public class RepoVtaAnaliticoOperacionesDto : Dto
	{
		public string adm_id { get; set; } = string.Empty;
		public string adm_nombre { get; set; } = string.Empty;
		public DateTime caja_apertura { get; set; }
		public DateTime? caja_cierre { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string usu_nombre { get; set; } = string.Empty;
		public int? caja_nro_ope { get; set; }
		public int? registro { get; set; }
		public string caja_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string nombre { get; set; } = string.Empty;
		public string co_tipo { get; set; } = string.Empty;
		public string co_tipo_desc { get; set; } = string.Empty;
		public DateTime? co_fecha { get; set; }
		public char? co_anulado { get; set; }
		public string tco_id { get; set; } = string.Empty;
		public char? tco_letra { get; set; }
		public string tco_tipo { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public decimal? importe { get; set; }
		public decimal? a_rendir { get; set; }
		public string ins_id { get; set; } = string.Empty;
		public string concepto { get; set; } = string.Empty;
		public decimal? valor { get; set; }
		public string cm_compte_hasta { get; set; } = string.Empty;
	}
}
