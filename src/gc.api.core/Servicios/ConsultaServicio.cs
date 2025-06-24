using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Security.Claims;

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
		public List<CertRetenIBDto> ConsultaCertRetenIB(string op_compte)
		{
			var sp = ConstantesGC.StoredProcedures.SP_C_CERT_RETEN_IB;
			var ps = new List<SqlParameter>() {
				new("@op_compte",op_compte),
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
	}
}
