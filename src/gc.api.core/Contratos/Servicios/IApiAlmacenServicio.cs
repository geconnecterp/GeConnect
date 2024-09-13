using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;

namespace gc.api.core.Contratos.Servicios
{
    public interface IApiAlmacenServicio:IServicio<Producto>
    {
        RprResponseDto ValidarUL(string ul, string admid);
        RprResponseDto ValidarBox(string box, string admid);
        RprResponseDto AlmacenaBoxUl(RprABRequest req);
        List<AutorizacionTIDto> TRObtenerAutorizacionesPendientes(string admId,string usuId,string titId);
        List<BoxRubProductoDto> TIObtenerListaBox(string admId, string usuId, string ti);
        List<BoxRubProductoDto> TIObtenerListaRubro(string admId, string usuId, string ti);
        List<TiListaProductoDto> BuscaTIListaProductos(string admId, string usuId, string ti, string boxid, string rubroid);
    }
}
