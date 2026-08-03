using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IRendicionServicio
    {
        Task<RespuestaGenerica<RendicionResponseDto>> CargarRendiciones(RendicionRequestDto request, string token);
        Task<RespuestaGenerica<RendicionNominalResponseDto>> CargarNominaciones(RendicionNominalRequestDto request, string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmarRendicion(RendicionCargaRequestDto request, string token);
    }
}
