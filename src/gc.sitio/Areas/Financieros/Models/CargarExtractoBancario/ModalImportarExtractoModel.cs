using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ModalImportarExtractoModel
	{
		public SelectList OrigenDeDatos { get; set; }
		public string selectedValue { get; set; } = string.Empty;
	}
}
