using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoDtoValorizaRprServicio : IServicio<TipoDtoValorizaRprDto>
	{
		List<TipoDtoValorizaRprDto> ObtenerTipoDtoValorizaRpr(string token);
	}
}
