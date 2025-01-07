using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
	public interface ITipoProveedorServicio : IServicio<TipoProveedor>
	{
		List<TipoProveedorDto> GetTiposProveedor();
	}
}
