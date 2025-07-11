namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class Confirmar_EncabezadoModel
	{
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string cm_nombre { get; set; } = string.Empty;
		public string cm_domicilio { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public DateTime cm_fecha { get; set; }
		public string ctag_id { get; set; } = string.Empty;
		public string ctag_motivo { get; set; } = string.Empty;
		public string cm_libro_iva { get; set; } = string.Empty;
		public decimal cm_no_gravado { get; set; } = 0.00M;
		public decimal cm_exento { get; set; } = 0.00M;
		public decimal cm_gravado { get; set; } = 0.00M;
		public decimal cm_iva { get; set; } = 0.00M;
		public decimal cm_otros_ng { get; set; } = 0.00M;
		public decimal cm_ii { get; set; } = 0.00M;
		public decimal cm_percepciones { get; set; } = 0.00M;
		public decimal cm_total { get; set; } = 0.00M;
	}
}
