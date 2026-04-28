using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//EF -> Efectivo
	public class MedioDePagoEFModel : IMedioDePago
	{
		public VtasPVCtlRendDetalleDto Item { get; set; }
	}
}
