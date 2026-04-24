using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//BA -> Transferencia Bancaria / Cuenta Bancaria
	public class MedioDePagoBAModel : IMedioDePago
	{
		public SelectList ListaMediosDePago { get; set; }
		public string MedioDePagoSeleccionado { get; set; } = string.Empty;
		public string Titular { get; set; } = string.Empty;
		public string NroOrden { get; set; } = string.Empty;
		public string Cuit { get; set; } = string.Empty;
		public decimal Importe { get; set; }
	}
}
