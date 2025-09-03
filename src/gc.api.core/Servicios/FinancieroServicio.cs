using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Text;

namespace gc.api.core.Servicios
{
	public class FinancieroServicio : Servicio<Financiero>, IFinancieroServicio
	{
		public FinancieroServicio(IUnitOfWork uow, IOptions<PaginationOptions> options) : base(uow, options)
		{

		}

		public List<PlanContableDto> GetPlanContableCuentaLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_CCB_CUENTA_LISTA;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<PlanContableDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroEstadoDto> GetFinancieroEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIERO_ESTADOS;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroEstadoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroDto> GetFinancierosPorTipoCfLista(string tcf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIEROS_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@tcf_id",tcf_id)
			};
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new FinancieroDto()
				{
					#region Campos
					ctaf_id = x.Ctaf_id,
					ctaf_denominacion = x.Ctaf_denominacion,
					ctaf_activo = x.Ctaf_activo,
					ctaf_lista = x.Ctaf_lista,
					#endregion
				}).ToList();
		}

		public List<FinancieroDto> GetFinancierosRelaPorTipoCfLista(string tcf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIEROS_RELA_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@tcf_id",tcf_id)
			};
			var res = _repository.InvokarSp2Lst(sp, ps, true);
			if (res.Count == 0)
				return [];
			else
				return res.Select(x => new FinancieroDto()
				{
					#region Campos
					ctaf_id = x.Ctaf_id,
					ctaf_denominacion = x.Ctaf_denominacion,
					ctaf_activo = x.Ctaf_activo,
					ctaf_lista = x.Ctaf_lista,
					#endregion
				}).ToList();
		}

		public List<FinancieroDesdeSeleccionDeTipoDto> GetFinancieroDesdeTipoParaSeleccionDeValores(string tcf_id, string adm_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_SV_CTAF;
			var ps = new List<SqlParameter>()
			{
				new("@tcf_id",tcf_id),
				new("@adm_id",adm_id)
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroDesdeSeleccionDeTipoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroCarteraDto> GetFinancieroCarteraParaSeleccionDeValores(string ctaf_id, string cta_id = "%")
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_OP_SV_CARTERA;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",ctaf_id),
				new("@cta_id",cta_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroCarteraDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> FinancieroConfirmarTransferencia(ConfirmarTransferenciaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_TR_CONFIRMA;
			var ps = new List<SqlParameter>()
			{
				new("@ttra_id",request.ttra_id),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
				new("@tra_concepto",request.tra_concepto),
				new("@tra_fecha",request.tra_fecha),
				new("@json_o",request.json_o),
				new("@json_d",request.json_d),
				new("@json_encabezado",request.json_encabezado),
				new("@json_concepto",request.json_concepto),
				new("@json_otro",request.json_otro),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroTraRepoCtagDto> GetFinancieroTraRepoCtag(string tra_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_TRA_REPO_CTAG;
			var ps = new List<SqlParameter>()
			{
				new("@tra_compte",tra_compte),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroTraRepoCtagDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroTraRepoDDto> GetFinancieroTraRepoDDto(string tra_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_TRA_REPO_D;
			var ps = new List<SqlParameter>()
			{
				new("@tra_compte",tra_compte),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroTraRepoDDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroCuentaAlCobroRelaDto> GetCuentaAlCobroRela(string ctaf_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CUENTA_AL_COBRO_RELA;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",ctaf_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroCuentaAlCobroRelaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroChequeDepositadoDto> GetFinancieroChequeDepositado(FinancieroChequeDepositadoRequest r)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CHEQUES_DEPOSITADOS;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",r.ctaf_id),
				new("@desde",r.fechaDesde),
				new("@hasta",r.fechaHasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroChequeDepositadoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<PerfilUserDto> GetFinancieroTraUsu(FinancieroTraUsuRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_TR_USUARIOS;
			var ps = new List<SqlParameter>()
			{
				new("@desde",request.FechaDesde),
				new("@hasta",request.FechaHasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<PerfilUserDto>(sp, ps, true);
			return listaTemp;
		}

		public List<MovimientoFinancieroListaDto> BuscarMovimientoFinanciero(ConsultaMovFinancierosRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_F_TR_LISTA;

			var ps = new List<SqlParameter>
			{
				new("@fecha_d", filtros.desde),
				new("@fecha_h", filtros.hasta),
				new("@ctaf_ori", filtros.ctaf_ori),
				new("@ctaf_des", filtros.ctaf_des),
				new("@tipo", filtros.tipo),
				new("@usu", filtros.usu)
			};

			//debo cargar aca todos los filtros sobre los parametros a utilizar
			if (filtros.ctaf_ori_list != null && filtros.ctaf_ori_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctaf_ori_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@ctaf_ori_list", sb.ToString() + ','));
			}
			if (filtros.ctaf_des_list != null && filtros.ctaf_des_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctaf_des_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@ctaf_des_list", sb.ToString() + ','));
			}
			if (filtros.tipo_list != null && filtros.tipo_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.tipo_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@tipo_list", sb.ToString() + ','));
			}
			if (filtros.usu_list != null && filtros.usu_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.usu_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@usu_list", sb.ToString() + ','));
			}

			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<MovimientoFinancieroListaDto> movFinan = _repository.EjecutarLstSpExt<MovimientoFinancieroListaDto>(sp, ps, true);

			return movFinan;
		}

		public List<RespuestaDto> MovimientoFinancieroAnular(MovimientoFinancieroAnularRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_TR_ANULAR;
			var ps = new List<SqlParameter>()
			{
				new("@tra_compte",request.tra_compte),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<MovimientoFinancieroListaDto> BuscarMovimientoFinancieroReporte(ConsultaMovFinancierosRequest filtros)
		{
			string sp = ConstantesGC.StoredProcedures.SP_F_TR_LISTA_REPORTE;

			var ps = new List<SqlParameter>
			{
				new("@fecha_d", filtros.desde),
				new("@fecha_h", filtros.hasta),
				new("@ctaf_ori", filtros.ctaf_ori),
				new("@ctaf_des", filtros.ctaf_des),
				new("@tipo", filtros.tipo),
				new("@usu", filtros.usu)
			};

			if (filtros.ctaf_ori_list != null && filtros.ctaf_ori_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctaf_ori_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@ctaf_ori_list", sb.ToString() + ','));
			}
			if (filtros.ctaf_des_list != null && filtros.ctaf_des_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctaf_des_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@ctaf_des_list", sb.ToString() + ','));
			}
			if (filtros.tipo_list != null && filtros.tipo_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.tipo_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@tipo_list", sb.ToString() + ','));
			}
			if (filtros.usu_list != null && filtros.usu_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.usu_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}

				ps.Add(new SqlParameter("@usu_list", sb.ToString() + ','));
			}

			List<MovimientoFinancieroListaDto> movFinan = _repository.EjecutarLstSpExt<MovimientoFinancieroListaDto>(sp, ps, true);

			return movFinan;
		}
	}
}
