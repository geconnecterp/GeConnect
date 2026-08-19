using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ListaBoxesModel
	{
		public SelectList ListaBoxs { get; set; }
		public string BoxSeleccionado { get; set; } = string.Empty;
	}
}
