using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface ITipoProveedorServicio : IServicio<TipoProveedorDto>
	{
		List<TipoProveedorDto> ObtenerTiposProveedor(string token);
	}
}
