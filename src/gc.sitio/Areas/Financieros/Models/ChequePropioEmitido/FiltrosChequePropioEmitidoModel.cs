using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class FiltrosChequePropioEmitidoModel
	{
		public string selectedValue { get; set; } = string.Empty;
		public DateTime Date1 { get; set; }
		public DateTime Date2 { get; set; }
		public SelectList ListaCuentaBanco { get; set; }
		public SelectList ListaUsuarios { get; set; }
		public SelectList ListaEstados { get; set; }

	}
}
