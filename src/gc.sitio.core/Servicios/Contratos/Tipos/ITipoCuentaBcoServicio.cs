using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoCuentaBcoServicio : IServicio<TipoCuentaBcoDto>
	{
		List<TipoCuentaBcoDto> GetTipoCuentaBcoLista(string token);
	}
}
