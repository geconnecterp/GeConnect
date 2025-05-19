using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarObligacionesOCreditosModel
	{
		public string ctaDir { get; set; }
		public SelectList listaCtaDir { get; set; }
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
	}
}
