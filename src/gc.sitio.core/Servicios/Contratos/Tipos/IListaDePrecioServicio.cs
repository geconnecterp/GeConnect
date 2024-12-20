using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IListaDePrecioServicio : IServicio<ListaPrecioDto>
	{
		List<ListaPrecioDto> GetListaPrecio(string token);
	}
}
