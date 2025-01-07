using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoRetGanServicio : IServicio<TipoRetGananciaDto>
	{
		List<TipoRetGananciaDto> ObtenerTipoRetGan(string token);
	}
}
