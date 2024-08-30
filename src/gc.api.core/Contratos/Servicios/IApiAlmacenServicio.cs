using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Rpr;

namespace gc.api.core.Contratos.Servicios
{
    public interface IApiAlmacenServicio:IServicio<Producto>
    {
        RprResponseDto ValidarUL(string ul, string admid);
        RprResponseDto ValidarBox(string box, string admid);
        RprResponseDto AlmacenaBoxUl(RprABRequest req);
    }
}
