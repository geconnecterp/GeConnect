using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    public interface INotaCreditoServicio
    {
        Task<RespuestaGenerica<TipoComprobanteDto>> GetTipoComprobante(string afip_id, string opt_id, string token);

        Task<RespuestaGenerica<NCValidaResponseDto>> ValidarNC(NCValidaRequestDto request, string token);

        Task<RespuestaGenerica<NCProductoBuscarResponseDto>> BuscarProducto(NCProductoBuscarRequestDto request, string token);
    }


}
