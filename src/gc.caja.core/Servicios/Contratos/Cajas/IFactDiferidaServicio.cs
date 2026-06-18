using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IFactDiferidaServicio
    {
        Task<RespuestaGenerica<FactPendienteResponseDto>> ObtenerFacturasPendientes(FactPendienteRequestDto req,string token);
    }
}
