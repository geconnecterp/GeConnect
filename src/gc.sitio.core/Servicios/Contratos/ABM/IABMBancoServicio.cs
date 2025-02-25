using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.sitio.core.Servicios.Contratos.ABM
{
    public interface IABMBancoServicio : IServicio<ABMBancoSearchDto>
	{
		Task<(List<ABMBancoSearchDto>, MetadataGrid)> BuscarBancos(QueryFilters filters, string token);
	}
}
