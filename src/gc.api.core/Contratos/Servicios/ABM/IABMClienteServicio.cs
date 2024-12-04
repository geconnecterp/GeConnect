using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios.ABM
{
    public interface IABMClienteServicio : IServicio<Cuenta>
    {
        List<ABMClienteSearchDto> Buscar(QueryFilters filtro);
    }
}
