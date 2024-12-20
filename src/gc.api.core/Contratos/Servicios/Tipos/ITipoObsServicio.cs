using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface ITipoObsServicio : IServicio<TipoObs>
    {
        List<TipoObsDto> GetTiposDeObs(string tipo = "C");
    }
}
