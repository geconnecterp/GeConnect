using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMCuentaDirectaServicio : IServicio<ABMCuentaDirectaSearchDto>
	{
		Task<(List<ABMCuentaDirectaSearchDto>, MetadataGrid)> BuscarCuentasDirectas(QueryFilters filters, string token);
	}
}
