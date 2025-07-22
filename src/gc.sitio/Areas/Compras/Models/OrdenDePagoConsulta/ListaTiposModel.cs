using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta
{
	public class ListaTiposModel
	{
		public SelectList ListaTipos { get; set; }
		public string id { get; set; }
	}
}
