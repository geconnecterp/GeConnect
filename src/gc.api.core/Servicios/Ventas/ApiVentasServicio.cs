using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Ventas;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
	public class ApiVentasServicio : Servicio<EntidadBase>, IApiVentasServicio
	{
		public ApiVentasServicio(IUnitOfWork uow) : base(uow)
		{
		}

		public List<VtasPVCtlProcesoDto> ObtenerVtasPVCtlProcesosLista(string adm_id)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_PROCESOS;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@adm_id", adm_id),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlProcesoDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlCierresDto> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_CIERRES;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlCierresDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlRendDto> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_REND;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", caja_nro_cierre),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlRendDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlRendDetalleDto> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_REND_DETALLE;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", caja_nro_rend),
				new SqlParameter("@tcf_id", tcf_id),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlRendDetalleDto>(sp, ps, true);
			return result;
		}
	}
}
