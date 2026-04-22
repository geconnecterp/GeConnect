using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//TD -> Tarjeta de Debito
	public class MedioDePagoTDModel : IMedioDePago
	{
		public SelectList ListaMediosDePago { get; set; }
	}
}
