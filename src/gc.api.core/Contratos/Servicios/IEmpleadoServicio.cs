using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos;

namespace gc.api.core.Interfaces.Servicios
{
    public interface IEmpleadoServicio : IServicio<Persona>
    {
        Task<bool> Add(Persona empleado, string?usuario,string?rol,string?pass);
        new PagedList<EmpleadoDto> GetAll(QueryFilters filters);
        EmpleadoDto Find(Guid id);
        Task<bool> CambioClave(Guid id, CambioClaveDto cambio);
        Task<(bool, string)> VerificaSiEstaLogueado(string?userName,string?ip);
        void RegistrarAcceso(Guid usuarioId, string?ip, char tipoAcceso);
        Task Logoff(string?userName,string?ip);
    }
}
