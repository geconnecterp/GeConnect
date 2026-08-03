using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiRendicionServicio
    {
        List<RendicionResponseDto> ObtenerRendiciones(RendicionRequestDto request);
        List<RendicionNominalResponseDto> ObtenerNominaciones(RendicionNominalRequestDto request);
        RespuestaDto ConfirmarRendicion(RendicionCargaRequestDto request);
    }
}
