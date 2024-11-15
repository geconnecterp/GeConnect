using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace gc.api.core.Servicios
{
    public class RemitoServicio : Servicio<Remito>, IRemitoServicio
	{
		public RemitoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<RemitoGenDto> ObtenerRemitosPendientes(string admId,string reeId="%")
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Pendientes;
			var ps = new List<SqlParameter>()
			{
					new("@adm_id",admId),
					new("@ree_id",reeId)
			};
			var listaTemp = _repository.EjecutarLstSpExt<RemitoGenDto>(sp, ps, true);
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

        public RespuestaDto VerificaProductoEnRemito(string remCompte, string pId)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Verifica_Producto;
            var ps = new List<SqlParameter>()
            {
                    new("@re_compte",remCompte),
                    new("@p_id",pId)
            };
            var resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return resp.First();
        }

        public RespuestaDto RTRCargarConteos(CargarJsonGenRequest request, bool esModificacion)
        {
            var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Cargar_Conteos;
            var ps = new List<SqlParameter>()
            {
                new("@json",request.json_str),
                new("@ul_modifica",esModificacion),     				
            };
            var resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
            return resp.First();
        }

		public List<RTRxULDto> RTRCargarConteosXUL(string reCompte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RTR_Cargar_Conteos_x_ul;
			var ps = new List<SqlParameter>()
			{
				new("@re_compte",reCompte),
			};
			var resp = _repository.EjecutarLstSpExt<RTRxULDto>(sp, ps, true);
			return resp;
		}
	}
}
