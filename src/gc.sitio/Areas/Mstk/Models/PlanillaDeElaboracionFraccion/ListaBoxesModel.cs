using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models.PlanillaDeElaboracionFraccion
{
	public class ListaBoxesModel
	{
		public SelectList ListaBoxes { get; set; } = new SelectList(new List<SelectListItem>());
		public string BoxSeleccionado { get; set; } = string.Empty;
	}
}
