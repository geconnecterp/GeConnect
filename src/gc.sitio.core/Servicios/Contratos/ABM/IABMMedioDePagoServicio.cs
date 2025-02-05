using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
	public interface IABMMedioDePagoServicio : IServicio<ABMMedioDePagoSearchDto>
	{
		Task<(List<ABMMedioDePagoSearchDto>, MetadataGrid)> BuscarMediosDePago(QueryFilters filters, string token);
	}
}
