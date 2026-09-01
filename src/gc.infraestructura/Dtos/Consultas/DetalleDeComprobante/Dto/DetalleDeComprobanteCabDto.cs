
namespace gc.infraestructura.Dtos
{
	public class DetalleDeComprobanteCabDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_letra { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string adm_id { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public string cm_libro_iva { get; set; } = string.Empty;
		public decimal cm_gravado { get; set; }
		public decimal cm_no_gravado { get; set; }
		public decimal cm_exento { get; set; }
		public decimal cm_otro_ng { get; set; }
		public decimal cm_ii { get; set; }
		public decimal cm_iva { get; set; }
		public decimal cm_percepciones { get; set; }
		public decimal cm_total { get; set; }
		public string mon_codigo { get; set; } = string.Empty;
		public string usu_id { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string afip_desc { get; set; } = string.Empty;
		public string cm_cae { get; set; } = string.Empty;
		public DateTime cm_cae_vto { get; set; }

	}
}
