using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiAnulacionServicio
    {
        List<AnulacionCobranzaResponseDto> BuscarCobranzas(AnulacionCobranzaBuscarRequestDto request);
        RespuestaDto AnularCobranza(AnulacionCobranzaConfirmarRequestDto request);
    }
}
