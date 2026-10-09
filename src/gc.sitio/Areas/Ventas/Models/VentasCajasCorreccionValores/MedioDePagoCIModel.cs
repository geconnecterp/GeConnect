using gc.infraestructura.Dtos.Ventas;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//CI -> Crédito Impositivo
	public class MedioDePagoCIModel : IMedioDePago
	{
		public string Certificado { get; set; } = string.Empty;
		public string CUIT { get; set; } = string.Empty;
		public string RazonSocial { get; set; } = string.Empty;
		public DateTime FechaEmision { get; set; }
		public decimal Importe { get; set; }
		public VtasPVCtlRendDetalleDto Item { get; set; }
	}
}
