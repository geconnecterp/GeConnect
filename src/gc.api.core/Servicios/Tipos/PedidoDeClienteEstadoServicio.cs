using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class PedidoDeClienteEstadoServicio : Servicio<PedidoDeClienteEstado>, IPedidoDeClienteEstadoServicio
	{
		public PedidoDeClienteEstadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<PedidoDeClienteEstadoDto> GetPedidoDeClienteEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_PC_ESTADOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new PedidoDeClienteEstadoDto()
				{
					#region Campos
					pce_id = x.pce_id,
					pce_desc = x.pce_desc
					#endregion
				}).ToList();
		}
	}
}
