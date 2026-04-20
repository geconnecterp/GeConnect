using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface ICajaServicio
    {
        Task<CajaSettings> ObtenerAsync(string ruta);
        Task<RespuestaGenerica<RespuestaDto>> ValidarIntegridadUsuarioCaja(CajaReqDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> AperturaCaja(CajaReqDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> CierreCaja(CajaReqDto req, string token);
        Task<RespuestaGenerica<CuentaBusquedaResultadoDto>> BusquedaClientes(string busqueda, string adm_id, string usu_id, string token);
        Task<RespuestaGenerica<CuentaDatosResultadoDto>> BusquedaDatosCliente(string origen, string valor, string adm_id, string usu_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> ConfirmaConsumidorFinal(ClienteRequestDto req, string token);
        Task<RespuestaGenerica<CajaDatosDto>> ObtenerDatosCF(string caja_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token);
    }
}
