using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ICuentaServicio: IServicio<CuentaDto>
    {
        List<ProveedorListaDto> ObtenerListaProveedores(string token);
        Task<List<CuentaDto>> ObtenerListaCuentaComercial(string texto, char tipo, string token);
        Task<List<RPROrdenDeCompraDto>> ObtenerListaOCxCuenta(string cta_id, string token);
        Task<List<RPROrdenDeCompraDetalleDto>> ObtenerDetalleDeOC(string oc_compte, string token);
	}
}
