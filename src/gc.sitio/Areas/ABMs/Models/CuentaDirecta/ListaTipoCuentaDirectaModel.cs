using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class ListaTipoCuentaDirectaModel
	{
		public SelectList ListaCuentaDirecta { get; set; }
		public string TipoSeleccionado { get; set; } = string.Empty;
	}
}
