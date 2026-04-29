using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using Microsoft.Data.SqlClient;

namespace gc.api.core.Servicios
{
	public class ApiVentasServicio : Servicio<EntidadBase>, IApiVentasServicio
	{
		public ApiVentasServicio(IUnitOfWork uow) : base(uow)
		{
		}

		public List<VtasPVCtlProcesoDto> ObtenerVtasPVCtlProcesosLista(string adm_id)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_PROCESOS;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@adm_id", adm_id),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlProcesoDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlCierresDto> ObtenerVtasPVCtlCierresLista(string caja_nro_proceso)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_CIERRES;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlCierresDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlRendDto> ObtenerVtasPVCtlRendLista(string caja_nro_proceso, int caja_nro_cierre)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_REND;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", caja_nro_cierre),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlRendDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlRendDetalleDto> ObtenerVtasPVCtlRendDetalleLista(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_REND_DETALLE;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", caja_nro_rend),
				new SqlParameter("@tcf_id", tcf_id),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlRendDetalleDto>(sp, ps, true);
			return result;
		}

		public RespuestaDto CargaCtlNuevoItemDetalle(CargaCtlNuevoItemDetalleRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_NUEVO;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", request.caja_nro_rend),
				new SqlParameter("@tcf_id", request.tcf_id),
				new SqlParameter("@nuevo_tcf", request.nuevo_tcf),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al cargar ctl detalle"
				};
			}
		}

		public RespuestaDto GuardarCtlDetalle(GuardarCtlDetalleRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_GUARDAR;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", request.caja_nro_rend),
				new SqlParameter("@tcf_id", request.tcf_id),
				new SqlParameter("@json_rend", request.json_rend),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
				new SqlParameter("@app", request.app),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al guardar ctl detalle"
				};
			}
		}

		public RespuestaDto ConfirmarCtlArqueo(ConfirmarCtlArqueoRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_CONFIRMAR;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", request.caja_nro_rend),
				new SqlParameter("@tcf_id", request.tcf_id),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al guardar ctl detalle"
				};
			}
		}

		public RespuestaDto AnularCtlArqueo(AnularCtlArqueoRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_ANULAR;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", request.caja_nro_rend),
				new SqlParameter("@tcf_id", request.tcf_id),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al guardar ctl detalle"
				};
			}
		}

		public RespuestaDto AgregarMedioDePago(AgregarMedioDePagoRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_NUEVO;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@caja_nro_rend", request.caja_nro_rend),
				new SqlParameter("@tcf_id", request.tcf_id),
				new SqlParameter("@nuevo_tcf", request.nuevo_tcf),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al guardar ctl detalle"
				};
			}
		}

		public RespuestaDto ConfirmacionContable(ConfirmacionContableRequest request)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_CCB;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@caja_nro_proceso", request.caja_nro_proceso),
				new SqlParameter("@caja_nro_cierre", request.caja_nro_cierre),
				new SqlParameter("@adm_id", request.adm_id),
				new SqlParameter("@usu_id", request.usu_id),
			};

			var result = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			if (result != null && result.Count > 0)
			{
				return result[0];
			}
			else
			{
				return new RespuestaDto()
				{
					resultado = -1,
					resultado_msj = "Error al guardar ctl detalle"
				};
			}
		}

		public List<VtasPVCtlEntregaDto> ObtenerVtasPVCtlEntregaLista(string adm_id, char estado)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_ENTREGAS;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@adm_id", adm_id),
				new SqlParameter("@estado", estado),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlEntregaDto>(sp, ps, true);
			return result;
		}

		public List<VtasPVCtlEntregaRendDto> ObtenerVtasPVCtlEntregaRendLista(string ent_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_VTAS_PV_CTL_ENTREGAS_REND;

			var ps = new List<SqlParameter>()
			{
				new SqlParameter("@ent_compte", ent_compte),
			 };

			var result = _repository.EjecutarLstSpExt<VtasPVCtlEntregaRendDto>(sp, ps, true);
			return result;
		}
	}
}
