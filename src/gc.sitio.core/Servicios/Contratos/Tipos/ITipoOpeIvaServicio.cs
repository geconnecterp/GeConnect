using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoOpeIvaServicio : IServicio<TipoOpeIvaDto>
	{
		List<TipoOpeIvaDto> ObtenerTipoOpeIva(string token);
	}
}
