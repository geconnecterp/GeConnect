using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiOrdenDeRepartoServicio
	{
		List<OrdenDeRepartoEstadoDto> GetOrdenDeRepartoEstados();
		List<OrdenDeRepartoListaDto> ObtenerListaOrdenDeReparto(OrdenDeRepartoRequest req);
		List<PedidoEnOrdenDeRepartoDto> ObtenerPedidosEnOrdenDeReparto(string orCompte);
	}
}
