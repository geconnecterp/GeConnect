using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.api.core.Contratos.Servicios
{
    public interface IProductoServicio : IServicio<Producto>
    {
        ProductoBusquedaDto ProductoBuscar(BusquedaBase busqueda);
        List<ProductoListaDto> ProductoListaBuscar(BusquedaProducto search);

        List<InfoProdStkD> InfoProductoStkD(string id, string admId);
        List<InfoProdStkBox> InfoProductoStkBoxes(string id, string adm, string depo);
        List<InfoProdStkA> InfoProductoStkA(string id, string admId);
        List<InfoProdMovStk> InfoProductoMovStk(string id,string adm,string depo,string tmov,DateTime desde,DateTime hasta);
        List<InfoProdLP> InfoProductoLP(string id);
    }
}
