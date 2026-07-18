using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ListaRubroModel
	{
		public SelectList ListaRubros { get; set; }
		public string RubroSeleccionado { get; set; } = string.Empty;
	}
}
