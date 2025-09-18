using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiOfertaServicio
    {
        string ConocerEstadoOferta(string p_id,string admId,string lp_id);
        List<CanalDto> BuscarCanales();
        RespuestaDto ConfirmacionAltaOferta(AbmPlusGenDto req, ParamOferta param);
        List<OfertaEstadoDto> ObtenerEstadoOfertaProducto(string p_id);
        List<OfertaSinActivarDto> ObtenerOfertasSinActivar(string admId, string lp_id);
    }
}
