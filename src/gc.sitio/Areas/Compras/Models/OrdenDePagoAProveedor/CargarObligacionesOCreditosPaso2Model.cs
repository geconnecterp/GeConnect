using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarObligacionesOCreditosPaso2Model
	{
		public bool EsPagoAnticipado { get; set; } = false;
		public string CuentaObs { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
		public GridCoreSmart<RetencionesDesdeObligYCredDto> GrillaRetenciones { get; set; }
		public GridCoreSmart<ValoresDesdeObligYCredDto> GrillaValores { get; set; }
		public GridCoreSmart<FormaDePagoDto> GrillaMedioDePago { get; set; }
	}

	//Clase temporal hasta definir que sorcho llevar la grilla de medios de pago
	public class MedioDePago
	{
		public string medioDePagoNombre { get; set; } = string.Empty;
	}
}
