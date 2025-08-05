using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante
{
	public class AgregarProductoModel
	{
		public string p_id { get; set; } = string.Empty;
		public string p_desc { get; set; } = string.Empty;
		public decimal p_cantidad { get; set; } = 0.000M;
		public bool incluye_rp { get; set; } = false;
		public string rp { get; set; } = string.Empty;
		public SelectList ComboRP { get; set; }
	}
}
