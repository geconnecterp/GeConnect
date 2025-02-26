using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
    public class TipoCuentaGastoServicio : Servicio<TipoCuentaGasto>, ITipoCuentaGastoServicio
	{
		public TipoCuentaGastoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoCuentaGastoDto> GetTiposCuentaGasto()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CUENTA_GASTO_LISTA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoCuentaGastoDto()
				{
					#region Campos
					tcg_desc = x.tcg_desc,
					tcg_id = x.tcg_id,
					tcg_lista = x.tcg_lista,
					#endregion
				}).ToList();
		}
	}
}
