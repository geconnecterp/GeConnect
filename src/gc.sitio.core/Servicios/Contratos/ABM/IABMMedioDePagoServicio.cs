using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
	public interface IABMMedioDePagoServicio : IServicio<ABMMedioDePagoSearchDto>
	{
		Task<(List<ABMMedioDePagoSearchDto>, MetadataGrid)> BuscarMediosDePago(QueryFilters filters, string token);
		Task<RespuestaGenerica<MedioDePagoListaDto>> ObtenerMediosDePagoLista(string tcf_id, string token);
	}
}
