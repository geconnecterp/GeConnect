using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarObligacionesOCreditosPaso2Model
	{
		public bool EsPagoAnticipado { get; set; } = false;
		public string CuentaObs { get; set; } = string.Empty;
		public bool TieneMediosDePagos { get; set; } = false;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
		public GridCoreSmart<RetencionesDesdeObligYCredDto> GrillaRetenciones { get; set; }
		public GridCoreSmart<ValoresDesdeObligYCredDto> GrillaValores { get; set; }
		public GridCoreSmart<CuentaFPDto> GrillaMedioDePago { get; set; }
	}

	//Clase temporal hasta definir que sorcho llevar la grilla de medios de pago
	public class MedioDePago
	{
		public string medioDePagoNombre { get; set; } = string.Empty;
	}
}
