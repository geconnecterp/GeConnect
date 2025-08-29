using gc.infraestructura.Dtos.Tipos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoTransferenciaServicio : IServicio<TipoTransferenciaDto>
	{
		List<TipoTransferenciaDto> GetTipoTransferenciaLista(string token);
	}
}
