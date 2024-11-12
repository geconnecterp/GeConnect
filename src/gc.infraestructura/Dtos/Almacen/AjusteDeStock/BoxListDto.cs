using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.infraestructura.Dtos.Almacen.AjusteDeStock
{
	public class BoxListDto : Dto
	{
		public SelectList ComboBoxes { get; set; }
	}
}
