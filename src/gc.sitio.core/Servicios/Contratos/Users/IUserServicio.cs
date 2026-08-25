using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Users.Request;
using gc.infraestructura.Dtos.Seguridad;

namespace gc.sitio.core.Servicios.Contratos.Users
{
    public interface IUserServicio:IServicio<UserDto>
    {
        Task<(List<UserDto>, MetadataGrid)> BuscarUsuarios(QueryFilters filtro, string token);
        Task<RespuestaGenerica<UserDto>> BuscarUsuarioDatos(string usuId,string token);
        Task<RespuestaGenerica<PerfilUserDto>> ObtenerPerfilesDelUsuario(string usuId, string token);
        Task<RespuestaGenerica<AdmUserDto>> ObtenerAdministracionesDelUsuario(string usuId, string token);
        Task<RespuestaGenerica<DerUserDto>> ObtenerDerechosDelUsuario(string usuId, string token);
        List<UserDto> ObtenerUsuarioParaLista(BuscarUsuarioRequest request, string token);
        Task<RespuestaGenerica<UserDto>> BuscarUsuarioLista(string admId, string token);
        Task<OperacionesSeguridadUsuarioDto> ObtenerOperacionesSeguridad(string token);
        Task<CambioClaveResultadoDto> BlanquearClave(string usuarioObjetivo, string token, string? ip);
        Task<CambioClaveResultadoDto> DesbloquearUsuario(string usuarioObjetivo, string token, string? ip);

	}
}
