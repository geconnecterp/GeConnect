using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;

namespace gc.api.core.Contratos.Servicios
{
	public interface IOrdenDePagoServicio : IServicio<OrdenDePago>
	{
		List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id);
		List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId);
		List<RespuestaRelaDto> CargarSacarOPDebitoCreditoDelProveedor(CargarOSacarObligacionesOCreditosRequest r);
	}
}
