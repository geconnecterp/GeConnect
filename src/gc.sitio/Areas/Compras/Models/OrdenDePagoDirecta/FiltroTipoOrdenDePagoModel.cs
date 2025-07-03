using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta
{
	public class FiltroTipoOrdenDePagoModel
	{
		public SelectList listaTiposOrdenDePago { get; set; }
		public string optIdSelected { get; set; } = string.Empty;
	}
}
