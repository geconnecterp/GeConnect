using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class SectorServicio : Servicio<Sector>, ISectorServicio
	{
		public SectorServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{

		}

		public List<RubroListaABMDto> GetRubro(string rub_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_RUBRO_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@rub_id", rub_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<RubroListaABMDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RubroListaABMDto> GetRubroParaABM(string sec_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_RUBRO_LISTA;
			var ps = new List<SqlParameter>()
			{
					new("@sec_id", sec_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<RubroListaABMDto>(sp, ps, true);
			return listaTemp;
		}

		public List<SectorDto> GetSectorParaABM(string sec_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_SECTOR_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@sec_id", sec_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<SectorDto>(sp, ps, true);
			return listaTemp;
		}

		public List<SubSectorDto> GetSubSector(string rubg_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_SUB_SECTOR_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@rubg_id", rubg_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<SubSectorDto>(sp, ps, true);
			return listaTemp;
		}

		public List<SubSectorDto> GetSubSectorParaABM(string sec_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_SUB_SECTOR_LISTA;
			var ps = new List<SqlParameter>()
			{
					new("@sec_id", sec_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<SubSectorDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
