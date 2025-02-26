using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.CuentaComercial;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class CuentaGastoServicio : Servicio<CuentaGasto>, ICuentaGastoServicio
	{
		public CuentaGastoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<CuentaGastoDto> GetCuentaGastoParaABM(string ctag_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_GASTOS_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@ctag_id", ctag_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<CuentaGastoDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
