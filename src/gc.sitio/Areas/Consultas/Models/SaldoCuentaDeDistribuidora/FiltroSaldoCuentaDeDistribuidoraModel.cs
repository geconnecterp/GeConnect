using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class FiltroSaldoCuentaDeDistribuidoraModel
	{
		public SelectList ListaVendedores{ get; set; }
		public string VendedorSeleccionado { get; set; } = string.Empty;
	}
}
