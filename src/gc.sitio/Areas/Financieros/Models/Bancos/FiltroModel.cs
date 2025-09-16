using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class FiltroModel
	{
		public SelectList CuentaBanco { get; set; }
		public string selectedValue { get; set; } = string.Empty;
	}
}
