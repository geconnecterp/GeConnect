using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoProveedorServicio : Servicio<TipoProveedor>, ITipoProveedorServicio
	{
		public TipoProveedorServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoProveedorDto> GetTiposProveedor()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_PROV;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoProveedorDto()
				{
					#region Campos
					tp_desc = x.tp_desc,
					tp_id = x.tp_id,
					tp_lista = x.tp_lista,
					#endregion
				}).ToList();
		}
	}
}
