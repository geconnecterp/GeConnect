using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Financieros.Models
{
	public class ConsultaMovFinanYAnulaModel
	{
		public string selectedValue { get; set; } = string.Empty;
		public DateTime Date1 { get; set; }
		public DateTime Date2 { get; set; }
		public SelectList ListaCFO { get; set; }
		public SelectList ListaCFD { get; set; }
		public SelectList ListaTT { get; set; }
		public SelectList ListaUsu { get; set; }
	}
}
