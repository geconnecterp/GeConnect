using gc.infraestructura.Dtos.Inventario;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IInventarioServicio : IServicio<InventarioDto>
	{
		List<InventarioDto> GetInventarioLista(GetInventarioListaRequest request, string token);
	}
}
