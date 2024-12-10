using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface ITiposDocumentoServicio : IServicio<TipoDocumento>
    {
        List<TipoDocumentoDto> GetTipoDocumentoLista();
    }
}
