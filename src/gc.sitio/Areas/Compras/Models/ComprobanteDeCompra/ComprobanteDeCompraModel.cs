using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models
{
	public class ComprobanteDeCompraModel
	{
		public ComprobanteDeCompraDto Comprobante { get; set; }
		public SelectList TipoOpe { get; set; }
		public SelectList CondAfip { get; set; }
		public SelectList TipoCompte { get; set; }
		public SelectList Moneda { get; set; }
		public SelectList CtaDirecta { get; set; }
		public SelectList Cuotas { get; set; }
		public SelectList Opciones { get; set; }
	}
}
