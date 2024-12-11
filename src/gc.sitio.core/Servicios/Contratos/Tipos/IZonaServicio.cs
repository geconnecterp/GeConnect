using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IZonaServicio : IServicio<ZonaDto>
    {
        List<ZonaDto> GetZonaLista(string token);
    }
}
