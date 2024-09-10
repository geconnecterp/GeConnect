using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProductoServicio : IServicio<ProductoDto>
    {
        Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token);
        Task<List<ProductoListaDto>> BusquedaListaProductos(BusquedaProducto search, string tokenCookie);
        Task<List<InfoProdStkD>> InfoProductoStkD(string id, string admId, string token);
        Task<List<InfoProdStkBox>> InfoProductoStkBoxes(string id, string adm, string depo, string token);
        Task<List<InfoProdStkA>> InfoProductoStkA(string id, string admId, string token);
        Task<List<InfoProdMovStk>> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta, string token);
        Task<List<InfoProdLP>> InfoProductoLP(string id, string token);
        Task<List<RPRAutorizacionPendienteDto>> RPRObtenerAutorizacionPendiente(string adm, string token);
        Task<RPRRegistroResponseDto> RPRRegistrarProductos(List<RPRProcuctoDto> json,string admId, string ul, string token);
        Task<List<RPRAutoComptesPendientesDto>> RPRObtenerComptesPendiente(string adm_id, string token);

        Task<RprResponseDto> ValidarUL(string ul,string adm,string token);
        Task<RprResponseDto> ValidarBox(string box,string adm,string token);
        Task<RprResponseDto> ConfirmaBoxUl(string box, string ul, string adm, string token);
        Task<List<RespuestaDto>> RPRCargarCompte(string json_str, string token);
        Task<List<RespuestaDto>> RPREliminarCompte(string rp, string token);
        Task<List<JsonDto>> RPObtenerJsonDesdeRP(string rp, string token);
        Task<List<RPRItemVerCompteDto>> RPRObtenerItemVerCompte(string rp, string token);
	}
}
