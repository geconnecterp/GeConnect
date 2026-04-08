using gc.infraestructura.Dtos.Cajas;
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
        Task<RespuestaGenerica<CuentaBusquedaResultadoDto>> BusquedaCaja_b_cuenta(string busqueda, string token);
        Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatos(ProductoDatosRequestDto req, string token);
        Task<RespuestaGenerica<RespuestaDto>> Cargar_CF(CargaCFRequestDto req, string token);
        Task<RespuestaGenerica<CajaDatosDto>> ObtenerDatosCF(string caja_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> CierreCajaGral(string usu_id, string adm_id, string token);
        Task<RespuestaGenerica<RespuestaDto>> HabilitarCajaGral(string usu_id, string adm_id, string token);
    }
}
