using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoAnticipoEmpleadoServicio : Servicio<TipoAnticipoEmpleado>, ITipoAnticipoEmpleadoServicio
	{
		public TipoAnticipoEmpleadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoAnticipoEmpleadoDto> GetTiposAnticipoEmpleado()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_ANTICIPOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoAnticipoEmpleadoDto()
				{
					#region Campos
					ant_id = x.ant_id,
					ant_desc = x.ant_desc,
					#endregion
				}).ToList();
		}
	}
}
