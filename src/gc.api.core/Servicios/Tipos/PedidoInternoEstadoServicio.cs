using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Tipos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class PedidoInternoEstadoServicio : Servicio<PedidoInternoEstado>, IPedidoInternoEstadoServicio
	{
		public PedidoInternoEstadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<PedidoInternoEstadoDto> GetPedidoInternoEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PI_Estados;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new PedidoInternoEstadoDto()
				{
					#region Campos
					pie_id = x.pie_id,
					pie_desc = x.pie_desc,
					pie_lista = x.pie_lista
					#endregion
				}).ToList();
		}
	}
}
