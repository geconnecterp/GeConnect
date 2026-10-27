
namespace gc.infraestructura.Dtos.Almacen
{
	public class ConsultaValorizacionesRegistradasDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string cta_id { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public decimal cm_gravado { get; set; }
		public decimal cm_no_gravado { get; set; }
		public decimal cm_exento { get; set; }
		public decimal cm_ii { get; set; }
		public decimal cm_otro_ng { get; set; }
		public decimal cm_iva { get; set; }
		public decimal cm_percepciones { get; set; }
		public decimal cm_total { get; set; }
		public string usu_id { get; set; } = string.Empty;
		public string rpce_id { get; set; } = string.Empty;
		public string rpce_desc { get; set; } = string.Empty;
		public string comprobante => $"{tco_desc} ({tco_id}) - {cm_compte}";
	}
}
