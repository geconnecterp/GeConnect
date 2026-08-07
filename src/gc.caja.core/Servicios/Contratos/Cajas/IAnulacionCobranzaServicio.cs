using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IAnulacionCobranzaServicio
    {
        Task<RespuestaGenerica<AnulacionCobranzaResponseDto>> BuscarCobranzas(AnulacionCobranzaBuscarRequestDto request, string token);
        Task<RespuestaGenerica<RespuestaDto>> AnularCobranza(AnulacionCobranzaConfirmarRequestDto request, string token);
    }
}

