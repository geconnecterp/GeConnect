using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarNuevosCreditosModel
	{
		public string MsgErrorEnCargarOSacarCreditos { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaCreditosNueva { get; set; }
		public bool RecargarGrillaDeDebitos { get; set; } = false; //Cuando actualizo la grilla de creditos, si la respuesta trae algo en el Json "rela" lo inserto
	}
}
