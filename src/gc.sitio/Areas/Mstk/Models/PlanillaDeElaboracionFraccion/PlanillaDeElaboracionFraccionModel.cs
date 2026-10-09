using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models.PlanillaDeElaboracionFraccion
{
	public class PlanillaDeElaboracionFraccionModel
	{
		public SelectList ListaDepositos { get; set; } = new SelectList(new List<SelectListItem>());
		public SelectList ListaBoxes { get; set; } = new SelectList(new List<SelectListItem>());
		public string DepositoSeleccionado { get; set; } = string.Empty;
		public string BoxSeleccionado { get; set; } = string.Empty;
		public string Nota { get; set; } = string.Empty;
		public GridCoreSmart<ProductoModel> TablaProductosElaborados { get; set; }
		public GridCoreSmart<ProductoModel> TablaProductosMateriaPrima { get; set; }
	}
}
