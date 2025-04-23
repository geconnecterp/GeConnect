using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ListaComptePendienteDeValorizarModel
	{
		public string cm_compte { get; set; }
		public SelectList LstComptePendiente { get; set; }
	}
}
