using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ListaTipoMedioDePagoModel
	{
		public SelectList ListaTipo { get; set; }
		public string TipoSeleccionado { get; set; } = string.Empty;
	}
}
