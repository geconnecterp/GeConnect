namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class Confirmar_ConceptoModel
	{
		public string afip_id { get; set; } = string.Empty;
		public string cm_cuit { get; set; } = string.Empty;
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string concepto { get; set; } = string.Empty;
		public int cantidad { get; set; } = 1;
		public string iva_situacion { get; set; } = string.Empty;
		public decimal iva_alicuota { get; set; } = 0.00M;
		public decimal subtotal { get; set; } = 0.00M;
		public decimal iva { get; set; } = 0.00M;
		public decimal total { get; set; } = 0.00M;
	}
}
