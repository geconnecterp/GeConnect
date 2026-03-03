using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Distribuidora.Models.PedidoDeCliente
{
	public class FiltroModel
	{
		public SelectList ListaEstados { get; set; }
		public string EstadoSeleccionado { get; set; } = string.Empty;
		public SelectList ListaVendedores { get; set; }
		public string VendedorSeleccionado { get; set; } = string.Empty;
		public SelectList ListaRepartidores { get; set; }
		public string RepartidorSeleccionado { get; set; } = string.Empty;
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
	}
}
