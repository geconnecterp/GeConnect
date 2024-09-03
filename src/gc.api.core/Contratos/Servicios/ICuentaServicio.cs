using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;

namespace gc.api.core.Contratos.Servicios
{
    public interface ICuentaServicio : IServicio<Cuenta>
    {
        List<ProveedorListaDto> GetProveedorLista();
        List<CuentaDto> GetCuentaComercialLista(string texto, char tipo);
        List<RPROrdenDeCompraDto> GetOCporCuenta(string cta_id);
        List<RPROrdenDeCompraDetalleDto> GetDetalleDeOC(string oc_compte);
	}
}
