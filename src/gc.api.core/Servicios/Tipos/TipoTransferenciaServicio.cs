using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Tipos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoTransferenciaServicio : Servicio<TipoTransferencia>, ITipoTransferenciaServicio
	{
		public TipoTransferenciaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoTransferenciaDto> GetTipoTransferenciaLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_TRANSFERENCIA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoTransferenciaDto()
				{
					#region Campos
					ttra_id	= x.ttra_id,
					ttra_desc = x.ttra_desc,
					ttra_lista = x.ttra_lista,
					#endregion
				}).ToList();
		}
	}
}
