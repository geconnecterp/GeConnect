
namespace gc.sitio.Areas.ControlComun.Models
{
	public class DetalleDeComptePerModel
	{
		public string tco_id { get; set; } = string.Empty;
		public string cm_compte { get; set; } = string.Empty;
		public string dia_movi { get; set; } = string.Empty;
		public string imp_id { get; set; } = string.Empty;
		public decimal @base { get; set; }
		public decimal ali { get; set; }
		public decimal percepcion { get; set; }
		public string imp_desc { get; set; } = string.Empty;
		public int orden { get; set; }
	}
}
