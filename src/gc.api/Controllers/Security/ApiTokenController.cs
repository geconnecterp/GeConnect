using gc.api.Controllers.Base;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.Core.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace gc.api.Controllers.Security
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    public class ApiTokenController : ApiControladorBase
    {
        private readonly IOptions<ConfigNegocioOption> _options;
        private readonly IConfiguration _configuration;
        private readonly ISecurityServicio _securityServicio;
        private readonly IPasswordService _passwordService;
        private readonly IAdministracionServicio _adminServicio;
        private readonly IApiUsuarioServicio _usuSv;
        private readonly ILogger<ApiTokenController> _logger;

        public ApiTokenController(IOptions<ConfigNegocioOption> options, IConfiguration configuration,
            ISecurityServicio securityServicio, IAdministracionServicio adminServicio,
            IPasswordService passwordService, ILogger<ApiTokenController> logger, IApiUsuarioServicio usuSv)
        {
            _options = options;
            _configuration = configuration;
            _securityServicio = securityServicio;
            _passwordService = passwordService;
            _adminServicio = adminServicio;
            _logger = logger;
            _usuSv = usuSv;
        }

        [HttpPost]
        public IActionResult Authentication(UserLogin login)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
            //string ip = ObtenerIPRemota(HttpContext);
            ////se generara un usuario y lo vamos a validar a modo de prueba. 
            ////Si el usuario fuera valido se deberia generar el token
            var validation = IsValidUser(login);
            if (string.IsNullOrEmpty(login.Admid))
            {
                return BadRequest("La sucursal no es valida");
            }
            if (validation.Item1)
            {   //el usuario es valido. Verificamos si esta logueado o no.
                var usuarioValidado = validation.Item2 ?? new Usuario();
                var estadoSeguridad = _securityServicio.ObtenerEstadoSeguridad(usuarioValidado.Usu_id);
                if (estadoSeguridad.CambioClaveObligatorio && estadoSeguridad.ClaveTemporalVencida)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        codigo = "CLAVE_TEMPORAL_VENCIDA",
                        mensaje = "La contraseña temporal ha vencido. Solicite un nuevo blanqueo al administrador."
                    });
                }

                var administraciones = _adminServicio.ObtenerAdministraciones("S");

                var adm = administraciones.FirstOrDefault(x => x.Adm_id.Equals(login.Admid));

                if (adm == null)
                {
                    return NotFound();
                }

                //Obtener los perfiles de acceso del usuario.
                List<PerfilUserDto> perfiles = _usuSv.GetUserPerfiles(login.UserName);
                var token = GenerateToken(usuarioValidado, adm, perfiles, estadoSeguridad);
                return Ok(new { token });
            }
            return NotFound();
        }

        [HttpGet]
        [Route("[action]")]
        public IActionResult Logoff(string userName)
        {
            //ApiResponse<bool> ret;
            //string ip = ObtenerIPRemota(HttpContext);
            //await _empleadoServicio.Logoff(userName,ip);
            //ret = new ApiResponse<bool>(true);
            //return Ok(ret);
            throw new NotImplementedException();
        }

        [HttpGet("politica-clave"), Authorize]
        public IActionResult ObtenerPoliticaClave()
        {
            var politica = _securityServicio.ObtenerPoliticaClave();
            return Ok(new ApiResponse<PoliticaClaveDto>(politica));
        }

        [HttpPost("cambio-clave"), Authorize]
        public IActionResult CambioClave(CambioClaveRequestDto cambio)
        {
            _logger.LogInformation("{Controller} - {Action}", GetType().Name, MethodBase.GetCurrentMethod()?.Name);

            if (cambio == null || string.IsNullOrWhiteSpace(cambio.ClaveActual))
                return BadRequest("Debe ingresar la contraseña actual.");
            if (string.IsNullOrWhiteSpace(cambio.ClaveNueva))
                return BadRequest("Debe ingresar la contraseña nueva.");
            if (cambio.ClaveActual.Length > 128 || cambio.ClaveNueva.Length > 128)
                return BadRequest("La contraseña supera la longitud máxima admitida.");

            var usuId = User.FindFirst("user")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuId))
                return Unauthorized();

            var estado = _securityServicio.ObtenerEstadoSeguridad(usuId);
            if (estado.CambioClaveObligatorio)
                return BadRequest("Debe utilizar el cambio obligatorio de contraseña.");

            var admClaim = User.FindFirst("AdmId")?.Value;
            var admId = admClaim?.Split('#', 2)[0];
            var ip = Request.Headers["X-ClientUsr"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();
            var operacionId = Guid.NewGuid();

            var resultado = _securityServicio.CambiarClave(
                usuId, cambio.ClaveActual, cambio.ClaveNueva, admId, ip, operacionId);

            return Ok(new ApiResponse<CambioClaveResultadoDto>(resultado));
        }

        [HttpPost("cambio-clave-forzada"), Authorize]
        public IActionResult CambioClaveForzada(CambioClaveForzadaRequestDto cambio)
        {
            if (cambio == null || string.IsNullOrWhiteSpace(cambio.ClaveNueva))
                return BadRequest("Debe ingresar la contraseña nueva.");
            if (cambio.ClaveNueva.Length > 128)
                return BadRequest("La contraseña supera la longitud máxima admitida.");

            var usuId = User.FindFirst("user")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(usuId))
                return Unauthorized();

            var admClaim = User.FindFirst("AdmId")?.Value;
            var admId = admClaim?.Split('#', 2)[0];
            var ip = Request.Headers["X-ClientUsr"].FirstOrDefault()
                ?? HttpContext.Connection.RemoteIpAddress?.ToString();

            var resultado = _securityServicio.CambiarClaveForzada(
                usuId, cambio.ClaveNueva, admId, ip, Guid.NewGuid());
            return Ok(new ApiResponse<CambioClaveResultadoDto>(resultado));
        }


        private (bool, Usuario?) IsValidUser(UserLogin login, bool esUp = false)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");

            if (login == null)
            {
                return (false, null);
                //throw new NegocioException("No se recepcinaron las credenciales del Usuario a autenticarse.");
            }
            if (esUp)
            {
                if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrWhiteSpace(login.UserName) ||
                string.IsNullOrEmpty(login.Password) || string.IsNullOrWhiteSpace(login.Password))
                {
                    return (false, null);
                    //throw new NegocioException("Las credenciales no son correctas.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrWhiteSpace(login.UserName) ||
                    string.IsNullOrEmpty(login.Password) || string.IsNullOrWhiteSpace(login.Password) ||
                    string.IsNullOrEmpty(login.Admid) || string.IsNullOrWhiteSpace(login.Admid))
                {
                    return (false, null);
                    //throw new NegocioException("Las credenciales no son correctas.");
                }
            }
            var user = _securityServicio.GetLoginByCredential(login, esUp);
            if (user == null)
            {
                return (false, null);
            }
            // El SP es la fuente autoritativa del identificador. Esto evita que una variación
            // de mayúsculas/minúsculas escrita en el login cambie el patrón cifrado.
            bool isValid = _passwordService.Check(user.Usu_password, user.Usu_id, login.Password);
            if (!isValid)
            {
                return (false, null);
            }
            return (isValid, user);
        }

        private string GenerateToken(Usuario usuario, AdministracionDto adm, List<PerfilUserDto> perfiles,
            EstadoSeguridadUsuarioDto estadoSeguridad)/**/
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");

            //token tiene 3 partes. Comenzamos por el Header
            var symetricSecurityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"] ?? ""));

            //credenciales
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(symetricSecurityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(signingCredentials);

            //serializando los perfiles del usuario
            var jsonp = JsonConvert.SerializeObject(perfiles);

            //Claims (informacion que queresmos validar y las caracteristicas del usuario
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Usu_id),
                new Claim("nya",usuario.Usu_apellidoynombre),
                new Claim(ClaimTypes.Email,usuario.Usu_email??"sin@mail.com"),
                new Claim("AdmId", $"{adm.Adm_id}#{adm.Adm_nombre}"),
                new Claim("lp_id", $"{adm.lp_id}"),
                new Claim("expires", DateTime.Now.AddMinutes(_options.Value.TiempoDuracionToken).Ticks.ToString()),
                new Claim("user",usuario.Usu_id),
                new Claim("clave_expirada", ClaveExpirada(usuario).ToString().ToLowerInvariant()),
                new Claim("cambio_clave_obligatorio", estadoSeguridad.CambioClaveObligatorio.ToString().ToLowerInvariant()),
                new Claim("cambio_clave_motivo", estadoSeguridad.CambioClaveMotivo ?? string.Empty),
                new Claim("credencial_version", estadoSeguridad.VersionCredencial.ToString()),
                new Claim("perfiles",jsonp)

                //new Claim("etiqueta",DateTime.Now.Ticks.ToString())

                //new Claim("Id",usuario.Id.ToString()),
                //new Claim(ClaimTypes.Role,sRoles),
            };

            //payload
            var payload = new JwtPayload
                (
                _configuration["Authentication:Issuer"],
                _configuration["Authentication:Audience"],
                claims, DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(_options.Value.TiempoDuracionToken)
                );

            //token
            var token = new JwtSecurityToken(header, payload);

            /***********************************************
             * ACÁ PUEDE IR EL CODIGO PARA IDENTIFICAR QUE SE LOGUEO EL USUARIO, EL PROBLEMA ES QUE POR LO GENERAL NO SE DESLOGUEAN.
             * *********************************************/


            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static bool ClaveExpirada(Usuario usuario)
        {
            return usuario.Usu_expira == true
                && usuario.Usu_fecha_expira_inicio.HasValue
                && usuario.Usu_dias_expiracion.HasValue
                && usuario.Usu_dias_expiracion.Value > 0
                && DateTime.Now >= usuario.Usu_fecha_expira_inicio.Value
                    .AddDays(usuario.Usu_dias_expiracion.Value);
        }
    }
}
