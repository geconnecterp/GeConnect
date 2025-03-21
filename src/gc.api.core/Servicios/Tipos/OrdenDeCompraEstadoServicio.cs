using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
    public class OrdenDeCompraEstadoServicio : Servicio<OrdenDeCompraEstado>, IOrdenDeCompraEstadoServicio
	{
		public OrdenDeCompraEstadoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<OrdenDeCompraEstadoDto> GetOrdenDeCompraEstadoLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OC_ESTADO_LISTA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new OrdenDeCompraEstadoDto()
				{
					#region Campos
					oce_id = x.oce_id,
					oce_desc = x.oce_desc,
					oce_lista = x.oce_lista,
					#endregion
				}).ToList();
		}
	}
}
