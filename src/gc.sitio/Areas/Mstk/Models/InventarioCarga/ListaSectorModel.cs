using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ListaSectorModel
	{
		public SelectList ListaSectores { get; set; }
		public string SectorSeleccionado { get; set; } = string.Empty;
	}
}
