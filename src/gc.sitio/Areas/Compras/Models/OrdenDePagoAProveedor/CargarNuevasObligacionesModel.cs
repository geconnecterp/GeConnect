using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarNuevasObligacionesModel
	{
		public string MsgErrorEnCargarOSacarObligaciones { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
	}
}
