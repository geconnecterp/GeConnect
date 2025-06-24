using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Consultas;

namespace gc.api.core.Contratos.Servicios
{
    public interface IConsultaServicio : IServicio<Cuenta>
    {
        List<ConsCtaCteDto> ConsultarCuentaCorriente(string ctaId, DateTime fechaD, string userId,int pag,int reg);
        List<ConsVtoDto> ConsultaVencimientoComprobantesNoImputados(string ctaId, DateTime fechaD, DateTime fechaH, string userId);
        List<ConsCompTotDto> ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctaId"></param>
        /// <param name="mes">el formato de este parametro es aaaamm</param>
        /// <param name="relCuit"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        List<ConsCompDetDto> ConsultaComprobantesMesDetalle(string ctaId,string mes,bool relCuit, string userId);
        List<ConsOrdPagosDto> ConsultaOrdenesDePagoProveedor(string ctaId, DateTime fd, DateTime fh, string tipoOP, string userId);
        List<ConsOrdPagosDetDto> ConsultaOrdenesDePagoProveedorDetalle(string cmptId);
        List<ConsRecepcionProveedorDto> ConsultaRecepcionProveedor(string ctaId, DateTime fd, DateTime fh, string admId);
        List<ConsRecepcionProveedorDetalleDto> ConsultaRecepcionProveedorDetalle(string cmptId);
        List<ConsOrdPagoDetExtendDto> ConsultaOrdenDePagoProveedor(string op_compte);
        List<CertRetenGananDto> ConsultaCertRetenGA(string op_compte);
        List<CertRetenIBDto> ConsultaCertRetenIB(string op_compte);
        List<CertRetenIVADto> ConsultaCertRetenIVA(string op_compte);

	}
}
