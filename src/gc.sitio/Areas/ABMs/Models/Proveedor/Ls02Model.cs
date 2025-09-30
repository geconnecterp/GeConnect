using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class Ls02Model
	{
		public string Tipo { get; set; }
		public SelectList LstTipoOpe { get; set; }
	}
}
