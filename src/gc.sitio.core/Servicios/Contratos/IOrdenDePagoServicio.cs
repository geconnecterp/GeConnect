using gc.infraestructura.Dtos.Consultas;
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
		List<RetencionesDesdeObligYCredDto> CargarRetencionesDesdeObligYCredSeleccionados(CargarRetencionesDesdeObligYCredSeleccionadosRequest r, string token);
		List<ValoresDesdeObligYCredDto> CargarValoresDesdeObligYCredSeleccionados(CargarValoresDesdeObligYCredSeleccionadosRequest r, string token);
		RespuestaGenerica<RespuestaDto> ConfirmarOrdenDePagoAProveedor(ConfirmarOPaProveedorRequest r, string token);
		List<ConsOrdPagoDetExtendDto> ConsultaOrdPagoDetExtend(string op_compte, string token);
		List<OPMotivoCtagDto> CargarOPMotivosCtag(string opt_id, string token);
	}
}
