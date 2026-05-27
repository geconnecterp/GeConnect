using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoTRServicio : Servicio<TRTipo>, ITipoTRServicio
	{
		public TipoTRServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TRTipoDto> GetTiposTR()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TR_TIPOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TRTipoDto()
				{
					#region Campos
					tit_id = x.tit_id,
					tit_desc = x.tit_desc,
					#endregion
				}).ToList();
		}
	}
}
