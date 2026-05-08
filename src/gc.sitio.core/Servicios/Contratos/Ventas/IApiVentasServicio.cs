using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IApiVentasServicio
	{
		Task<RespuestaGenerica<VtasPVCtlProcesoDto>> ObtenerVtasPVCtlProcesosLista(string adm_id, string token);
		Task<RespuestaGenerica<VtasPVCtlCierresDto>> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso, string token);
		Task<RespuestaGenerica<VtasPVCtlRendDto>> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre, string token);
		Task<RespuestaGenerica<VtasPVCtlRendDetalleDto>> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id, string token);
		Task<RespuestaGenerica<RespuestaDto>> CargaCtlNuevoItemDetalle(CargaCtlNuevoItemDetalleRequest request, string token);
		Task<RespuestaGenerica<RespuestaDto>> GuardarCtlDetalle(GuardarCtlDetalleRequest req, string token);
		Task<RespuestaGenerica<RespuestaDto>> ConfirmarCtlArqueo(ConfirmarCtlArqueoRequest req, string token);
		Task<RespuestaGenerica<RespuestaDto>> AnularCtlArqueo(AnularCtlArqueoRequest req, string token);
		Task<RespuestaGenerica<RespuestaDto>> AgregarMedioDePago(AgregarMedioDePagoRequest req, string token);
		Task<RespuestaGenerica<RespuestaDto>> ConfirmacionContable(ConfirmacionContableRequest req, string token);
		Task<RespuestaGenerica<VtasPVCtlEntregaDto>> ObtenerVtasPVCtlEntregaLista(string adm_id, string estado, string token);
		Task<RespuestaGenerica<VtasPVCtlEntregaRendDto>> ObtenerVtasPVCtlEntregaRendLista(string ent_compte, string token);
		Task<RespuestaGenerica<RespuestaDto>> ConfirmarCtlEntrega(ConfirmarCtlEntregaRequest req, string token);
		Task<RespuestaGenerica<RespuestaDto>> AnularCtlEntrega(AnularCtlEntregaRequest req, string token);
		List<AnaVtaMesDto> ObtenerAnaVtaMesLista(AnaVtaMesRequest request, string token);
		List<AnaVtaMesDetalleDiarioDto> ObtenerAnaVtaMesDetalleDiaLista(AnaVtaMesRequest request, string token);
		List<AnaVtaMesDetalleHoraDto> ObtenerAnaVtaMesDetalleHoraLista(AnaVtaMesRequest request, string token);
		List<AnaVtaMesDetalleSucursalDto> ObtenerAnaVtaMesDetalleSucursalLista(AnaVtaMesRequest request, string token);
		List<AnaVtaMesDetalleCierreDto> ObtenerAnaVtaMesDetalleCierreLista(AnaVtaMesRequest request, string token);
		List<AnaVtaMesDetalleAnualDto> ObtenerAnaVtaMesDetalleAnualLista(AnaVtaMesRequest request, string token);
	}
}
