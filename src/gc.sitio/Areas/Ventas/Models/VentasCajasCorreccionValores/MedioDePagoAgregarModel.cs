using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	public class MedioDePagoAgregarModel
	{
		public SelectList ListaMedioDePago { get; set; }
		public string MedioDePagoSeleccionado { get; set; }
	}
}
