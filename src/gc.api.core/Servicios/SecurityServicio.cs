using Azure.Core;
using gc.api.core.Constantes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace gc.api.core.Servicios
{
    public class SecurityServicio : Servicio<Usuario>, ISecurityServicio
    {
        private readonly UsuarioSettings _settings;
        public SecurityServicio(IUnitOfWork uow, IOptions<UsuarioSettings> options) : base(uow)
        {
            _settings = options.Value;
        }

        public async Task<Usuario?> GetLoginByCredential(UserLogin login,bool esUp = false)
        {
            var sp = ConstantesGC.StoredProcedures.SP_USU_X_IDYADM;
            var ps = new List<SqlParameter>()
            {
                    new("@usu_id",login.UserName),
                    new("@adm_id",login.Admid),
                    new("@sinAdm",esUp),
            };
            var usuario = _repository.EjecutarLstSpExt<Usuario>(sp, ps, true);
            return usuario.FirstOrDefault();

            //if (esUp) {
            //    var regs = GetAllIq();
            //    return await regs
            //        .FirstOrDefaultAsync(u => u.Usu_id != null && u.Usu_id.Equals(login.UserName));
            //}
            //else {
            //    var user =  await GetAllIq()
            //        .FirstOrDefaultAsync(u => u.Usu_id != null && u.Usu_id.Equals(login.UserName) && u.UsuarioAdministraciones.Any(a => a.Adm_Id.Equals(login.Admid)));

                
            //}
            
        }

        public async Task<bool> RegistrerUser(Usuario registro,bool esUp=false)
        {
            if (registro == null)
            {
                throw new NegocioException("No se encontraron los datos para el registro");
            }

            if (string.IsNullOrEmpty(registro.Usu_apellidoynombre))
            {
                throw new NegocioException("No se ha especificado el Apellido y Nombre");
            }

            if (string.IsNullOrEmpty(registro.Usu_email))
            {
                throw new NegocioException("No se ha especificado el email");
            }
            var val = HelperGen.ValidarCorreoElectronico(registro.Usu_email);
            if (!val)
            {
                throw new NegocioException("El correo ingresado, no es válido.");
            }
            if (!esUp)            
            {
                registro.Usu_alta = DateTime.Now;
            }
            registro.Usu_bloqueado = false;
            registro.Usu_expira = true;
            registro.Usu_pin = string.Empty;

            if (esUp) { 
                _repository.Update(registro);
            } else
            {
                await _repository.AddAsync(registro);
            }

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
