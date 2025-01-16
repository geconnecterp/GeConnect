using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;

namespace gc.sitio.core.Servicios.Contratos.Users
{
    public interface IMenuesServicio:IServicio<PerfilDto>
    {
        Task<(List<PerfilDto>, MetadataGrid)> GetPerfiles(QueryFilters filters,string token);
        Task<RespuestaGenerica<PerfilDto>> GetPerfil(string id,string token);
        Task<RespuestaGenerica<PerfilUserDto>> GetPerfilUsers(string perfilId, string token);
    }
}
