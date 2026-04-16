using gc.infraestructura.Dtos.Cajas.Request;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiProductoFactServicio
    {
        List<ProductoDatosResponseDto> ObtenerProductoDatos(ProductoDatosRequestDto req);
    }
}
