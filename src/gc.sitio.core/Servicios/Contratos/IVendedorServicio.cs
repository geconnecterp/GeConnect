using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IVendedorServicio : IServicio<VendedorDto>
	{
		List<VendedorDto> GetVendedorLista(string token);
	}
}
