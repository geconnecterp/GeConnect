using gc.api.core.Entidades;

namespace gc.api.core.Contratos.Servicios
{
    public interface IProductoDepositoServicio : IServicio<ProductoDeposito>
    {
        ProductoDeposito ObtenerFechaVencimiento(string pId, string bId);
    }
}
