using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class InitCargaRegExt
	{
		public SelectList ComboDepositos { get; set; } = new SelectList(new List<SelectListItem>());
		public SelectList ComboBoxes { get; set; } = new SelectList(new List<SelectListItem>());
		public SelectList TipoComprobantes { get; set; } = new SelectList(new List<SelectListItem>());
		public string TipoComprobanteSeleccionado { get; set; } = string.Empty;
		public string DepositoSeleccionado { get; set; } = string.Empty; 
		public string BoxSeleccionado { get; set; } = string.Empty;
	}
}
