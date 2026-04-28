using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores
{
	//NI -> No Identificado
	public class MedioDePagoNIModel : IMedioDePago
	{
		public VtasPVCtlRendDetalleDto Item { get; set; }
	}
}
