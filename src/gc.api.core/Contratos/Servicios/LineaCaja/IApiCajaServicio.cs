using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiCajaServicio
    {
        RespuestaDto ValidaIntegridadUsuarioCaja(CajaReqDto req);
        RespuestaDto AperturaCaja(CajaReqDto reqDto);
        RespuestaDto CierreCaja(CajaReqDto reqDto);
        CuentaBusquedaResultadoDto BusquedaCaja_b_cuenta(string busqueda);
        ProductoDatosResponseDto ObtenerProductoDatos(ProductoDatosRequestDto req);
        RespuestaDto Cargar_CF(CargaCFRequestDto req);
        CajaDatosDto ObtenerDatosCF(string caja_id);
        RespuestaDto CierreCajaGral(string usu_id, string adm_id);
        RespuestaDto HabilitarCajaGral(string usu_id, string adm_id);
        List<CajaPVAbiertosDto> ObtenerPVAbiertos(string adm_id);

	}
}
