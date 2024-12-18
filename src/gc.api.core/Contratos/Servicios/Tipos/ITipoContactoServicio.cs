using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
	public interface ITipoContactoServicio : IServicio<TipoContacto>
	{
		List<TipoContactoDto> GetTipoContactoLista(string tipo = "P");
	}
}
