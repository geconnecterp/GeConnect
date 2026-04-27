using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface IProductoFactServicio
    {
        Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatos(ProductoDatosRequestDto req, string token);
        //control de busqueda de productos en caja, se hace una busqueda general y luego se filtra por el id del producto
        Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token);

        Task<(List<ProductoListaDto>, MetadataGrid?)> BusquedaListaProductos(BusquedaProducto busqueda, string token);
        Task<CalculaFilasResDto> CalcularFilas(CalcularFilasReqDto req, string token);
    }
}
