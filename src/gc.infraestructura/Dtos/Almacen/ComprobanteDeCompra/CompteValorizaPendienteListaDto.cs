
namespace gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra
{
	public class CompteValorizaPendienteListaDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public char tco_iva_discriminado { get; set; }
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string cm_cai { get; set; } = string.Empty;
		public DateTime? cm_cai_vto { get; set; }
		public DateTime cm_fecha { get; set; }
		public decimal cm_gravado { get; set; } = 0.00M;
		public decimal cm_no_gravado { get; set; } = 0.00M;
		public decimal cm_exento { get; set; } = 0.00M;
		public decimal cm_ii { get; set; } = 0.00M;
		public decimal cm_otro_ng { get; set; } = 0.00M;
		public decimal cm_iva { get; set; } = 0.00M;
		public decimal cm_percepciones { get; set; } = 0.00M;
		public decimal cm_flete_importe { get; set; } = 0.00M;
		public decimal cm_dto_importe { get; set; } = 0.00M;
		public decimal cm_total { get; set; } = 0.00M;
		public string usu_id { get; set; } = string.Empty;
		public string rpce_id { get; set; } = string.Empty;
		public string rpce_desc { get; set; } = string.Empty;
		public string cm_compte_obs { get; set; } = string.Empty;
		public char controlador_fiscal { get; set; }
	}
}
