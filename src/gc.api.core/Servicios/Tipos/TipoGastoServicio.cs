using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoGastoServicio : Servicio<TipoGasto>, ITipoGastoServicio
	{
		public TipoGastoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoGastoDto> GetTipoGastos()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_GASTO;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoGastoDto()
				{
					#region Campos
					ctag_denominacion = x.ctag_denominacion,
					ctag_gasto_ingreso = x.ctag_gasto_ingreso,
					ctag_id = x.ctag_id,
					ctag_lista = x.ctag_lista,
					ctag_tipo = x.ctag_tipo,
					#endregion
				}).ToList();
		}
	}
}
