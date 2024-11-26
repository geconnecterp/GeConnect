using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProducto2Servicio
    {
        Task<RespuestaGenerica<BoxInfoDto>> ObtenerBoxInfo(string boxId, string token);
        Task<RespuestaGenerica<BoxInfoStkDto>> ObtenerBoxInfoStk(string box_id, string token);
        Task<RespuestaGenerica<BoxInfoMovStkDto>> ObtenerBoxInfoMovStk(string box_id, string sm_tipo, DateTime desde, DateTime hasta, string token);

        Task<RespuestaGenerica<RespuestaDto>> AJ_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token);
        Task<RespuestaGenerica<RespuestaDto>> DV_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token);
    }
}
