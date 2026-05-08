using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiVentasServicio : IServicio<EntidadBase>
	{
		List<VtasPVCtlProcesoDto> ObtenerVtasPVCtlProcesosLista(string adm_id);
		List<VtasPVCtlCierresDto> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso);
		List<VtasPVCtlRendDto> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre);
		List<VtasPVCtlRendDetalleDto> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id);
		RespuestaDto CargaCtlNuevoItemDetalle(CargaCtlNuevoItemDetalleRequest request);
		RespuestaDto GuardarCtlDetalle(GuardarCtlDetalleRequest request);
		RespuestaDto ConfirmarCtlArqueo(ConfirmarCtlArqueoRequest request);
		RespuestaDto AnularCtlArqueo(AnularCtlArqueoRequest request);
		RespuestaDto AgregarMedioDePago(AgregarMedioDePagoRequest request);
		RespuestaDto ConfirmacionContable(ConfirmacionContableRequest request);
		List<VtasPVCtlEntregaDto> ObtenerVtasPVCtlEntregaLista(string adm_id, char estado);
		List<VtasPVCtlEntregaRendDto> ObtenerVtasPVCtlEntregaRendLista(string ent_compte);
		RespuestaDto ConfirmarCtlEntrega(ConfirmarCtlEntregaRequest request);
		RespuestaDto AnularCtlEntrega(AnularCtlEntregaRequest request);
		List<AnaVtaMesDto> ObtenerAnaVtaMesLista(AnaVtaMesRequest request);
		List<AnaVtaMesDetalleDiarioDto> ObtenerAnaVtaMesDetalleDiaLista(AnaVtaMesRequest request);
		List<AnaVtaMesDetalleHoraDto> ObtenerAnaVtaMesDetalleHoraLista(AnaVtaMesRequest request);
		List<AnaVtaMesDetalleSucursalDto> ObtenerAnaVtaMesDetalleSucursalLista(AnaVtaMesRequest request);
		List<AnaVtaMesDetalleAnualDto> ObtenerAnaVtaMesDetalleAnualLista(AnaVtaMesRequest request);
		List<AnaVtaMesDetalleCierreDto> ObtenerAnaVtaMesDetalleCierreLista(AnaVtaMesRequest request);
	}
}
