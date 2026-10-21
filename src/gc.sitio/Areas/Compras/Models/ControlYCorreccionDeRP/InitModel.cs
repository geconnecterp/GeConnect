using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class InitModel
	{
		public SelectList ListaRecepciones { get; set; }
		public string RecepcionSeleccionada { get; set; } = string.Empty;
	}
}
