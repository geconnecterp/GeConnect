using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    /// <summary>
    /// Interfaz del servicio de Checkout (Proceso de Pago)
    /// Implementa las 4 fases del requerimiento de medios de pago
    /// </summary>
    public interface ICheckoutServicio
    {
        Task<RespuestaGenerica<ValoresPendientesResDto>> ObtenerValoresPendientes(ValoresPendientesReqDto req,string token);
        Task<RespuestaGenerica<ValoresNCResDto>> ObtenerValoresNC(ValoresNCReqDto req, string token);
        Task<RespuestaGenerica<ValoresMPResDto>> ObtenerValoresMP(ValoresMPReqDto req, string token);
        Task<RespuestaGenerica<ValoresInsResDto>> ObtenerValoresIns(ValoresInsReqDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> FinalizarCompra(CajaOpeConfirmarReq req, string token);
        
        //para combo de banco
        Task<RespuestaGenerica<ABMChequeListaDto>> GetBancoChequeLista(string token);

    }
}
