
namespace gc.infraestructura.Dtos
{
	public class TipoComprobanteDto : Dto
	{
		public string tco_id { get; set; } = string.Empty;
		public string tco_desc { get; set; } = string.Empty;
		public string tco_letra { get; set; } = string.Empty;
		public string tco_tipo { get; set; } = string.Empty;
		public string tco_iva_discriminado { get; set; } = string.Empty;
		public string tco_iva_compra { get; set; } = string.Empty;
        public string tco_iva_venta { get; set; } = string.Empty;
        public string tco_sin_nro { get; set; } = string.Empty;
        public string tco_grupo { get; set; } = string.Empty;
        public string tco_desc_libro { get; set; } = string.Empty;
        public string tco_id_afip { get; set; } = string.Empty;
	}
}
