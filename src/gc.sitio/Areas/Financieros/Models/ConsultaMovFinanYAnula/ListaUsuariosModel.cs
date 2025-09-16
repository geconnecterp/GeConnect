using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ListaUsuariosModel
	{
		public string selectedValue { get; set; } = string.Empty;
		public SelectList ListaUsu { get; set; }
	}
}
