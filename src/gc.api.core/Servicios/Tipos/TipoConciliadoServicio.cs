using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoConciliadoServicio : Servicio<TipoConciliado>, ITipoConciliadoServicio
	{
		public TipoConciliadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoConciliadoDto> GetTipoConciliadoLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CONCILIADO;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoConciliadoDto()
				{
					#region Campos
					ct_concilia = x.ct_concilia,
					ct_descripcion = x.ct_descripcion,
					ct_modo = x.ct_modo,
					ct_tipo = x.ct_tipo,
					extr_desc = x.extr_desc,
					extr_id = x.extr_id
					#endregion
				}).ToList();
		}
	}
}
