using gc.infraestructura.Dtos.Productos.Ofertas;

namespace gc.api.core.Contratos.Servicios.Ofertas
{
    public interface IApiOfertaServicio
    {
        string ConocerEstadoOferta(string p_id,string admId,string lp_id);
        List<CanalDto> BuscarCanales();
    }
}
