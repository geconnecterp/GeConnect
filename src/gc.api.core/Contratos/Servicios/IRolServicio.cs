using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;

namespace gc.api.core.Interfaces.Servicios
{
    public interface IRolServicio:IServicio<Role>
    {
        void AddUsersToRoles(string[] usernames, string[] roleNames);
        void CreateRole(string?roleName);
        Task<bool> DeleteRoleAsync(string?roleName, bool throwOnPopulatedRole);
        string[] GetAllRoles();
        string[] GetRolesForUser(string?username);
        bool IsUserInRole(string?username, string?roleName);
        bool RoleExists(string?roleName);
        string[] GetUsersInRole(string?roleName);
        void RemoveUsersFromRoles(string[] userNames, string[] roleNames);
    }
}
