using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class TipoCuentaFinServicio : Servicio<TipoCuentaFin>, ITipoCuentaFinServicio
	{
		public TipoCuentaFinServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoCuentaFinDto> GetTiposCuentaFin()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_CUENTA_FIN_LISTA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoCuentaFinDto()
				{
					#region Campos
					tcf_desc = x.tcf_desc,
					tcf_id = x.tcf_id,
					tcf_lista = x.tcf_lista,
					#endregion
				}).ToList();
		}
	}
}
