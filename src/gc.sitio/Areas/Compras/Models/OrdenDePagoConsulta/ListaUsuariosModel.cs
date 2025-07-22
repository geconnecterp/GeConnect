using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta
{
	public class ListaUsuariosModel
	{
		public SelectList ListaUsuarios { get; set; }
		public string id { get; set; }
	}
}
