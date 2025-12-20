using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoInventarioServicio : Servicio<TipoInventario>, ITipoInventarioServicio
	{
		public TipoInventarioServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoInventarioDto> GetTiposEnventario()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_INV_TIPOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoInventarioDto()
				{
					#region Campos
					invt_id = x.invt_id,
					invt_desc = x.invt_desc,
					#endregion
				}).ToList();
		}
	}
}
