using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProducto2Servicio
    {
        Task<RespuestaGenerica<BoxInfoDto>> ObtenerBoxInfo(string boxId, string token);
        Task<RespuestaGenerica<BoxInfoStkDto>> ObtenerBoxInfoStk(string box_id, string token);
        Task<RespuestaGenerica<BoxInfoMovStkDto>> ObtenerBoxInfoMovStk(string box_id, string sm_tipo, DateTime desde, DateTime hasta, string token);
    }
}
