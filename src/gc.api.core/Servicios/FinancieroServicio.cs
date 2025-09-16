using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
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
				new("@id_f",request.id_f),
				new("@ctaf_id",request.ctaf_id),
				new("@id_c",request.id_c),
				new("@cta_id",request.cta_id),
				new("@id_u",request.id_u),
				new("@usu_id",request.usu_id),
				new("@tipo_fecha",request.tipo_fecha),
				new("@desde",request.desde),
				new("@hasta",request.hasta),
				new("@estado",request.estado),
				new("@impreso",request.impreso),
			};
			var listaTemp = _repository.EjecutarLstSpExt<FinancieroBcoVencChequeEmitidoListaDto>(sp, ps, true);
			return listaTemp;
		}
	}
}
