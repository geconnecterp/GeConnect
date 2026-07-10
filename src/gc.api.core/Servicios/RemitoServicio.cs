using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Rpr;
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
		public List<RemitoGenDto> ObtenerRemitosPendientes(string admId, string reeId = "%")
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

		public List<RemitoExternoValidaDto> CargarProductosDesdeComprobante(RemitoExternoValidaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RE_VALIDA;
			var ps = new List<SqlParameter>()
			{
					new("@pre_id",request.pre_id),
					new("@tco_id",request.tco_id),
					new("@cm_compte",request.cm_compte)
			};
			var listaTemp = _repository.EjecutarLstSpExt<RemitoExternoValidaDto>(sp, ps, true);
			return listaTemp;
		}

		public RespuestaDto ConfirmarRemitoExterno(ConfirmarRemitoExternoRequest request)
		{
			// Normalizar valores vacíos → null
			string tco_id = string.IsNullOrWhiteSpace(request.tco_id) ? null : request.tco_id;
			string cm_compte = string.IsNullOrWhiteSpace(request.cm_compte) ? null : request.cm_compte;
			string pre_id = string.IsNullOrWhiteSpace(request.pre_id) ? null : request.pre_id;

			var sp = Constantes.ConstantesGC.StoredProcedures.SP_RE_CONFIRMAR;

			var ps = new List<SqlParameter>()
			{
				new("@opcion", request.opcion),
				new("@cta_id", request.cta_id),
				new("@tco_id", (object)tco_id ?? DBNull.Value),
				new("@cm_compte", (object)cm_compte ?? DBNull.Value),
				new("@pre_id", (object)pre_id ?? DBNull.Value),
				new("@re_obs", request.re_obs),
				new("@adm_id", request.adm_id),
				new("@usu_id", request.usu_id),
				new("@json", request.json)
			};

			var resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return resp.First();
		}


		//public RespuestaDto ConfirmarRemitoExterno(ConfirmarRemitoExternoRequest request)
		//{
		//	// Normalizar valores vacíos → string vacío
		//	string tco_id = string.IsNullOrWhiteSpace(request.tco_id) ? "" : request.tco_id;
		//	string cm_compte = string.IsNullOrWhiteSpace(request.cm_compte) ? "" : request.cm_compte;
		//	string pre_id = string.IsNullOrWhiteSpace(request.pre_id) ? "" : request.pre_id;

		//	var sp = Constantes.ConstantesGC.StoredProcedures.SP_RE_CONFIRMAR;

		//	var ps = new List<SqlParameter>()
		//	{
		//		new("@opcion", request.opcion),
		//		new("@cta_id", request.cta_id),
		//		new("@tco_id", tco_id),          // 🔥 ahora envía "" si venía vacío
		//		new("@cm_compte", cm_compte),    // 🔥 idem
		//		new("@pre_id", pre_id),          // 🔥 idem
		//		new("@re_obs", request.re_obs),
		//		new("@adm_id", request.adm_id),
		//		new("@usu_id", request.usu_id),
		//		new("@json", request.json)
		//	};

		//	var resp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
		//	return resp.First();
		//}

	}
}
