using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.ABM;

namespace gc.api.core.Contratos.Servicios.ABM
{
    public interface IABMVendedorServicio:IServicio<Vendedor>
    {
        List<ABMVendedorDto> ObtenerVendedores(QueryFilters filters);
        List<ABMVendedorDatoDto> ObtenerVendedorPorId(string ve_id);
       
    }
}
