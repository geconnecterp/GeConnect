using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class TipoMonedaServicio : Servicio<Moneda>, ITipoMonedaServicio
	{
		public TipoMonedaServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<TipoMonedaDto> GetTiposMoneda()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_TIPOS_MONEDA_LISTA;
			var ps = new List<SqlParameter>();
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new TipoMonedaDto()
				{
					#region Campos
					Mon_Codigo=x.Mon_Codigo,
					Mon_Lista=x.Mon_Lista,
					Mon_Cotizacion = x.Mon_Cotizacion,
					Mon_Defecto=x.Mon_Defecto,
					Mon_Desc= x.Mon_Desc,
					Mon_Vigente=x.Mon_Vigente,
					#endregion
				}).ToList();
		}
	}
}
