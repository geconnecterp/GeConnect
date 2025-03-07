using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IConsultasServicio:IServicio<ConsultasDto>
    {
        Task<(List<ConsCtaCteDto>, MetadataGrid)> ConsultarCuentaCorriente(string ctaId, long fechaD, string userId, int pagina, int registros, string token);
        Task<RespuestaGenerica<ConsVtoDto>> ConsultaVencimientoComprobantesNoImputados(string ctaId, long fechaD, long fechaH, string userId, string token);
        Task<RespuestaGenerica<ConsCompTotDto>> ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId, string token);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctaId"></param>
        /// <param name="mes">el formato de este parametro es aaaamm</param>
        /// <param name="relCuit"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<RespuestaGenerica<ConsCompDetDto>> ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId, string token);
    }
}

