using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaRubroModel
	{
		public SelectList ListaRubros { get; set; }
		public string selectedValue { get; set; } = string.Empty;
	}
}
