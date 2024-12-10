using gc.api.core.Entidades;
using gc.infraestructura.Dtos;

namespace gc.api.core.Contratos.Servicios
{
    public interface IDepartamentoServicio : IServicio<Departamento>
    {
        List<DepartamentoDto> GetDepartamentoPorProvinciaLista(string prov_id);
    }
}
