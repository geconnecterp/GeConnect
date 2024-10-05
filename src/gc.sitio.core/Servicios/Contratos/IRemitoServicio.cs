using gc.infraestructura.Dtos.Almacen.Tr.Remito;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IRemitoServicio : IServicio<RemitoDto>
    {
        Task<List<RemitoTransferidoDto>> ObtenerRemitosTransferidos(string admId, string token);
    }
}
