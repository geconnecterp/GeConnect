using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Discontinuo;

namespace gc.api.core.Contratos.Servicios
{
    public interface IApiDiscontinuoServicio
    {
        List<DiscontinuoDetalleDto> ObtenerProductosDiscontinuos(QueryFilters filters);
       
        RespuestaDto ConfirmarDiscontinuo(AbmGenDto req);
    }
}
