using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Ventas;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiVentasServicio : IServicio<EntidadBase>
	{
		List<VtasPVCtlProcesoDto> ObtenerVtasPVCtlProcesosLista(string adm_id);
		List<VtasPVCtlCierresDto> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso);
		List<VtasPVCtlRendDto> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre);
		List<VtasPVCtlRendDetalleDto> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id);
	}
}
