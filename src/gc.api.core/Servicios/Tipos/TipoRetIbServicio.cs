using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoRetIbServicio : Servicio<TipoRetIngBr>, ITipoRetIbServicio
	{
		public TipoRetIbServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoRetIngBrDto> GetTiposRetIngBr()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_RET_IB;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoRetIngBrDto()
				{
					#region Campos
					rib_alic = x.rib_alic,
					rib_alic_lh = x.rib_alic_lh,
					rib_desc = x.rib_desc,
					rib_id = x.rib_id,
					rib_lista = x.rib_lista,
					rib_min_imponible = x.rib_min_imponible,
					rib_tipo_base = x.rib_tipo_base,
					#endregion
				}).ToList();
		}
	}
}
