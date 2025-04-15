using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios
{
    public interface IZonaServicio : IServicio<Zona>
    {
        List<ZonaDto> GetZonaLista();
        List<ABMZonaDto> ObtenerZonas(QueryFilters filters);
        List<ZonaDto> ObtenerZonaPorId(string zn_id);
    }
}
