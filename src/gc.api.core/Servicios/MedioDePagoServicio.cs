using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class MedioDePagoServicio : Servicio<Instrumento>, IMedioDePagoServicio
	{
		public MedioDePagoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<MedioDePagoABMDto> GetMedioDePagoParaABM(string ins_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_MEDIOS_PAGOS_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@ins_id", ins_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<MedioDePagoABMDto>(sp, ps, true);
			return listaTemp;
		}

		public List<OpcionCuotaDto> GetOpcionDeCuotaParaABM(string ins_id, int cuota)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_OPCION_CUOTA_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@ins_id", ins_id),
					new("@cuota", cuota)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OpcionCuotaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<OpcionCuotaDto> GetOpcionesDeCuotaParaABM(string ins_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_OPCION_CUOTA_LISTA;
			var ps = new List<SqlParameter>()
			{
					new("@ins_id", ins_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OpcionCuotaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroListaDto> GetCuentaFinYContableListaParaABM(string ins_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CUENTA_FIN_LISTA;
			var ps = new List<SqlParameter>()
			{
					new("@ins_id", ins_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroListaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroListaDto> GetCuentaFinYContableParaABM(string ctaf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_CUENTA_FIN_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@ctaf_id", ctaf_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroListaDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
