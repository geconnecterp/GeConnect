using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.ABMs.Models
{
	public class Ls03Model
	{
		public string Tipo { get; set; }
		public SelectList LstZona { get; set; }
	}
}
