using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoOpeIvaServicio : Servicio<OpeIva>, ITipoOpeIvaServicio
	{
		public TipoOpeIvaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoOpeIvaDto> GetTipoOpeIva()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_OPE_IVA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoOpeIvaDto()
				{
					#region Campos
					ope_iva = x.ope_iva,
					ope_iva_descripcion = x.ope_iva_descripcion,
					ope_lista = x.ope_lista,
					#endregion
				}).ToList();
		}
	}
}
