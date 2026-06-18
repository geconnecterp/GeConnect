using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiPagoFactServicio
    {
        List<ValoresPendientesResDto> ObtenerValoresPendientes(ValoresPendientesReqDto req);
        List<ValoresNCResDto> ObtenerValoresNC(ValoresNCReqDto req);
        List<ValoresMPResDto> ObtenerValoresMP(ValoresMPReqDto req);
        List<ValoresInsResDto> ObtenerValoresIns(ValoresInsReqDto req);
        RespuestaDto ConfirmarOperacionCaja(CajaOpeConfirmarReq req);

        List<FactPendienteResponseDto> ObtenerFacturasPendientes(FactPendienteRequestDto req);
    }
}
