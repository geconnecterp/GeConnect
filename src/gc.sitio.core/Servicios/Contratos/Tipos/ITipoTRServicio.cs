using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoTRServicio : IServicio<TRTipoDto>
	{
		List<TRTipoDto> GetTiposTRLista(string token);
	}
}
