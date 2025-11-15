using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Security.Claims;
using System.Text;

namespace gc.api.core.Servicios
{
    public class ConsultaServicio : Servicio<Cuenta>, IConsultaServicio
    {
        public ConsultaServicio(IUnitOfWork uow):base(uow)
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
				ps.Add(new SqlParameter("@ctc_list", sb.ToString() + ','));
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
				ps.Add(new SqlParameter("@ope_list", sb.ToString() + ','));
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

			return movFinan;
		}
	}
}
