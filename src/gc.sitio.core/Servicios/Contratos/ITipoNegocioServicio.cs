using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoNegocioServicio : IServicio<TipoNegocioDto>
    {
        List<TipoNegocioDto> ObtenerTiposDeNegocio(string token);
    }
}
