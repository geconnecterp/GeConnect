using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;

namespace gc.sitio.core.Servicios.Contratos
{
	public interface IOrdenDeRepartoServicio
	{
		Task<RespuestaGenerica<OrdenDeRepartoEstadoDto>> ObtenerEstadosDeOrdenDeReparto(string token);
		Task<RespuestaGenerica<OrdenDeRepartoListaDto>> BuscarOrdenesDeReparto(QueryFilters filtro, string token);
		Task<RespuestaGenerica<PedidoEnOrdenDeRepartoDto>> ObtenerPedidosDeLaOrdenDeReparto(string orCompte, string token);
		Task<RespuestaGenerica<RespuestaDto>> ConfirmarOrdenDeReparto(ConfirmaOrdenDeRepartoRequest req, string token);
		Task<RespuestaGenerica<AnalizarAutOrdenDeRepartoDto>> AnalizarAutOrdenDeReparto(AnalizarAutOrdenDeRepartoRequest request, string token);
		Task<RespuestaGenerica<RespuestaDto>> APonerEnCursoOrdenDeReparto(APonerEnCursoOrdenDeRepartoRequest req, string token);
		Task<RespuestaGenerica<AConsolidarPedidoClienteDetalleDto>> AConsolidarPedidoClienteDetalle(AConsolidarPedidoClienteDetalleRequest request, string token);
		Task<RespuestaGenerica<AConsolidarConteosDto>> AConsolidarConteos(string orCompte, string token);
		Task<RespuestaGenerica<RespuestaDto>> AConsolidarOrdenDeReparto(AConciliarOrdenDeRepartoRequest req, string token);
		Task<RespuestaGenerica<CambioDePrecioDto>> CambioDePreciosLista(CambioDePrecioRequest request, string token);
		Task<RespuestaGenerica<RespuestaDto>> CambioDePreciosEnOrdenDeReparto(CambioDePrecioConfirmaRequest req, string token);
	}
}
