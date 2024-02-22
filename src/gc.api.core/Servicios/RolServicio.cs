using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.api.core.Servicios;
using gc.api.Core.Interfaces.Servicios;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Security;

namespace gc.api.Core.Servicios
{
    public class RolServicio : Servicio<Role>, IRolServicio
    {
        readonly IUsuarioServicio _usuarioServicio;
        public RolServicio(IUnitOfWork uow, IUsuarioServicio usuarioServicio) : base(uow)
        {
            _usuarioServicio = usuarioServicio;
        }

        public void AddUsersToRoles(string[] usernames, string[] roleNames)
        {

            var huboInserts = false;
            foreach (var rol in roleNames)
            {
                var role = GetAllIq().FirstOrDefault(r => r.Nombre.Equals(rol));

                if (role != null)
                {
                    foreach (var user in usernames)
                    {
                        var usuario = _usuarioServicio.GetUsuarioByUserName(user);
                        if (usuario == null)
                        {
                            continue;
                        }
                        usuario.Autorizados.Add(new Autorizado { RoleId = role.Id, UsuarioId = usuario.Id });
                        huboInserts = true;
                    }
                }
            }


            if (huboInserts)
            {
                _uow.SaveChanges();
            }

        }

        public void CreateRole(string? roleName)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteRoleAsync(string? roleName, bool throwOnPopulatedRole)
        {
            var role = GetAllIq().Where(r => r.Nombre.Equals(roleName)).FirstOrDefault();
            var _autorRep = _uow.GetRepository<Autorizado>();
            if (role != null)
            {
                foreach (var auto in role.Autorizados)
                {
                    _autorRep.Remove(auto);
                }
                _repository.Remove(role);
                var res = await _uow.SaveChangesAsync();
                return res > 0;
            }
            throw new SecurityException("El Rol no se encuentra.");
        }

        public string?[] GetAllRoles()
        {
            return GetAllIq().Select(r => r.Nombre).ToArray();
        }

        public string?[] GetRolesForUser(string? username)
        {
            var _userRep = _uow.GetRepository<Usuario>();
            var usuario = _userRep.GetAll().Where(u => u.UserName.Equals(username)).Include(a => a.Autorizados).ThenInclude(a => a.Role).FirstOrDefault();
            if (usuario != null)
            {
                var roles = usuario.Autorizados.Select(a => a.Role.Nombre).ToArray();
                return roles;
            }
            throw new SecurityException("Usuario no encontrado");
        }

        public string[] GetUsersInRole(string? roleName)
        {
            throw new NotImplementedException();
        }

        public bool IsUserInRole(string? username, string? roleName)
        {
            throw new NotImplementedException();
        }

        public void RemoveUsersFromRoles(string[] userNames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public bool RoleExists(string? roleName)
        {
            throw new NotImplementedException();
        }
    }
}
