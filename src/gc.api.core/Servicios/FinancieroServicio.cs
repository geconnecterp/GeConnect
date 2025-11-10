using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
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

		/// <summary>
		/// Obtiene la lista de cuentas del plan contable.
		/// </summary>
		/// <returns>Lista de cuentas contables.</returns>
		public List<PlanContableDto> GetPlanContableCuentaLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_CCB_CUENTA_LISTA;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<PlanContableDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene la lista de estados financieros disponibles.
		/// </summary>
		/// <returns>Lista de estados financieros.</returns>
		public List<FinancieroEstadoDto> GetFinancieroEstados()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_FINANCIERO_ESTADOS;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroEstadoDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene la lista de cuentas financieras por tipo.
		/// </summary>
		/// <param name="tcf_id">Identificador del tipo de cuenta financiera.</param>
		/// <returns>Lista de cuentas financieras.</returns>
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

		/// <summary>
		/// Obtiene la lista de cuentas financieras relacionadas por tipo.
		/// </summary>
		/// <param name="tcf_id">Identificador del tipo de cuenta financiera.</param>
		/// <returns>Lista de cuentas financieras relacionadas.</returns>
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

		/// <summary>
		/// Obtiene cuentas financieras para selección de valores según tipo y administración.
		/// </summary>
		/// <param name="tcf_id">Tipo de cuenta financiera.</param>
		/// <param name="adm_id">Identificador de administración.</param>
		/// <returns>Lista de cuentas financieras para selección.</returns>
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

		/// <summary>
		/// Obtiene la cartera financiera para selección de valores.
		/// </summary>
		/// <param name="ctaf_id">Identificador de cuenta financiera.</param>
		/// <param name="cta_id">Identificador de cuenta (opcional).</param>
		/// <returns>Lista de cartera financiera.</returns>
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

		/// <summary>
		/// Confirma una transferencia financiera.
		/// </summary>
		/// <param name="request">Datos de la transferencia a confirmar.</param>
		/// <returns>Lista de respuestas de la operación.</returns>
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

		/// <summary>
		/// Obtiene los datos de reporte de cuentas de transferencia (CTAG) por comprobante.
		/// </summary>
		/// <param name="tra_compte">Identificador de comprobante de transferencia.</param>
		/// <returns>Lista de datos de reporte CTAG.</returns>
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

		/// <summary>
		/// Obtiene los datos de reporte de detalle de transferencia por comprobante.
		/// </summary>
		/// <param name="tra_compte">Identificador de comprobante de transferencia.</param>
		/// <returns>Lista de datos de detalle de reporte.</returns>
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

		/// <summary>
		/// Obtiene las cuentas al cobro relacionadas a una cuenta financiera.
		/// </summary>
		/// <param name="ctaf_id">Identificador de cuenta financiera.</param>
		/// <returns>Lista de cuentas al cobro relacionadas.</returns>
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

		/// <summary>
		/// Obtiene los cheques depositados según los filtros recibidos.
		/// </summary>
		/// <param name="r">Parámetros de búsqueda de cheques depositados.</param>
		/// <returns>Lista de cheques depositados.</returns>
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

		/// <summary>
		/// Obtiene los usuarios relacionados a transferencias financieras en un rango de fechas.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de usuarios.</param>
		/// <returns>Lista de usuarios relacionados.</returns>
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

		/// <summary>
		/// Busca movimientos financieros según los filtros recibidos, con paginación.
		/// </summary>
		/// <param name="filtros">Filtros de búsqueda y paginación.</param>
		/// <returns>Lista de movimientos financieros.</returns>
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

		/// <summary>
		/// Anula un movimiento financiero.
		/// </summary>
		/// <param name="request">Datos del movimiento a anular.</param>
		/// <returns>Lista de respuestas de la operación.</returns>
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

		/// <summary>
		/// Busca movimientos financieros para reportes, según los filtros recibidos.
		/// </summary>
		/// <param name="filtros">Filtros de búsqueda.</param>
		/// <returns>Lista de movimientos financieros para reporte.</returns>
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

		/// <summary>
		/// Obtiene el extracto bancario según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de extracto bancario.</param>
		/// <returns>Lista de extractos bancarios.</returns>
		public List<FinancieroBcoExtractoDto> GetFinancieroBcoExtracto(FinancieroBcoExtractoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_EXTRACTO;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@desde",request.FechaDesde),
				new("@hasta",request.FechaHasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoExtractoDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene los movimientos de cuenta corriente bancaria según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de cuenta corriente.</param>
		/// <returns>Lista de movimientos de cuenta corriente bancaria.</returns>
		public List<FinancieroBcoCtaCteDto> GetFinancieroBcoCtaCte(FinancieroBcoCtaCteRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CTA_CTE;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@desde",request.FechaDesde),
				new("@hasta",request.FechaHasta),
				new("@tipo_filtro",request.tipo_filtro),
				new("@ct_tipo",request.ct_tipo),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoCtaCteDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene el resumen del libro bancario según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de libro resumen.</param>
		/// <returns>Lista de resúmenes de libro bancario.</returns>
		public List<FinancieroBcoLibroResumenDto> GetFinancieroBcoLibroResumen(FinancieroBcoLibroResumenRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_LIBRO_RESUMEN;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@hasta",request.hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoLibroResumenDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene el detalle del libro bancario según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de libro bancario.</param>
		/// <returns>Lista de detalles de libro bancario.</returns>
		public List<FinancieroBcoLibroDto> GetFinancieroBcoLibro(FinancieroBcoLibroRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_LIBRO;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@hasta",request.hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoLibroDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene los cheques emitidos próximos a vencer según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda de cheques emitidos.</param>
		/// <returns>Lista de cheques emitidos próximos a vencer.</returns>
		public List<FinancieroBcoVencChequeEmitidoDto> GetFinancieroBcoVencChequeEmitido(FinancieroBcoVencChequeEmitidoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_VTO_PROY;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@desde",request.desde),
				new("@hasta",request.hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoVencChequeEmitidoDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene el detalle de los cheques emitidos próximos a vencer según los filtros recibidos.
		/// </summary>
		/// <param name="request">Parámetros de búsqueda detallada de cheques emitidos.</param>
		/// <returns>Lista de detalles de cheques emitidos próximos a vencer.</returns>
		public List<FinancieroBcoVencChequeEmitidoListaDto> GetFinancieroBcoVencChequeEmitidoLista(FinancieroBcoVencChequeEmitidoListaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_EMITIDOS_LISTA;
			var ps = new List<SqlParameter>()
			{
				new("@id_f", request.id_f),
				new("@ctaf_id", request.ctaf_id),
				new("@id_c", request.id_c),
				new("@cta_id", request.cta_id),
				new("@id_u", request.id_u),
				new("@usu_id", request.usu_id),
				new("@tipo_fecha", request.tipo_fecha),
				new("@desde", request.desde),
				new("@hasta", request.hasta),
				new("@estado", request.estado),
				new("@impreso", request.impreso),
				new("@registros", 999999),
				new("@pagina", 1),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoVencChequeEmitidoListaDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Obtiene una lista de estados de cheques emitidos.
		/// </summary>
		/// 
		/// <returns>Lista de estados de cheques emitidos.</returns>
		public List<ChequeEmitidoEstadoDto> GetChequeEmitidoEstadoLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_ESTADOS;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<ChequeEmitidoEstadoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<ChequeModificadosListaDto> GetChequeModificadosLista(GetChequeModificadosListaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_MODIFICADOS;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@che_emision",request.che_emision),
			};
			var listaTemp = _repository.EjecutarLstSpExt<ChequeModificadosListaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> SetChequeModificar(GetChequeModificarListaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_MODIFICAR;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@che_emision",request.che_emision),
				new("@che_nro",request.che_nro),
				new("@che_fecha",request.che_fecha),
				new("@che_anombre",request.che_anombre),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> SetFechaDeEntrega(RegistrarFechaDeEntregaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_ENTREGA;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@che_emision",request.che_emision),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> SetRechazoDeCheque(RegistrarRechazoDeChequeRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_RECHAZAR;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@che_emision",request.che_emision),
				new("@fecha_rechazo",request.fecha_rechazo),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<ECheqDto> GetECheqLista(PasoPrevioECheqRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_CH_E_CHEQ;
			var ps = new List<SqlParameter>()
			{
				new("@json_che",request.json_che),
			};
			var listaTemp = _repository.EjecutarLstSpExt<ECheqDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> SetExtractoBancarioConfirmar(SetExtractoBancarioConfirmaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_EXTRACTO_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@json_extracto",request.json_extracto),
				new("@json_eliminado",request.json_eliminado),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<CrudExtractoBancarioDto> GetBcoExtractoDesdeFile(ExtractoBcoFileRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_BCO_EXTRACTO_FILE;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@tipo_file",request.tipo_file),
				new("@json_file",request.json_file),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<CrudExtractoBancarioDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroConciliaDatosDto> GetFinancieroConciliaDatos(FinancieroConciliaDatosRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CONCILIA_DATOS;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id", request.ctaf_id),
				new("@desde", request.desde),
				new("@hasta", request.hasta),
				new("@concilia", request.concilia),
				new("@select_conciliados", request.select_conciliados),
				new("@usu_id", request.usu_id),
				new("@adm_id", request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroConciliaDatosDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroConciliaNroDto> GetFinancieroConciliaNro(FinancieroConciliaNrosRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CONCILIA_NRO;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id", request.ctaf_id),
				new("@conciliado_nro", request.conciliado_nro),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroConciliaNroDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> FinancieroExtractoDesconcilia(FinancieroExtractoDesconciliaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CONCILIA_DESCONCILIA_NRO;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@conciliado_nro",request.conciliado_nro),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> FinancieroConciliacionExtractoConfirmar(FinancieroConciliacionExtractoConfirmarRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_CONCILIA_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@ctaf_id",request.ctaf_id),
				new("@json_e",request.json_e),
				new("@json_s",request.json_s),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<GastoProyListaDto> GetGastosProyLista()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_GASTOS_PROY_LISTA;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<GastoProyListaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<GastoProyListaDto> GetGastosProyDatos(int items)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_ABM_GASTOS_PROY_DATOS;
			var ps = new List<SqlParameter>()
			{
				new("@items",items),
			};
			var listaTemp = _repository.EjecutarLstSpExt<GastoProyListaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<ProyFinanDto> GetProyeccionFinanciera(BuscarProyFinanRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_REPO_FINAN_PROY;
			var ps = new List<SqlParameter>()
			{
				new("@desde", request.Desde),
				new("@hasta", request.Hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<ProyFinanDto>(sp, ps, true);
			return listaTemp;
		}

		public List<SaldoDeCuentaDto> GetSaldoDeCuentas(BuscarSaldoDeCuentasRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_REPO_FINAN_SALDOS;
			var ps = new List<SqlParameter>()
			{
				new("@fecha", request.Hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<SaldoDeCuentaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FlujoDeIngresoDto> GetFlujoDeIngreso(BuscarFlujoDeIngresoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_REPO_FINAN_FLUJO;
			var ps = new List<SqlParameter>()
			{
				new("@desde", request.Desde),
				new("@hasta", request.Hasta),
				new("@adm_id", request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FlujoDeIngresoDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> FinancieroAnticipoEmpleadoConfirma(CargaAnticipoEmpleadoRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_AN_CONFIRMA;
			var ps = new List<SqlParameter>()
			{
				new("@ant_id",request.ant_id),
				new("@an_concepto",request.an_concepto),
				new("@an_porc_interes",request.an_porc_interes),
				new("@cta_id_prov",request.cta_id),
				new("@json_an",request.json_anticipos),
				new("@adm_id",request.adm_id),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroTopeCtaDto> GetFinancieroTopePorCuenta(string cta_id)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_OBTENER_TOPE_CTA;
			var ps = new List<SqlParameter>()
			{
				new("@cta_id", cta_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroTopeCtaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<AnticipoDetalleDto> GetAnticipoDetalle(string an_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_AN_DETALLE;
			var ps = new List<SqlParameter>()
			{
				new("@an_compte", an_compte),
			};
			var listaTemp = _repository.EjecutarLstSpExt<AnticipoDetalleDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroUsuarioDto> GetFinancieroUsuarios(GetFinancieroUsuariosRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_AN_USU;
			var ps = new List<SqlParameter>()
			{
				new("@desde", request.desde),
				new("@hasta", request.hasta),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroUsuarioDto>(sp, ps, true);
			return listaTemp;
		}

		/// <summary>
		/// Busca los anticipos financieros de empleados en base a los filtros recibidos
		/// </summary>
		/// <param name="filtros">Filtros de búsqueda y paginación.</param>
		/// <returns>Lista de anticipos financieros.</returns>
		public List<AnticipoFinanEmpListaDto> BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_F_AN_LISTA;

			var ps = new List<SqlParameter>
			{
				new("@fecha_d", filtros.desde),
				new("@fecha_h", filtros.hasta),
			};

			if (filtros.cta)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.cta_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@cta", "1"));
				ps.Add(new SqlParameter("@cta_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@cta", "0"));
			if (filtros.tipo)
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
				ps.Add(new SqlParameter("@tipo", "1"));
				ps.Add(new SqlParameter("@tipo_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@tipo", "0"));
			if (filtros.usu)
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
				ps.Add(new SqlParameter("@usu", "1"));
				ps.Add(new SqlParameter("@usu_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@usu", "0"));
			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<AnticipoFinanEmpListaDto> movFinan = _repository.EjecutarLstSpExt<AnticipoFinanEmpListaDto>(sp, ps, true);

			return movFinan;
		}

		public List<RespuestaDto> FinancieroAnticipoAnular(FinancieroAnticipoAnularRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_AN_ANULA_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@an_compte",request.an_compte),
				new("@adm_id",request.adm_id),
				new("@usu_id",request.usu_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<FinancieroLEProximaDto> GetFinancieroProximaLE()
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_LE_PROXIMA;
			var ps = new List<SqlParameter>();
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroLEProximaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<LiqEmpCargaDto> GetLiqEmpCarga(FinancieroLiqEmpCargaRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_LE_CARGA;
			var ps = new List<SqlParameter>()
			{
				new("@periodo",request.periodo),
				new("@mes",request.mes),
				new("@json_topes",request.json_topes),
				new("@porc_tope",request.porc_tope),
			};
			var listaTemp = _repository.EjecutarLstSpExt<LiqEmpCargaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<RespuestaDto> FinancieroLiqEmpleadoConfirmar(FinancieroLiqEmpleadoConfirmarRequest request)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_LE_CONFIRMAR;
			var ps = new List<SqlParameter>()
			{
				new("@periodo",request.periodo),
				new("@mes",request.mes),
				new("@concepto",request.concepto),
				new("@actualiza_tope",request.actualiza_tope),
				new("@json_topes",request.json_tope),
				new("@porc_tope",request.porc_tope),
				new("@json_dto",request.json_detalle),
				new("@usu_id",request.usu_id),
				new("@adm_id",request.adm_id),
			};
			var listaTemp = _repository.EjecutarLstSpExt<RespuestaDto>(sp, ps, true);
			return listaTemp;
		}

		public List<LiqEmpleadoDetalleParaReporteDto> GetLiqEmpDetalleParaReporte(string le_compte)
		{
			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_LE_DETALLE;
			var ps = new List<SqlParameter>()
			{
				new("@le_compte",le_compte),
			};
			var listaTemp = _repository.EjecutarLstSpExt<LiqEmpleadoDetalleParaReporteDto>(sp, ps, true);
			return listaTemp;
		}

		public List<LiqDeEmpleadoListaDto> BuscarLiquidacionesDeEmpleados(ConsultaLiqDeEmpleadoRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			var sp = Constantes.ConstantesGC.StoredProcedures.SP_F_LE_LISTA;
			var ps = new List<SqlParameter>
			{
				new("@fecha_d", filtros.desde),
				new("@fecha_h", filtros.hasta),
			};
			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<LiqDeEmpleadoListaDto> movFinan = _repository.EjecutarLstSpExt<LiqDeEmpleadoListaDto>(sp, ps, true);

			return movFinan;
		}
	}
}
