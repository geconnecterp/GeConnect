using gc.api.core.Constantes;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Datos;
using gc.api.core.Interfaces.Servicios;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.Dtos.Users;
using Microsoft.Data.SqlClient;
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

        public Usuario? GetLoginByCredential(UserLogin login,bool esUp = false)
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

        public PoliticaClaveDto ObtenerPoliticaClave()
        {
            var respuesta = _repository.EjecutarLstSpExt<PoliticaClaveDto>(
                "SPGECO_SEG_Configuracion_Obtener", [], true);

            if (respuesta == null || respuesta.Count == 0)
            {
                throw new NegocioException("No se encontró la configuración de seguridad.");
            }

            return respuesta[0];
        }

        public CambioClaveResultadoDto CambiarClave(string usuId, string claveActual, string claveNueva,
            string? admId, string? ip, Guid operacionId)
        {
            var parametros = new List<SqlParameter>
            {
                new("@usu_id", usuId),
                new("@clave_actual", claveActual),
                new("@clave_nueva", claveNueva),
                new("@adm_id", (object?)admId ?? DBNull.Value),
                new("@ip", (object?)ip ?? DBNull.Value),
                new("@origen", "GC.SITIO"),
                new("@operacion_id", operacionId)
            };

            var respuesta = _repository.EjecutarLstSpExt<CambioClaveResultadoDto>(
                "SPGECO_USU_Clave_Cambiar", parametros, true);

            return respuesta?.FirstOrDefault() ?? new CambioClaveResultadoDto
            {
                resultado = -1,
                resultado_id = "SIN_RESPUESTA",
                resultado_msj = "No se obtuvo respuesta al intentar modificar la contraseña.",
                OperacionId = operacionId
            };
        }

        public EstadoSeguridadUsuarioDto ObtenerEstadoSeguridad(string usuId)
        {
            var respuesta = _repository.EjecutarLstSpExt<EstadoSeguridadUsuarioDto>(
                "SPGECO_USU_Seguridad_Estado",
                [new SqlParameter("@usu_id", usuId)], true);

            return respuesta?.FirstOrDefault() ?? new EstadoSeguridadUsuarioDto();
        }

        public OperacionesSeguridadUsuarioDto ObtenerOperacionesSeguridad(string usuId)
        {
            var politica = ObtenerPoliticaClave();
            var derechos = _repository.EjecutarLstSpExt<DerUserDto>(
                ConstantesGC.StoredProcedures.SP_USU_DER,
                [new SqlParameter("@usu_id", usuId)], true) ?? [];

            bool Posee(string? codigo) => !string.IsNullOrWhiteSpace(codigo) && derechos.Any(x =>
                x.asignado && string.Equals(x.der_codigo?.Trim(), codigo.Trim(), StringComparison.OrdinalIgnoreCase));

            return new OperacionesSeguridadUsuarioDto
            {
                PuedeBlanquearClave = Posee(politica.DerechoBlanquearClave),
                PuedeDesbloquearUsuario = Posee(politica.DerechoDesbloquearUsuario)
            };
        }

        public CambioClaveResultadoDto BlanquearClave(string usuarioObjetivo, string usuarioEjecutor,
            string claveTemporal, string? admId, string? ip, Guid operacionId)
        {
            return EjecutarOperacionUsuario("SPGECO_USU_Clave_Blanquear",
            [
                new("@usu_id_objetivo", usuarioObjetivo),
                new("@usu_id_ejecutor", usuarioEjecutor),
                new("@clave_temporal", claveTemporal),
                new("@adm_id", (object?)admId ?? DBNull.Value),
                new("@ip", (object?)ip ?? DBNull.Value),
                new("@origen", "GC.SITIO"),
                new("@operacion_id", operacionId)
            ], operacionId);
        }

        public CambioClaveResultadoDto CambiarClaveForzada(string usuId, string claveNueva,
            string? admId, string? ip, Guid operacionId)
        {
            return EjecutarOperacionUsuario("SPGECO_USU_Clave_Forzada_Cambiar",
            [
                new("@usu_id", usuId),
                new("@clave_nueva", claveNueva),
                new("@adm_id", (object?)admId ?? DBNull.Value),
                new("@ip", (object?)ip ?? DBNull.Value),
                new("@origen", "GC.SITIO"),
                new("@operacion_id", operacionId)
            ], operacionId);
        }

        public CambioClaveResultadoDto DesbloquearUsuario(string usuarioObjetivo, string usuarioEjecutor,
            string? admId, string? ip, Guid operacionId)
        {
            return EjecutarOperacionUsuario("SPGECO_USU_Desbloquear",
            [
                new("@usu_id_objetivo", usuarioObjetivo),
                new("@usu_id_ejecutor", usuarioEjecutor),
                new("@adm_id", (object?)admId ?? DBNull.Value),
                new("@ip", (object?)ip ?? DBNull.Value),
                new("@origen", "GC.SITIO"),
                new("@operacion_id", operacionId)
            ], operacionId);
        }

        private CambioClaveResultadoDto EjecutarOperacionUsuario(string sp,
            List<SqlParameter> parametros, Guid operacionId)
        {
            var respuesta = _repository.EjecutarLstSpExt<CambioClaveResultadoDto>(sp, parametros, true);
            return respuesta?.FirstOrDefault() ?? new CambioClaveResultadoDto
            {
                resultado = -1,
                resultado_id = "SIN_RESPUESTA",
                resultado_msj = "No se obtuvo respuesta al procesar la operación de seguridad.",
                OperacionId = operacionId
            };
        }
    }
}
