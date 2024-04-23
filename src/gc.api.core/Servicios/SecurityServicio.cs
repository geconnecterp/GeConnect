using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class SecurityServicio : Servicio<Usuarios>, ISecurityServicio
    {
        private readonly UsuarioSettings _settings;
        public SecurityServicio(IUnitOfWork uow, IOptions<UsuarioSettings> options) : base(uow)
        {
            _settings = options.Value;
        }

        public async Task<Usuarios?> GetLoginByCredential(UserLogin login)
        {
            return await GetAllIq().FirstOrDefaultAsync(u => u.usu_id != null && u.usu_id.Equals(login.UserName));
        }

        public async Task<bool> RegistrerUser(Usuarios registro)
        {
            if (registro == null)
            {
                throw new NegocioException("No se encontraron los datos para el registro");
            }

            if (string.IsNullOrEmpty(registro.usu_apellidoynombre))
            {
                throw new NegocioException("No se ha especificado el Apellido y Nombre");
            }

            if (string.IsNullOrEmpty(registro.usu_email))
            {
                throw new NegocioException("No se ha especificado el email");
            }
            var val = HelperGen.ValidarCorreoElectronico(registro.usu_email);
            if (!val)
            {
                throw new NegocioException("El correo ingresado, no es válido.");
            }



            registro.usu_alta = DateTime.Now;
            registro.usu_bloqueado = false;
            registro.usu_expira = true;
            registro.usu_pin = string.Empty;

            //

            _repository.Add(registro);

            _uow.SaveChanges();
            

            //var _rolRep = _uow.GetRepository<Role>();
            //Role? role = await _rolRep.GetAll()
            //    .FirstOrDefaultAsync(r => r.Nombre != null && r.Nombre.Equals(registro.Role));
            //if (role == null)
            //{
            //    throw new NotFoundException("El Rol que se pretende asignar no existe");
            //}
            //Usuarios user = new Usuarios
            //{
            //    UserName = registro.User,
            //    Contrasena = registro.Password,
            //    Correo = registro.Correo,
            //    Id = Guid.NewGuid(),
            //    Bloqueado = false,
            //    Intentos = 0,
            //    FechaAlta = DateTime.Now
            //};

            //user.Autorizados.Add(new Autorizado
            //{
            //    UsuarioId = user.Id,
            //    RoleId = role.Id,
            //    Role = role,
            //});

            //return await AddAsync(user);
            return true;
        }
    }
}
