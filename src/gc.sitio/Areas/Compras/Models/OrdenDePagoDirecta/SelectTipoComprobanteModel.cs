using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class SelectTipoComprobanteModel
	{
		public SelectList listaTiposComptes { get; set; }
		public string tco_id { get; set; } = string.Empty;
	}
}
