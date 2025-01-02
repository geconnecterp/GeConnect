using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IProducto2Servicio
    {
        Task<RespuestaGenerica<BoxInfoDto>> ObtenerBoxInfo(string boxId, string token);
        Task<RespuestaGenerica<BoxInfoStkDto>> ObtenerBoxInfoStk(string box_id, string token);
        Task<RespuestaGenerica<BoxInfoMovStkDto>> ObtenerBoxInfoMovStk(string box_id, string sm_tipo, DateTime desde, DateTime hasta, string token);

        Task<RespuestaGenerica<RespuestaDto>> AJ_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token);
        Task<RespuestaGenerica<RespuestaDto>> DV_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token);

        Task<RespuestaGenerica<ConsULDto>> ConsultaUL(string tipo, DateTime fecD, DateTime fecH, string admId, string token);

        Task<RespuestaGenerica<MedidaDto>> ObtenerMedidas(string token);
        Task<RespuestaGenerica<IVASituacionDto>> ObtenerIVASituacion(string token);
        Task<RespuestaGenerica<IVAAlicuotaDto>> ObtenerIVAAlicuotas(string token);
        Task<RespuestaGenerica<ProductoBarradoDto>> ObtenerBarradoDeProd(string p_id,string token);
        Task<RespuestaGenerica<LimiteStkDto>> ObtenerLimiteStk(string p_id, string token);
        Task<RespuestaGenerica<ProductoBarradoDto>> ObtenerBarrado(string p_id, string barradoId, string tokenCookie);
    }
}
