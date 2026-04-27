using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionCustodia
{
	public class ListaEntregasModel
	{
		public SelectList ListaEntregas { get; set; }
		public string EntregaSeleccionada { get; set; }
	}
}
