using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios
{
    public interface IApiAlmacenServicio:IServicio<Producto>
    {
        RespuestaDto ValidarUL(string ul, string admid, string sm);
        RprResponseDto ValidarBox(string box, string admid);
        RprResponseDto AlmacenaBoxUl(RprABRequest req);
        List<AutorizacionTIDto> TRObtenerAutorizacionesPendientes(string admId,string usuId,string titId);
        List<BoxRubProductoDto> TIObtenerListaBox(string admId, string usuId, string ti);
        List<BoxRubProductoDto> TIObtenerListaRubro(string admId, string usuId, string ti);
        List<TiListaProductoDto> BuscaTIListaProductos(string admId, string usuId, string ti, string boxid, string rubroid);
        List<DepositoInfoBoxDto> ObtenerListaDeBoxesPorDeposito(string depoId);

	}
}
