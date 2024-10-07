using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
	public class RemitoServicio : Servicio<Remito>, IRemitoServicio
	{
		public RemitoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<RemitoTransferidoDto> ObtenerRemitosTransferidos(string admId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Pendientes;
			var ps = new List<SqlParameter>()
			{
					new("@adm_id",admId),
					new("@ree_id","%")
			};
			var listaTemp = _repository.EjecutarLstSpExt<RemitoTransferidoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> SeteaEstado(RSetearEstadoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Setea_Estado;
			var ps = new List<SqlParameter>()
			{
					new("@re_compte",request.remCompte),
					new("@ree_id",request.estado)
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RemitoVerConteoDto> VerConteos(string remCompte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Ver_Conteos;
			var ps = new List<SqlParameter>()
			{
					new("@re_compte",remCompte),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RemitoVerConteoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> ConfirmaRecepcion(RConfirmaRecepcionRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Confirma;
			var ps = new List<SqlParameter>()
			{
					new("@re_compte",request.remCompte),
					new("@usu_id",request.usuario),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
