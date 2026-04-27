using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiProductoFactServicio
    {
        List<ProductoDatosResponseDto> ObtenerProductoDatos(ProductoDatosRequestDto req);
        CalculaFilasResDto CalcularFilas(CalcularFilasReqDto req);
    }
}
