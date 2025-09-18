using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IOfertaServicio
    {
        Task<RespuestaGenerica<string>> ConocerEstadoOferta(string p_id, string admId, string pl_id, string tokenCookie);
        Task<RespuestaGenerica<CanalDto>> BuscarCanales(string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmacionAltaOferta(AbmPlusGenDto req, string token);
        Task<RespuestaGenerica<OfertaEstadoDto>> ObtenerEstadoOfertaProducto(string p_id, string token);
        Task<RespuestaGenerica<OfertaSinActivarDto>> ObtenerOfertasSinActivar(string admId, string lp_id, string tokenCookie);
    }
}
