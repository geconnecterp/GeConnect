using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ModalImportarArchivoModel
	{
		public SelectList OrigenDeDatos { get; set; }
		public string selectedValue { get; set; } = string.Empty;
	}
}
