using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Consultas.Models
{
	public class FiltroMovYCarteraDeCuentaFinanModel
	{
		public DateTime Desde { get; set; }
		public DateTime Hasta { get; set; }
		public SelectList ListaTipoCuenta { get; set; }
		public string TipoCuentaSeleccionada { get; set; }
		public SelectList ListaCuentaFinanciera { get; set; }
		public string CuentaFinancieraSeleccionada { get; set; }
	}
}
