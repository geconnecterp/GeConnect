namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class OPDModel
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string nro_compte { get; set; } = string.Empty;
		public DateTime fecha_compte { get; set; }
		public string cuit { get; set; } = string.Empty;
		public string afip_id { get; set; } = string.Empty;
		public string afip_desc { get; set; } = string.Empty;
		public string razon_soc { get; set; } = string.Empty;
		public string cta_dir_id { get; set; } = string.Empty;
	}
}
