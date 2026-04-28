using gc.infraestructura.Dtos.Ventas;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//BA -> Transferencia Bancaria / Cuenta Bancaria
	public class MedioDePagoBAModel : IMedioDePago
	{
		public SelectList ListaMediosDePago { get; set; }
		public string MedioDePagoSeleccionado { get; set; } = string.Empty;
		public string Banco { get; set; } = string.Empty;
		public string NroCuenta { get; set; } = string.Empty;
		public string NroDeposito { get; set; } = string.Empty;
		public decimal Importe { get; set; }
		public VtasPVCtlRendDetalleDto Item { get; set; }
	}
}
