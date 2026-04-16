using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IProductoFactServicio 
    {
        Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatos(ProductoDatosRequestDto req, string token);
    }
}
