using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Tipos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoOrdenDePagoServicio : Servicio<TipoOrdenDePago>, ITipoOrdenDePagoServicio
	{
		public TipoOrdenDePagoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<TipoOrdenDePagoDto> GetTiposDeOrdenDePago()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_ORDEN_PAGO;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(y => new TipoOrdenDePagoDto()
				{
					#region Campos
					opt_id = y.opt_id,
					opt_desc = y.opt_desc,
					opt_lista = y.opt_lista
					#endregion
				}).ToList();
		}
	}
}
