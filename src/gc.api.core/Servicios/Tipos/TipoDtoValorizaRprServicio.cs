using gc.api.core.Contratos.Servicios.Tipos;
using gc.api.core.Entidades.Tipos;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios.Tipos
{
	public class TipoDtoValorizaRprServicio : Servicio<RecepcionesProvConceptos>, ITipoDtoValorizaRprServicio
	{
		public TipoDtoValorizaRprServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoDtoValorizaRprDto> GetTipoDtoValorizaRpr()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_DTOS_VALORIZA_RPR_LISTA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return [.. res.Select(x => new TipoDtoValorizaRprDto()
				{
					#region Campos
					dtoc_desc = x.dtoc_desc,
					dtoc_id = x.dtoc_id,
					dtoc_lista = x.dtoc_lista,
					tco_id = x.tco_id,
					#endregion
				})];
		}
	}
}
