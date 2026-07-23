using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiProductoFactServicio
    {
        List<ProductoDatosResponseDto> ObtenerProductoDatos(ProductoDatosRequestDto req);
        CalculaFilasResDto CalcularFilas(CalcularFilasReqDto req);
        CalculaFilasResDto CalcularFila(CalcularFilasReqDto req);
        List<PrefacturaResDto> ObtenerPrefactura(PrefacturaReqDto req);
        List<CotizacionResDto> ObtenerCotizacion(CotizacionReqDto req);
        RespuestaDto CrearPrefacturaDiferida(CajaPrefDiferidaReqDto req);
        RespuestaDto CrearPagoDiferido(CajaOpeConfirmarReq req);

        List<FeResDto> ObtenerFE(FeReqDto req);
        List<FeDetResDto> ObtenerFEDetalle(FeReqDto req);
        List<FeIvaResDto> ObtenerFEIva(FeReqDto req);
        List<FePerResDto> ObtenerFEPer(FeReqDto req);
    }
}
