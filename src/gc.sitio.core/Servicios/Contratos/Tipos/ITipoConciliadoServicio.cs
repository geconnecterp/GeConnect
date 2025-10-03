using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoConciliadoServicio : IServicio<TipoConciliadoDto>
	{
		List<TipoConciliadoDto> GetTipoConciliadoLista(string token);
	}
}
