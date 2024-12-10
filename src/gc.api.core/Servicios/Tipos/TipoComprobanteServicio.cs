using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoComprobanteServicio :Servicio<TipoComprobante>, ITiposComprobanteServicio
    {
		public TipoComprobanteServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoComprobanteDto> GetTipoComprobanteListaPorCuenta(string cuenta)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RPR_TIPOS_COMPTES;
			var ps = new List<SqlParameter>()
			{
					new("@cta_id",cuenta)
			};
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoComprobanteDto()
				{
					#region Campos
					tco_desc=x.Tco_desc,
					tco_desc_libro=x.Tco_desc_libro,
					tco_grupo=x.Tco_grupo,	
					tco_id=x.Tco_id,
					tco_id_afip=x.Tco_id_afip,
					tco_iva_compra=x.Tco_iva_compra,
					tco_iva_discriminado=x.Tco_iva_discriminado,
					tco_iva_venta=x.Tco_iva_venta,
					tco_letra=x.Tco_letra,
					tco_sin_nro=x.Tco_sin_nro,
					tco_tipo=x.Tco_tipo,
					#endregion
				}).ToList();
		}
	}
}
