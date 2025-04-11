using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaOcPendienteModel
	{
		public string oc_compte { get; set; }
		public SelectList LstOcPendiente { get; set; }
	}
}
