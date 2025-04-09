using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios.ABM
{
    public interface IABMRepartidorServicio:IServicio<Vendedor>
    {
        List<ABMRepartidorDto> ObtenerRepartidores(QueryFilters filters);
        List<ABMRepartidorDatoDto> ObtenerRepartidorPorId(string rp_id);
       
    }
}
