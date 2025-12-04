using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ListaRubroValorizadoModel
	{
		public SelectList ListaRubros { get; set; }
		public string RubroSeleccionado { get; set; } = string.Empty;
	}
}
