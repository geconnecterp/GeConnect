using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class BoxListDto : Dto
	{
		public SelectList ComboBoxes { get; set; } = new SelectList(new List<SelectListItem>());
	}
}
