using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoMovStkServicio : Servicio<TipoMovStk>, ITipoMovStkServicio
	{
		public TipoMovStkServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoMovStkDto> ObtenerTiposDeMovimientosDeStock()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_MOV_STK;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoMovStkDto()
				{
					#region Campos
					sm_tipo= x.sm_tipo,
					sm_desc= x.sm_desc
					#endregion
				}).ToList();
		}
	}
}
