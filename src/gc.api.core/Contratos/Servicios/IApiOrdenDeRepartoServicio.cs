using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.api.core.Contratos.Servicios
{
	public interface IApiOrdenDeRepartoServicio
	{
		List<OrdenDeRepartoEstadoDto> GetOrdenDeRepartoEstados();
		List<OrdenDeRepartoListaDto> ObtenerListaOrdenDeReparto(OrdenDeRepartoRequest req);
		List<PedidoEnOrdenDeRepartoDto> ObtenerPedidosEnOrdenDeReparto(string orCompte);
		RespuestaDto ConfirmarOrdenDeReparto(ConfirmaOrdenDeRepartoRequest req);
		List<AnalizarAutOrdenDeRepartoDto> AnalizarAutOrdenDeReparto(AnalizarAutOrdenDeRepartoRequest req);
		RespuestaDto APonerEnCursoOrdenDeReparto(APonerEnCursoOrdenDeRepartoRequest req);
	}
}
