using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;

namespace gc.sitio.Areas.Compras.Models.OrdenDePagoAProveedor
{
	public class CargarNuevasObligacionesModel
	{
		public string MsgErrorEnCargarOSacarObligaciones { get; set; } = string.Empty;
		public GridCoreSmart<OPDebitoYCreditoDelProveedorDto> GrillaObligacionesNuevas { get; set; }
		public bool RecargarGrillaDeCreditos { get; set; } = false; //Cuando actualizo la grilla de debitos, si la respuesta trae algo en el Json "rela" lo inserto
	}
}
