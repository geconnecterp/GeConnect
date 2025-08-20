using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class PasoUnoModel
	{
		public SelectList ListaCuentaValoresEnCartera { get; set; }
		public string ctafIdSelected { get; set; } = string.Empty;
	}
}
