using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoDocumentoServicio:IServicio<TipoDocumentoDto>
    {
        List<TipoDocumentoDto> GetTipoDocumentoLista(string token);
    }
}
