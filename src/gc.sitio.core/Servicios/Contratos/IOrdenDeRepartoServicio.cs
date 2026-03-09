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
	}
}
