using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDeCompraConsulta
{
	public class ListaEstadosModel
	{
		public SelectList ListaEstados { get; set; }
		public string id { get; set; }
	}
}
