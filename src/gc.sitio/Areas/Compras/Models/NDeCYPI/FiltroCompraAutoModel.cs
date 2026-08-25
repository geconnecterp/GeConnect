using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class FiltroCompraAutoModel
	{
		public bool EsOC { get; set; }
		public SelectList ListaSucursales { get; set; }
		public SelectList ListaDepositos { get; set; }
		public string SelectedValue { get; set; } = string.Empty;
		public int DiasAprov { get; set; }
		public DateTime VentaDiariaDesde { get; set; }
		public DateTime VentaDiariaHasta { get; set; }
		public bool LimitarPedidoACompletar { get; set; }
		public bool LimitarPedidoParaCumplir { get; set; }
		public bool TomarUltimoPedido { get; set; }
		public bool PedidoConPisoPalletCompleto { get; set; }
		public bool ExcluirOCPendientes { get; set; }
		public bool ExcluirPIPendientes { get; set; }
		public bool MostrarExcluirOCPendientes { get; set; } = false;
		public string Titulo { get; set; } = string.Empty;
	}
}
