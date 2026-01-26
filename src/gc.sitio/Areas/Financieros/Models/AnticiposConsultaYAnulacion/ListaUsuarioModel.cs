using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Mstk.Models
{
	public class ListaUsuModel
	{
		public SelectList ListaUsuario { get; set; }
		public string UsuarioSeleccionado { get; set; } = string.Empty;
	}
}
