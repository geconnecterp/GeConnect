using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class FiltroAnticipoConsYAnuModel
	{
		public DateTime Date1 { get; set; }
		public DateTime Date2 { get; set; }
		public string selectedValue { get; set; } = string.Empty;
		public SelectList ListaTipo { get; set; }
		public SelectList ListaUsuario { get; set; }
	}
}
