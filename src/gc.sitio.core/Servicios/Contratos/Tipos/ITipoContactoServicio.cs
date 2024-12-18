using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoContactoServicio : IServicio<TipoContactoDto>
	{
		List<TipoContactoDto> GetTipoContactoLista(string token, string tipo = "P");
	}
}
