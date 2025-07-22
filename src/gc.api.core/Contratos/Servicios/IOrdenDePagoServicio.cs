using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
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
		List<OPUserDto> CargarOPUsuarios(string f_desde, string f_hasta);
		List<OrdenDePagoConsultaDto> CargarOrdenDePagoConsultaLista(BuscarOrdenesDePagoRequest request);
		List<OrdenDePagoConsultaDto> CargarOrdenDePagoConsultaListaReporte(BuscarOrdenesDePagoRequest request);
		List<RespuestaDto> AnularOrdenDePago(AnularOrdenDePagoRequest request);
		List<RespuestaDto> AnularCertificadoDeOrdenDePago(AnularCertificadoDeOrdenDePagoRequest request);
	}
}
