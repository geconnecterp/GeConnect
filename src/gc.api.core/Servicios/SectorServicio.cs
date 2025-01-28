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
	}
}
