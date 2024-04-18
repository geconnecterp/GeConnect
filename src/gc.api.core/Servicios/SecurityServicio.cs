using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using Microsoft.EntityFrameworkCore;

namespace gc.api.core.Servicios
{
    public class SecurityServicio : Servicio<Usuarios>, ISecurityServicio
    {
        public SecurityServicio(IUnitOfWork uow) : base(uow)
        {

        }

        public async Task<Usuarios?> GetLoginByCredential(UserLogin login)
        {
            return await GetAllIq().FirstOrDefaultAsync(u => u.UserName != null && u.UserName.Equals(login.UserName));
        }

        public async Task<bool> RegistrerUser(RegistroUserDto registro)
        {
            var _rolRep = _uow.GetRepository<Role>();
            Role? role = await _rolRep.GetAll()
                .FirstOrDefaultAsync(r => r.Nombre != null && r.Nombre.Equals(registro.Role));
            if (role == null)
            {
                throw new NotFoundException("El Rol que se pretende asignar no existe");
            }
            Usuarios user = new Usuarios
            {
                UserName = registro.User,
                Contrasena = registro.Password,
                Correo = registro.Correo,
                Id = Guid.NewGuid(),
                Bloqueado = false,
                Intentos = 0,
                FechaAlta = DateTime.Now
            };

            user.Autorizados.Add(new Autorizado
            {
                UsuarioId = user.Id,
                RoleId = role.Id,
                Role = role,
            });

            return await AddAsync(user);
        }
    }
}
