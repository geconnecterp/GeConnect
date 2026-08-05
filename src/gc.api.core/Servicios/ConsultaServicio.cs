using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using Microsoft.Data.SqlClient;
using System.Text;

namespace gc.api.core.Servicios
{
	public class ConsultaServicio : Servicio<Cuenta>, IConsultaServicio
	{
		public ConsultaServicio(IUnitOfWork uow) : base(uow)
		{

		}
		public List<ConsCompDetDto> ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_COMPROBANTES_DET;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@periodo",mes),
				new SqlParameter("@rela_cuit",relCuit),
				new SqlParameter("@usu_id",userId),
			};

			List<ConsCompDetDto> res = _repository.EjecutarLstSpExt<ConsCompDetDto>(sp, ps, true);
			return res;
		}

		public List<ConsCompTotDto> ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_COMPROBANTES_TOT;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@meses",meses),
				new SqlParameter("@rela_cuit",relCuit),
				new SqlParameter("@usu_id",userId),
			};

			List<ConsCompTotDto> res = _repository.EjecutarLstSpExt<ConsCompTotDto>(sp, ps, true);
			return res;
		}

		public List<ConsCtaCteDto> ConsultarCuentaCorriente(string ctaId, DateTime fechaD, string userId, int pag, int regs)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_CTACTE;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@desde",fechaD),
				new SqlParameter("@usu_id",userId),
				new SqlParameter("@registros",regs),
				new SqlParameter("@pagina",pag),
			};

			List<ConsCtaCteDto> res = _repository.EjecutarLstSpExt<ConsCtaCteDto>(sp, ps, true);
			return res;
		}

		public List<ConsVtoDto> ConsultaVencimientoComprobantesNoImputados(string ctaId, DateTime fechaD, DateTime fechaH, string userId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_VENCIMIENTOS_CMP_SINPUTAR;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@desde",fechaD),
				new SqlParameter("@hasta",fechaH),
				new SqlParameter("@usu_id",userId),
			};

			List<ConsVtoDto> res = _repository.EjecutarLstSpExt<ConsVtoDto>(sp, ps, true);
			return res;
		}

		public List<ConsOrdPagosDto> ConsultaOrdenesDePagoProveedor(string ctaId, DateTime fd, DateTime fh, string tipoOP, string userId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_OPAGO_PROVEEDORES;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@fecha_d",fd),
				new SqlParameter("@fecha_h",fh),
				new SqlParameter("@opt_id",tipoOP),
				new SqlParameter("@usu_id",userId),
			};

			List<ConsOrdPagosDto> res = _repository.EjecutarLstSpExt<ConsOrdPagosDto>(sp, ps, true);
			return res;
		}

		public List<ConsOrdPagosDetDto> ConsultaOrdenesDePagoProveedorDetalle(string cmptId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_OPAGO_PROVEEDORES_DET;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@op_compte",cmptId) ,

			};

			List<ConsOrdPagosDetDto> res = _repository.EjecutarLstSpExt<ConsOrdPagosDetDto>(sp, ps, true);
			return res;
		}

		public List<ConsRecepcionProveedorDto> ConsultaRecepcionProveedor(string ctaId, DateTime fd, DateTime fh, string admId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_RECEPCIONES_PROV;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@cta_id",ctaId) ,
				new SqlParameter("@fecha_d",fd),
				new SqlParameter("@fecha_h",fh),
				new SqlParameter("@adm_id",admId),
			};

			List<ConsRecepcionProveedorDto> res = _repository.EjecutarLstSpExt<ConsRecepcionProveedorDto>(sp, ps, true);
			return res;
		}

		public List<ConsRecepcionProveedorDetalleDto> ConsultaRecepcionProveedorDetalle(string cmptId)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_RECEPCIONES_PROV_DET;
			var ps = new List<SqlParameter>() {
				new SqlParameter("@rp_compte",cmptId) ,
			};

			List<ConsRecepcionProveedorDetalleDto> res = _repository.EjecutarLstSpExt<ConsRecepcionProveedorDetalleDto>(sp, ps, true);
			return res;
		}

		public List<ConsOrdPagoDetExtendDto> ConsultaOrdenDePagoProveedor(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_OPAGO_PROVEEDORES_DET;
			var ps = new List<SqlParameter>() {
				new("@op_compte",op_compte),
			};

			List<ConsOrdPagoDetExtendDto> res = _repository.EjecutarLstSpExt<ConsOrdPagoDetExtendDto>(sp, ps, true);
			return res;
		}

		public List<CertRetenGananDto> ConsultaCertRetenGA(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_GA;
			var ps = new List<SqlParameter>() {
				new("@op_compte",op_compte),
			};

			List<CertRetenGananDto> res = _repository.EjecutarLstSpExt<CertRetenGananDto>(sp, ps, true);
			return res;
		}
		public List<CertRetenGananDto> ConsultaCertRetenGAFromList(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_GA_FROM_LIST;
			var ps = new List<SqlParameter>() {
				new("@op_compte_lista",op_compte),
			};

			List<CertRetenGananDto> res = _repository.EjecutarLstSpExt<CertRetenGananDto>(sp, ps, true);
			return res;
		}
		public List<CertRetenIBDto> ConsultaCertRetenIB(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_IB;
			var ps = new List<SqlParameter>() {
				new("@op_compte",op_compte),
			};

			List<CertRetenIBDto> res = _repository.EjecutarLstSpExt<CertRetenIBDto>(sp, ps, true);
			return res;
		}
		public List<CertRetenIBDto> ConsultaCertRetenIBFromList(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_IB_FROM_LIST;
			var ps = new List<SqlParameter>() {
				new("@op_compte_lista",op_compte),
			};

			List<CertRetenIBDto> res = _repository.EjecutarLstSpExt<CertRetenIBDto>(sp, ps, true);
			return res;
		}
		public List<CertRetenIVADto> ConsultaCertRetenIVA(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_IVA;
			var ps = new List<SqlParameter>() {
				new("@op_compte",op_compte),
			};

			List<CertRetenIVADto> res = _repository.EjecutarLstSpExt<CertRetenIVADto>(sp, ps, true);
			return res;
		}
		public List<CertRetenIVADto> ConsultaCertRetenIVAFromList(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_IVA_FROM_LIST;
			var ps = new List<SqlParameter>() {
				new("@op_compte_lista",op_compte),
			};

			List<CertRetenIVADto> res = _repository.EjecutarLstSpExt<CertRetenIVADto>(sp, ps, true);
			return res;
		}

		/// <summary>
		/// Busca los anticipos financieros de empleados en base a los filtros recibidos
		/// </summary>
		/// <param name="filtros">Filtros de búsqueda y paginación.</param>
		/// <returns>Lista de anticipos financieros.</returns>
		public List<VencimientoListaDto> ConsultarVencimientosPorTipo(ConsultarVencimientosRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_CONS_VENCIMIENTOS_POR_TIPO;

			var ps = new List<SqlParameter>();

			if (filtros.fv)
			{
				ps.Add(new SqlParameter("@fv", "1"));
				ps.Add(new SqlParameter("@dv", filtros.fvDesde));
				ps.Add(new SqlParameter("@hv", filtros.fvhasta));
			}
			else
				ps.Add(new SqlParameter("@fv", "0"));

			if (filtros.fg)
			{
				ps.Add(new SqlParameter("@fc", "1"));
				ps.Add(new SqlParameter("@dc", filtros.fgDesde));
				ps.Add(new SqlParameter("@hc", filtros.fghasta));
			}
			else
				ps.Add(new SqlParameter("@fc", "0"));

			if (filtros.id_ctc)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctc_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@id_ctc", "1"));
				ps.Add(new SqlParameter("@ctc_list", FormatList(sb.ToString())));
			}
			else
				ps.Add(new SqlParameter("@id_ctc", "0"));

			if (filtros.id_ope)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ope_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@id_ope", "1"));
				ps.Add(new SqlParameter("@ope_list", FormatList(sb.ToString())));
			}
			else
				ps.Add(new SqlParameter("@id_ope", "0"));

			if (filtros.id_tco)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.tco_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@id_tco", "1"));
				ps.Add(new SqlParameter("@tco_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@id_tco", "0"));

			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<VencimientoListaDto> movFinan = _repository.EjecutarLstSpExt<VencimientoListaDto>(sp, ps, true);
			Console.WriteLine("=== Parámetros enviados al SP ===");
			foreach (var p in ps)
			{
				Console.WriteLine($"{p.ParameterName} = {p.Value}");
			}
			Console.WriteLine("=================================");
			return movFinan;
		}

		string FormatList(string value)
		{
			return value == "%" ? value : value + ",";
		}

		public List<CertificadoListaDto> ConsultarCertificadosNRNP(ConsultarCertificadosRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_CONS_CERT_NRNP;

			var ps = new List<SqlParameter>
			{
				new SqlParameter("@imp_id", filtros.imp_id),
				new SqlParameter("@ret", filtros.ret),
				new SqlParameter("@per", filtros.per),
				new SqlParameter("@no_vencido", filtros.no_vencido),
				new SqlParameter("@vencido", filtros.vencido),
				new SqlParameter("@registros", filtros.Registros),
				new SqlParameter("@pagina", filtros.Pagina),
				new SqlParameter("@ordenar", filtros.Sort ?? "")
			};

			List<CertificadoListaDto> certNRNP = _repository.EjecutarLstSpExt<CertificadoListaDto>(sp, ps, true);

			return certNRNP;
		}

		public List<ProductoStkDto> ConsultarProductoStk(ConsultarStockRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_LISTA;

			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lDep != null && filtros.lDep.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lDep)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@depo", "1"));
				ps.Add(new SqlParameter("@depo_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@depo", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@stock_p", filtros.chkStkPos));
			ps.Add(new SqlParameter("@stock_0", filtros.chkStkCero));
			ps.Add(new SqlParameter("@stock_n", filtros.chkStkNeg));
			ps.Add(new SqlParameter("@activo", filtros.chkEstAct));
			ps.Add(new SqlParameter("@discontinuo", filtros.chkEstDisc));

			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<ProductoStkDto> lstProductos = _repository.EjecutarLstSpExt<ProductoStkDto>(sp, ps, true);

			return lstProductos;
		}

		public List<ProductoStkDto> ConsultarProductoStkValor(ConsultarStockValorizadoRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = "";

			switch (filtros.agrupador)
			{
				case 0:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_P;
					break;
				case 1:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_SEC;
					break;
				case 2:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_RUBG;
					break;
				case 3:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_RUB;
					break;
				case 4:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_CTA;
					break;
				default:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_P;
					break;
			}

			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lDep != null && filtros.lDep.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lDep)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@depo", "1"));
				ps.Add(new SqlParameter("@depo_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@depo", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@stock_p", filtros.chkStkPos));
			ps.Add(new SqlParameter("@stock_0", filtros.chkStkCero));
			ps.Add(new SqlParameter("@stock_n", filtros.chkStkNeg));
			ps.Add(new SqlParameter("@activo", filtros.chkEstAct));
			ps.Add(new SqlParameter("@discontinuo", filtros.chkEstDisc));
			ps.Add(new SqlParameter("@costo_repo", filtros.chkCostoRepo));

			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<ProductoStkDto> lstProductos = _repository.EjecutarLstSpExt<ProductoStkDto>(sp, ps, true);

			return lstProductos;
		}

		public List<ProductoStkCompensadoDto> ConsultarProductoStkCompensado(ConsultarStockCompensadoRequest filtros)
		{
			filtros.Pagina = filtros.Pagina == null || filtros.Pagina <= 0 ? _pagSet.DefaultPageNumber : filtros.Pagina;
			filtros.Registros = filtros.Registros == null || filtros.Registros <= 0 ? _pagSet.DefaultPageSize : filtros.Registros;

			string sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_COMP;

			var ps = new List<SqlParameter>();

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@activo", filtros.chkEstAct));
			ps.Add(new SqlParameter("@discontinuo", filtros.chkEstDisc));
			ps.Add(new SqlParameter("@stk_compensado", filtros.diferencia));

			ps.Add(new SqlParameter("@registros", filtros.Registros));
			ps.Add(new SqlParameter("@pagina", filtros.Pagina));
			ps.Add(new SqlParameter("@ordenar", filtros.Sort ?? ""));

			List<ProductoStkCompensadoDto> lstProductos = _repository.EjecutarLstSpExt<ProductoStkCompensadoDto>(sp, ps, true);

			return lstProductos;
		}

		public List<MovimientoListaDto> ConsultaMovimientoLista(BuscarMovDeCuentaDirectaRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_G_MOVIMIENTOS;
			var ps = new List<SqlParameter>();
			if (filtros.ctag_list != null && filtros.ctag_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ctag_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				if (sb.Length > 1)
					ps.Add(new SqlParameter("@ctag_list", sb.ToString() + ','));
				else
					ps.Add(new SqlParameter("@ctag_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@ctag_list", "%"));


			ps.Add(new SqlParameter("@desde", filtros.desde));
			ps.Add(new SqlParameter("@hasta", filtros.hasta));

			List<MovimientoListaDto> res = _repository.EjecutarLstSpExt<MovimientoListaDto>(sp, ps, true);
			return res;
		}

		public List<SaldoDetalleDto> BuscarSaldoDetalleCtaDistribuidora(BuscarSaldoDetalleRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_VTO_X_VE;
			var ps = new List<SqlParameter>();
			if (filtros.ve_list != null && filtros.ve_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ve_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				if (sb.Length > 1)
					ps.Add(new SqlParameter("@ve_list", sb.ToString() + ','));
				else
					ps.Add(new SqlParameter("@ve_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@ve_list", "%"));


			List<SaldoDetalleDto> res = _repository.EjecutarLstSpExt<SaldoDetalleDto>(sp, ps, true);
			return res;
		}

		public List<SaldoResumenDto> BuscarSaldoResumenCtaDistribuidora(BuscarSaldoDetalleRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_VTO_X_VE_RESUMEN;
			var ps = new List<SqlParameter>();
			if (filtros.ve_list != null && filtros.ve_list.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.ve_list)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				if (sb.Length > 1)
					ps.Add(new SqlParameter("@ve_list", sb.ToString() + ','));
				else
					ps.Add(new SqlParameter("@ve_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@ve_list", "%"));


			List<SaldoResumenDto> res = _repository.EjecutarLstSpExt<SaldoResumenDto>(sp, ps, true);
			return res;
		}

		public List<ComisionesDeVendedoresDetalleDto> BuscarComisionDeVendedorDetalle(ComisionesDeVendedoresRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_COMISIONES_VE;
			var ps = new List<SqlParameter>
			{
				new SqlParameter("@desde", filtros.Desde),
				new SqlParameter("@hasta", filtros.Hasta)
			};
			List<ComisionesDeVendedoresDetalleDto> res = _repository.EjecutarLstSpExt<ComisionesDeVendedoresDetalleDto>(sp, ps, true);
			return res;
		}

		public List<ComisionesDeVendedoresResumenDto> BuscarComisionDeVendedorResumen(ComisionesDeVendedoresRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_COMISIONES_VE_RESUMEN;
			var ps = new List<SqlParameter>
			{
				new SqlParameter("@desde", filtros.Desde),
				new SqlParameter("@hasta", filtros.Hasta)
			};
			List<ComisionesDeVendedoresResumenDto> res = _repository.EjecutarLstSpExt<ComisionesDeVendedoresResumenDto>(sp, ps, true);
			return res;
		}

		public List<ComisionesDeRepartidoresDetalleDto> BuscarComisionDeRepartidorDetalle(ComisionesDeRepartidoresRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_COMISIONES_RP;
			var ps = new List<SqlParameter>
			{
				new SqlParameter("@desde", filtros.Desde),
				new SqlParameter("@hasta", filtros.Hasta)
			};
			List<ComisionesDeRepartidoresDetalleDto> res = _repository.EjecutarLstSpExt<ComisionesDeRepartidoresDetalleDto>(sp, ps, true);
			return res;
		}

		public List<ComisionesDeRepartidoresResumenDto> BuscarComisionDeRepartidorResumen(ComisionesDeRepartidoresRequest filtros)
		{
			var sp = ConstantesGC.StoredProcedures.SP_CONS_C_COMISIONES_RP_RESUMEN;
			var ps = new List<SqlParameter>
			{
				new SqlParameter("@desde", filtros.Desde),
				new SqlParameter("@hasta", filtros.Hasta)
			};
			List<ComisionesDeRepartidoresResumenDto> res = _repository.EjecutarLstSpExt<ComisionesDeRepartidoresResumenDto>(sp, ps, true);
			return res;
		}

		public List<RepRkgRentabVtasDto> RepRkgRentabVtas(ReporteRankingRentabVtasRequest filtros)
		{
			string sp = "";

			switch (filtros.agrupador)
			{
				case 0:
					sp = ConstantesGC.StoredProcedures.SP_E_RANKING_VTAS_P;
					break;
				case 1:
					sp = ConstantesGC.StoredProcedures.SP_E_RANKING_VTAS_SEC;
					break;
				case 2:
					sp = ConstantesGC.StoredProcedures.SP_E_RANKING_VTAS_RUB;
					break;
				case 3:
					sp = ConstantesGC.StoredProcedures.SP_E_RANKING_VTAS_CTA;
					break;
				default:
					sp = ConstantesGC.StoredProcedures.SP_CONS_STOCK_VALOR_P;
					break;
			}
			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@desde", filtros.desde));
			ps.Add(new SqlParameter("@hasta", filtros.hasta));
			ps.Add(new SqlParameter("@tipo", string.Empty));
			List<RepRkgRentabVtasDto> lstProductos = _repository.EjecutarLstSpExt<RepRkgRentabVtasDto>(sp, ps, true);

			return lstProductos;
		}

		public List<ReporteEvoVtasPerAnterioresDto> RepEvoVtasPerAnteriores(ReporteEvoVtasPerAnterioresRequest filtros)
		{
			string sp = "";

			switch (filtros.agrupador)
			{
				case 0:
					sp = ConstantesGC.StoredProcedures.SP_E_EVO_VTAS_ANUAL_P;
					break;
				case 1:
					sp = ConstantesGC.StoredProcedures.SP_E_EVO_VTAS_ANUAL_SEC;
					break;
				case 2:
					sp = ConstantesGC.StoredProcedures.SP_E_EVO_VTAS_ANUAL_RUB;
					break;
				case 3:
					sp = ConstantesGC.StoredProcedures.SP_E_EVO_VTAS_ANUAL_CTA;
					break;
				default:
					sp = ConstantesGC.StoredProcedures.SP_E_EVO_VTAS_ANUAL_P;
					break;
			}
			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@desde", filtros.desde));
			ps.Add(new SqlParameter("@hasta", filtros.hasta));
			ps.Add(new SqlParameter("@tipo", string.Empty));
			List<ReporteEvoVtasPerAnterioresDto> lstProductos = _repository.EjecutarLstSpExt<ReporteEvoVtasPerAnterioresDto>(sp, ps, true);

			return lstProductos;
		}

		public List<ReporteVarVtasYCompUltDoceMDto> RepoVarVtasYCompUltDoceM(ReporteVarVtasYCompUltDoceMRequest filtros)
		{
			string sp = "";

			switch (filtros.agrupador)
			{
				case 0:
					sp = ConstantesGC.StoredProcedures.SP_E_VAR_VTAS_COMP_P;
					break;
				case 1:
					sp = ConstantesGC.StoredProcedures.SP_E_VAR_VTAS_COMP_SEC;
					break;
				case 2:
					sp = ConstantesGC.StoredProcedures.SP_E_VAR_VTAS_COMP_RUB;
					break;
				case 3:
					sp = ConstantesGC.StoredProcedures.SP_E_VAR_VTAS_COMP_CTA;
					break;
				default:
					sp = ConstantesGC.StoredProcedures.SP_E_VAR_VTAS_COMP_P;
					break;
			}
			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@tipo", string.Empty));
			List<ReporteVarVtasYCompUltDoceMDto> lstProductos = _repository.EjecutarLstSpExt<ReporteVarVtasYCompUltDoceMDto>(sp, ps, true);

			return lstProductos;
		}

		public List<ReporteEvalDeNivelDeServicioDto> RepoEvalDeNivelDeServicio(ReporteEvalDeNivelDeServicioRequest filtros)
		{
			string sp = "";

			switch (filtros.agrupador)
			{
				case 0:
					sp = ConstantesGC.StoredProcedures.SP_E_NS_P;
					break;
				case 1:
					sp = ConstantesGC.StoredProcedures.SP_E_NS_SEC;
					break;
				case 2:
					sp = ConstantesGC.StoredProcedures.SP_E_NS_RUB;
					break;
				case 3:
					sp = ConstantesGC.StoredProcedures.SP_E_NS_CTA;
					break;
				default:
					sp = ConstantesGC.StoredProcedures.SP_E_NS_P;
					break;
			}
			var ps = new List<SqlParameter>();

			if (filtros.lSuc != null && filtros.lSuc.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lSuc)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@adm", "1"));
				ps.Add(new SqlParameter("@adm_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@adm", "0"));

			if (filtros.lProv != null && filtros.lProv.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lProv)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@prov", "1"));
				ps.Add(new SqlParameter("@prov_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@prov", "0"));

			if (filtros.lFam != null && filtros.lFam.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lFam)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@pg", "1"));
				ps.Add(new SqlParameter("@pg_list", sb.ToString() + ','));
			}
			else
				ps.Add(new SqlParameter("@pg", "0"));

			if (filtros.lRub != null && filtros.lRub.Count > 0)
			{
				StringBuilder sb = new();
				bool first = true;
				foreach (var item in filtros.lRub)
				{
					if (first)
						first = false;
					else
						sb.Append(',');

					sb.Append(item);
				}
				ps.Add(new SqlParameter("@rub", "1"));
				ps.Add(new SqlParameter("@rub_list", sb.ToString()));
			}
			else
				ps.Add(new SqlParameter("@rub", "0"));

			ps.Add(new SqlParameter("@tipo", string.Empty));
			List<ReporteEvalDeNivelDeServicioDto> lstProductos = _repository.EjecutarLstSpExt<ReporteEvalDeNivelDeServicioDto>(sp, ps, true);

			return lstProductos;
		}
	}
}
