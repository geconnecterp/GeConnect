using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;

namespace gc.sitio.core.Servicios.Contratos.Users
{
    public interface IUserServicio:IServicio<UserDto>
    {
        Task<(List<UserDto>, MetadataGrid)> BuscarUsuarios(QueryFilters filtro, string token);
        Task<RespuestaGenerica<UserDto>> BuscarUsuarioDatos(string usuId,string token);
        Task<RespuestaGenerica<PerfilUserDto>> ObtenerPerfilesDelUsuario(string usuId, string token);
        Task<RespuestaGenerica<AdmUserDto>> ObtenerAdministracionesDelUsuario(string usuId, string token);
        Task<RespuestaGenerica<DerUserDto>> ObtenerDerechosDelUsuario(string usuId, string token);
    }
}
