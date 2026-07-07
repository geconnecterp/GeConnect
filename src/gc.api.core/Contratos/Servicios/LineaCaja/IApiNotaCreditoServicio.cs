using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.api.core.Contratos.Servicios.LineaCaja
{
    public interface IApiNotaCreditoServicio
    {
        List<NCValidaResponseDto> ValidarNC(NCValidaRequestDto request);
        List<NCProductoBuscarResponseDto> BuscarProducto(NCProductoBuscarRequestDto request);
    }
}
