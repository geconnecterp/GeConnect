using gc.api.Controllers.Base;
using gc.api.core.DTOs;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;

namespace gc.api.Controllers.Security
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControladorBase
    {
        private readonly IOptions<ConfigNegocioOption> _options;
        private readonly IConfiguration _configuration;
        private readonly IRolServicio _roleServicio;
        private readonly ISecurityServicio _securityServicio;
        private readonly IPasswordService _passwordService;
        private readonly IEmpleadoServicio _empleadoServicio;
        private readonly ILogger<TokenController> _logger;

        public TokenController(IOptions<ConfigNegocioOption> options, IConfiguration configuration,
            IRolServicio roleServicio, IEmpleadoServicio empleadoServicio, ISecurityServicio securityServicio,
            IPasswordService passwordService, ILogger<TokenController> logger)
        {
            _options = options;
            _configuration = configuration;
            _roleServicio = roleServicio;
            _securityServicio = securityServicio;
            _passwordService = passwordService;
            _empleadoServicio = empleadoServicio;
            _logger = logger;
        }
        [HttpPost]
        public async Task<IActionResult> Authentication(UserLogin login)
        {            
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            string ip = ObtenerIPRemota(HttpContext);
            //se generara un usuario y lo vamos a validar a modo de prueba. 
            //Si el usuario fuera valido se deberia generar el token
            var validation = await IsValidUser(login);           
            if (validation.Item1)
            {   //el usuario es valido. Verificamos si esta logueado o no.
                if (validation.Item2.EstaLogueado)
                {
                    var estaLogueado = await VerificaSiEstaLogueado(login.UserName,ip);

                    if (estaLogueado.Item1)
                    {
                        return BadRequest($"El Usuario {login.UserName} ya se encuentra logueado en la IP {estaLogueado.Item2}");
                    }
                }
                var token = GenerateToken(validation.Item2,ip);
                return Ok(new { token });

            }
            return NotFound();
        }


        private async Task<(bool,string)> VerificaSiEstaLogueado(string userName,string ip)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");

            return await _empleadoServicio.VerificaSiEstaLogueado(userName,ip);


        }

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Logoff(string userName)
        {
            ApiResponse<bool> ret;
            string ip = ObtenerIPRemota(HttpContext);
            await _empleadoServicio.Logoff(userName,ip);
            ret = new ApiResponse<bool>(true);
            return Ok(ret);
        }

        [HttpPost, Authorize , Route("[action]")]
        public async Task<IActionResult> CambioClave(CambioClaveDto cambio)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            if (string.IsNullOrEmpty(cambio.PassAct))
            {
                return BadRequest("No se recepcionó la clave.");
            }
            if (string.IsNullOrEmpty(cambio.PassNew) || string.IsNullOrEmpty(cambio.PassNewVer))
            {
                return BadRequest("No se recepcionó la clave nueva.");
            }
            if (string.Compare(cambio.PassNew, cambio.PassNewVer) != 0)
            {
                return BadRequest("La clave no pudo ser validada correctamente.");
            }

            //obtenemos el usuario desde el token
            var res = ObtenerTokenDesdeRequestAsync(false);
            var validacion = await IsValidUser(new UserLogin { UserName = res.Item2, Password = cambio.PassAct });
            if (validacion.Item1)
            {
                //la clave fue valida.. ahora se modifica la clave.
                cambio.PassNew = _passwordService.Hash(cambio.PassNew);
                bool resultado = await _empleadoServicio.CambioClave(res.Item1.ToGuid(),cambio);
                if (resultado)
                {
                    return Ok(new ApiResponse<bool>(resultado));
                }
                else
                {
                    return BadRequest("No se pudo actualizar la clave.");
                }
            }
            else
            {
                return BadRequest("El usuario o clave o ambos son erroreos.");
            }
        }


        private async Task<(bool, Usuario)> IsValidUser(UserLogin login)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");

            var user = await _securityServicio.GetLoginByCredential(login);
            if (user == null)
            {
                return (false, null);
            }
            var isValid = _passwordService.Check(user.Contrasena, login.Password);
            return (isValid, user);
        }

        private string GenerateToken(Usuario usuario,string ip)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            bool first = true;
            string sRoles = string.Empty;
            //debemos obtener los datos de un usuario y de sus roles            
            var roles = _roleServicio.GetRolesForUser(usuario.UserName);
            foreach (var rol in roles)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sRoles += ',';
                }
                sRoles += rol;
            }
            //token tiene 3 partes. Comenzamos por el Header
            var symetricSecurityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"]));

            //credenciales
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(symetricSecurityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(signingCredentials);


            //Claims (informacion que queresmos validar y las caracteristicas del usuario
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, usuario.UserName),
                new Claim("User",usuario.UserName),
                new Claim(ClaimTypes.Email,usuario.Correo),
                new Claim("Id",usuario.Id.ToString()),
                new Claim(ClaimTypes.Role,sRoles),
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
            _empleadoServicio.RegistrarAcceso(usuario.Id,ip,'L');
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
