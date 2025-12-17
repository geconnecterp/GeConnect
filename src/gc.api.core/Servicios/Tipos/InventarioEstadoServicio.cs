using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class InventarioEstadoServicio : Servicio<InventarioEstado>, IInventarioEstadoServicio
	{
		public InventarioEstadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<InventarioEstadoDto> GetInventarioEstadoLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_ESTADOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new InventarioEstadoDto()
				{
					#region Campos
					inve_id = x.inve_id,
					inve_desc = x.inve_desc
					#endregion
				}).ToList();
		}
	}
}
