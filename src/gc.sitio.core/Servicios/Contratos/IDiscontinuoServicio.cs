using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Discontinuo;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IDiscontinuoServicio
    {
        Task<RespuestaGenerica<DiscontinuoDetalleDto>> ObtenerProductosDiscontinuos(QueryFilters filters,string token);

        Task<RespuestaGenerica<RespuestaDto>> ConfirmarDiscontinuo(AbmGenDto req,string token);
    }
}
