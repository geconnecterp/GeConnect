using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoObsServicio : IServicio<TipoObsDto>
    {
        List<TipoObsDto> GetTiposDeObs(string token, string tipo = "C");
    }
}
