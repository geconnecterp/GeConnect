using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;

namespace gc.sitio.core.Servicios.Contratos
{
	public  interface IOrdenDePagoServicio : IServicio<OrdenDePagoDto>
	{
		List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id, string token);
		List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId, string token);
		RespuestaGenerica<RespuestaRelaDto> CargarSacarOPDebitoCreditoDelProveedor(CargarOSacarObligacionesOCreditosRequest r, string token);
	}
}
