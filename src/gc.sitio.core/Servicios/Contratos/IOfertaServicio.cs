using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IOfertaServicio
    {
        Task<RespuestaGenerica<string>> ConocerEstadoOferta(string p_id, string admId, string pl_id, string tokenCookie);
        Task<RespuestaGenerica<CanalDto>> BuscarCanales(string token);
        Task<RespuestaGenerica<TipoOfertaDto>> BuscarTiposOferta(string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmacionAltaOferta(AbmPlusGenDto req, string token);
        Task<RespuestaGenerica<OfertaEstadoDto>> ObtenerEstadoOfertaProducto(string p_id, string token);
        Task<RespuestaGenerica<OfertaDto>> ObtenerOfertasSinActivar(string admId, string lp_id, string tokenCookie);
        Task<RespuestaGenerica<RespuestaDto>> ActivacionDeOferta(AbmPlusGenDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> ActualizarOfertaVencidaSinActivar(AbmGenDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> CargarActivasASinActivar(AbmGenDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> EliminarOfertas(AbmPlusGenDto req, string token);
        Task<RespuestaGenerica<OfertaDto>> ObtenerOfertasActivas(string admId, string lp_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> EliminaOfertasActivas(AbmGenDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> CopiarACanal(AbmPlusGenDto req, string token);
    }
}
