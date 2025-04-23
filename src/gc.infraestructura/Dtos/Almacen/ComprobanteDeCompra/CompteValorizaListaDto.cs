
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaListaDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string? afip_id { get; set; }
		public string adm_id { get; set; } = string.Empty;
		public string? cta_id { get; set; }
		public string mon_codigo { get; set; } = string.Empty;
		public string ope_iva { get; set; } = string.Empty;
		public string? ccb_id { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string? tdoc_id { get; set; }
		public string cm_cuit { get; set; } = string.Empty;
		public string? cm_nombre { get; set; }
		public string? cm_domicilio { get; set; }
		public DateTime cm_fecha { get; set; }
		public decimal cm_gravado { get; set; } = 0.00M;
		public decimal cm_no_gravado { get; set; } = 0.00M;
		public decimal cm_exento { get; set; } = 0.00M;
		public decimal cm_otro_ng { get; set; } = 0.00M;
		public decimal cm_ii { get; set; } = 0.00M;
		public decimal cm_iva { get; set; } = 0.00M;
		public decimal cm_percepciones { get; set; } = 0.00M;
		public decimal cm_importe_mt { get; set; } = 0.00M;
		public decimal cm_total { get; set; } = 0.00M;
		public string? cm_compte_obs { get; set; }
		public decimal? cm_percep_imp_nacionales { get; set; }
		public decimal? cm_percep_imp_municipales { get; set; }
		public string? cm_ib { get; set; }
		public bool seleccionable { get; set; }
		public bool aplica_valorizacion { get; set; }
		public int resultado { get; set; }
		public string resultado_msj { get; set; } = string.Empty;
	}
}