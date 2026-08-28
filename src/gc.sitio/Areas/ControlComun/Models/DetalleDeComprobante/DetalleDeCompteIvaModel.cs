namespace gc.sitio.Areas.ControlComun.Models
{
	public class DetalleDeCompteIvaModel
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public decimal alicuota_iva { get; set; }
		public string concepto { get; set; } = string.Empty;
		public decimal importe { get; set; }
		public decimal iva { get; set; }
		public int orden { get; set; }
	}
}
