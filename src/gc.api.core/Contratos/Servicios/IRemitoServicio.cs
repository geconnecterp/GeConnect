using gc.api.core.Entidades;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;

namespace gc.api.core.Contratos.Servicios
{
    public interface IRemitoServicio : IServicio<Remito>
    {
        List<RemitoTransferidoDto> ObtenerRemitosTransferidos(string admId);
    }
}
