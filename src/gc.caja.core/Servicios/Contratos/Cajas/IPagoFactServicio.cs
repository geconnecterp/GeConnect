using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IPagoFactServicio
    {
        Task<RespuestaGenerica<ValoresPendientesResDto>> ObtenerValoresPendientes(ValoresPendientesReqDto req,string token);
        Task<RespuestaGenerica<ValoresNCResDto>> ObtenerValoresNC(ValoresNCReqDto req, string token);
        Task<RespuestaGenerica<ValoresMPResDto>> ObtenerValoresMP(ValoresMPReqDto req, string token);
        Task<RespuestaGenerica<ValoresInsResDto>> ObtenerValoresIns(ValoresInsReqDto req, string token);
    }
}
