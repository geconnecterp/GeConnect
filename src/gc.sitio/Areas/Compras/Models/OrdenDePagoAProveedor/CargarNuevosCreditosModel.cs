using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarNuevosCreditosModel
	{
		public string MsgErrorEnCargarOSacarCreditos { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
	}
}
