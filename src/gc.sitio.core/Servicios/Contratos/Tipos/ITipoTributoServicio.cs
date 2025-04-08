using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface ITipoTributoServicio : IServicio<TipoTributoDto>
	{
        List<TipoTributoDto> GetTiposTributoLista(string token);
    }
}
