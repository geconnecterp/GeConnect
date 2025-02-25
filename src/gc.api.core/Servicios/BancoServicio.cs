using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class BancoServicio : Servicio<Banco>, IBancoServicio
	{
		public BancoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}

		public List<BancoDto> GetBancoParaABM(string ctaf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_BANCO_DATOS;
			var ps = new List<SqlParameter>()
			{
					new("@ctaf_id", ctaf_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<BancoDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
