using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models.Cliente
{
	public class Ls02Model
	{
		public string Tipo { get; set; }
		public SelectList LstTipo { get; set; }
	}
}
