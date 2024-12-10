
namespace gc.api.core.Entidades
{
	public partial class TipoComprobante : EntidadBase
	{
		public TipoComprobante()
		{
			Tco_id = string.Empty;
			Tco_desc = string.Empty;
			Tco_letra = string.Empty;
			Tco_tipo = string.Empty;
			Tco_iva_discriminado = string.Empty;
			Tco_iva_compra = string.Empty;
			Tco_iva_venta = string.Empty;
			Tco_sin_nro = string.Empty;
			Tco_grupo = string.Empty;
			Tco_desc_libro = string.Empty;
			Tco_id_afip = string.Empty;
		}
		public string Tco_id { get; set; }
		public string Tco_desc { get; set; }
		public string Tco_letra { get; set; }
		public string Tco_tipo { get; set; }
		public string Tco_iva_discriminado { get; set; }
		public string Tco_iva_compra { get; set; }
		public string Tco_iva_venta { get; set; }
		public string Tco_sin_nro { get; set; }
		public string Tco_grupo { get; set; }
		public string Tco_desc_libro { get; set; }
		public string Tco_id_afip { get; set; }
	}
}
