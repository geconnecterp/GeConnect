using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarObligacionesOCreditosModel
	{
		public string ctaDir { get; set; } = string.Empty;
		public SelectList listaCtaDir { get; set; }
		public string valoresANombreDe { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
	}
}
