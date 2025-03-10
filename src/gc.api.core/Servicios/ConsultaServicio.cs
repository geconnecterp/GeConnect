using gc.api.core.Constantes;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
using System.Diagnostics.Metrics;

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

        public List<ConsPagosDto> ConsultaOrdenesDePagoProveedor(string ctaId, DateTime fd, DateTime fh, string tipoOP, string userId)
        {
            throw new NotImplementedException();
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
    }
}
