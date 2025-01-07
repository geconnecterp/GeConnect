using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoRetGanServicio : Servicio<TipoRetGanancia>, ITipoRetGanServicio
	{
		public TipoRetGanServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoRetGananciaDto> GetTiposRetGanancia()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_RET_GAN;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoRetGananciaDto()
				{
					#region Campos
					imp_acu_mensual = x.imp_acu_mensual,
					rgan_desc = x.rgan_desc,
					rgan_id = x.rgan_id,
					rgan_imp_no_ret = x.rgan_imp_no_ret,
					rgan_lista = x.rgan_lista,
					#endregion
				}).ToList();
		}
	}
}
