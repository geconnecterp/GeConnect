using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Servicios
{
	public class OrdenDePagoServicio : Servicio<OrdenDePago>, IOrdenDePagoServicio
	{
		public OrdenDePagoServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{
		}
		public List<OPValidacionPrevDto> GetOPValidacionesPrev(string cta_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_VALIDACIONES_PREV;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", cta_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OPValidacionPrevDto>(sp, ps, true);
			return listaTemp;
		}

		public List<OPDebitoYCreditoDelProveedorDto> GetOPDebitoYCreditoDelProveedor(string cta_id, char tipo, bool excluye_notas, string admId, string usuId)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_VTO;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", cta_id),
				new("@tipo", tipo),
				new("@excluye_notas", excluye_notas)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OPDebitoYCreditoDelProveedorDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaRelaDto> CargarSacarOPDebitoCreditoDelProveedor(CargarOSacarObligacionesOCreditosRequest r)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_CARGAR_SACAR;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", r.cta_id),
				new("@dia_movi", r.dia_movi),
				new("@tco_id", r.tco_id),
				new("@cm_compte", r.cm_compte),
				new("@cm_compte_cuota", r.cuota),
				new("@cv_importe", r.cv_importe),
				new("@accion", r.accion),
				
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaRelaDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Carga las retenciones desde las obligaciones y creditos seleccionados
		/// </summary>
		/// <param name="r">CargarRetencionesDesdeObligYCredSeleccionadosRequest</param>
		/// <returns>Lista de objetos RetencionesDesdeObligYCredDto</returns>
		public List<RetencionesDesdeObligYCredDto> CargarRetencionesDesdeObligYCredSeleccionados(CargarRetencionesDesdeObligYCredSeleccionadosRequest r)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_RETENCIONES;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", r.cta_id),
				new("@json_d", r.json_d),
				new("@json_h", r.json_h),

			};
			var listaTemp = _repository.EjecutarLstSpExt<RetencionesDesdeObligYCredDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Carga los valores desde las obligaciones y creditos seleccionados
		/// </summary>
		/// <param name="r"></param>
		/// <returns>Lista de objetos ValoresDesdeObligYCredDto</returns>
		public List<ValoresDesdeObligYCredDto> CargarValoresDesdeObligYCredSeleccionados(CargarValoresDesdeObligYCredSeleccionadosRequest r)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_VALORES;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", r.cta_id),
				new("@json_d", r.json_d),
				new("@json_h", r.json_h),

			};
			var listaTemp = _repository.EjecutarLstSpExt<ValoresDesdeObligYCredDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> ConfirmarOrdenDePagoAProveedor(ConfirmarOPaProveedorRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id",request.cta_id),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
				new("@opt_id",request.opt_id),
				new("@op_desc",request.op_desc),
				new("@json_d",request.json_d),
				new("@json_h",request.json_h),
				new("@json_r",request.json_r),
				new("@json_v",request.json_v),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<OPMotivoCtagDto> CargarOPMotivosCtag(string opt_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_MOTIVOS_CTAG;
			var ps = new List<SqlParameter>()
			{
				new("@opt_id", opt_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<OPMotivoCtagDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> ConfirmarOrdenDePagoDirecta(ConfirmarOrdenDePagoDirectaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OPD_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
				new("@opt_id",request.opt_id),
				new("@op_desc",request.op_desc),
				new("@json_encabezado",request.json_encabezado),
				new("@json_concepto",request.json_concepto),
				new("@json_otro",request.json_otro),
				new("@json_v",request.json_v),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
