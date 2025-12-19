using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ListaUsuarioModel
	{
		public SelectList ListaUsuarios { get; set; }
		public string UsuarioSeleccionado { get; set; } = string.Empty;
	}
}
