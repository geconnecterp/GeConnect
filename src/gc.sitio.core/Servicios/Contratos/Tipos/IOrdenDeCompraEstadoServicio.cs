using gc.infraestructura.Dtos;

namespace gc.sitio.core.Servicios.Contratos
{
    public interface IOrdenDeCompraEstadoServicio : IServicio<OrdenDeCompraEstadoDto>
    {
		List<OrdenDeCompraEstadoDto> GetOrdenDeCompraEstadoLista(string token);
	}
}
