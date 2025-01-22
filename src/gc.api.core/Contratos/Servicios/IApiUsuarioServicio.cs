using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Users;
namespace gc.api.core.Contratos.Servicios
{
    public interface IApiUsuarioServicio : IServicio<Usuario>
    {
        Usuario GetUsuarioByUserName(string userName);
        List<PerfilDto> GetPerfiles(QueryFilters filters);
        PerfilDto GetPerfil(string id);
        List<PerfilUserDto> GetPerfilUsers(string perfilId);
        List<MenuDto> GetMenu();
        List<MenuItemsDto> GetMenuItems(string menuId, string perfil);
    }
}
