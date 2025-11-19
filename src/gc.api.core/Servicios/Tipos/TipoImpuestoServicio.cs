using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoImpuestoServicio : Servicio<TipoImpuesto>, ITipoImpuestoServicio
	{
		public TipoImpuestoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoImpuestoDto> GetTiposDeImpuestos()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_IMPUESTOS;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoImpuestoDto()
				{
					#region Campos
					agente_per = x.agente_per,
					agente_per_ccb = x.agente_per_ccb,
					agente_per_pag_ant = x.agente_per_pag_ant,
					agente_ret = x.agente_ret,
					agente_ret_ccb = x.agente_ret_ccb,
					agente_ret_pag_ant = x.agente_ret_pag_ant,
					cont = x.cont,
					cont_pago_ant = x.cont_pago_ant,
					cont_sufre_per = x.cont_sufre_per,
					cont_sufre_ret = x.cont_sufre_ret,
					cont_sufre_reta = x.cont_sufre_reta,
					cont_sufre_ret_bco = x.cont_sufre_ret_bco,
					imp_descripcion = x.imp_descripcion,
					imp_id = x.imp_id
					#endregion
				}).ToList();
		}
	}
}
