using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoCanalServicio : IServicio<TipoCanalDto>
	{
		List<TipoCanalDto> GetTipoCanalLista(string token);
	}
}
