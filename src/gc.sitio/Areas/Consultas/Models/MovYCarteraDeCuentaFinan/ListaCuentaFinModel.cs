using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class ListaCuentaFinModel
	{
		public SelectList ListaCuentaFinanciera { get; set; }
		public string CuentaFinancieraSeleccionada { get; set; }
	}
}
