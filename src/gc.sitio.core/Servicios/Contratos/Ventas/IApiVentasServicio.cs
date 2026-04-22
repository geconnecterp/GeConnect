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
	}
}
