using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoTributoServicio : Servicio<TipoTributo>, ITipoTributoServicio
	{
		public TipoTributoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoTributoDto> GetTiposTributo()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPO_TRIBUTO;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoTributoDto()
				{
					#region Campos
					ins_id = x.ins_id,
					ins_desc = x.ins_desc,
					ins_tipo = x.ins_tipo,
					carga_aut_discriminado = x.carga_aut_discriminado,
					carga_aut_no_discriminado = x.carga_aut_no_discriminado,
					orden = x.orden
					#endregion
				}).ToList();
		}
	}
}
