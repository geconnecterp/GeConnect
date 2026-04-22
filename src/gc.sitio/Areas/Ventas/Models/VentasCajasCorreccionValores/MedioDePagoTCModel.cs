using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//TC -> Tarjeta de Credito
	public class MedioDePagoTCModel : IMedioDePago
	{
		public SelectList ListaMediosDePago { get; set; }
		public string MedioDePagoSeleccionado { get; set; } = string.Empty;
		public string NroTarjeta { get; set; } = string.Empty;
		public string Lote { get; set; } = string.Empty;
		public string Cupon { get; set; } = string.Empty;
		public decimal Importe { get; set; }
	}
}
