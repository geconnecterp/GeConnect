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
		List<RetencionesDesdeObligYCredDto> CargarRetencionesDesdeObligYCredSeleccionados(CargarRetencionesDesdeObligYCredSeleccionadosRequest r);
		List<ValoresDesdeObligYCredDto> CargarValoresDesdeObligYCredSeleccionados(CargarValoresDesdeObligYCredSeleccionadosRequest r);
		List<RespuestaDto> ConfirmarOrdenDePagoAProveedor(ConfirmarOPaProveedorRequest request);
		List<OPMotivoCtagDto> CargarOPMotivosCtag(string opt_id);
		List<RespuestaDto> ConfirmarOrdenDePagoDirecta(ConfirmarOrdenDePagoDirectaRequest request);
	}
}
