using gc.api.Controllers.Base;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private readonly ILogger<ApiTokenController> _logger;

        public ApiTokenController(IOptions<ConfigNegocioOption> options, IConfiguration configuration,
            ISecurityServicio securityServicio,
            IPasswordService passwordService, ILogger<ApiTokenController> logger)
        {
            _options = options;
            _configuration = configuration;
            _securityServicio = securityServicio;
            _passwordService = passwordService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Authentication(UserLogin login)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            //string ip = ObtenerIPRemota(HttpContext);
            ////se generara un usuario y lo vamos a validar a modo de prueba. 
            ////Si el usuario fuera valido se deberia generar el token
            var validation = await IsValidUser(login);
            if (validation.Item1)
            {   //el usuario es valido. Verificamos si esta logueado o no.
              
                var token = GenerateToken(validation.Item2,login.Admid);
                return Ok(new { token });

            }
            return NotFound();
        }       

        [HttpGet]
        [Route("[action]")]
        public async Task<IActionResult> Logoff(string userName)
        {
            //ApiResponse<bool> ret;
            //string ip = ObtenerIPRemota(HttpContext);
            //await _empleadoServicio.Logoff(userName,ip);
            //ret = new ApiResponse<bool>(true);
            //return Ok(ret);
            throw new NotImplementedException();
        }

        [HttpPost, Authorize, Route("[action]")]
        public async Task<IActionResult> CambioClave(CambioClaveDto cambio)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            //if (string.IsNullOrEmpty(cambio.PassAct))
            //{
            //    return BadRequest("No se recepcionó la clave.");
            //}
            //if (string.IsNullOrEmpty(cambio.PassNew) || string.IsNullOrEmpty(cambio.PassNewVer))
            //{
            //    return BadRequest("No se recepcionó la clave nueva.");
            //}
            //if (string.Compare(cambio.PassNew, cambio.PassNewVer) != 0)
            //{
            //    return BadRequest("La clave no pudo ser validada correctamente.");
            //}

            ////obtenemos el usuario desde el token
            //var res = ObtenerTokenDesdeRequestAsync(false);
            //var validacion = await IsValidUser(new UserLogin { UserName = res.Item2, Password = cambio.PassAct });
            //if (validacion.Item1)
            //{
            //    //la clave fue valida.. ahora se modifica la clave.
            //    cambio.PassNew = _passwordService.Hash(cambio.PassNew);
            //    bool resultado = await _empleadoServicio.CambioClave(res.Item1.ToGuid(),cambio);
            //    if (resultado)
            //    {
            //        return Ok(new ApiResponse<bool>(resultado));
            //    }
            //    else
            //    {
            //        return BadRequest("No se pudo actualizar la clave.");
            //    }
            //}
            //else
            //{
            //    return BadRequest("El usuario o clave o ambos son erroreos.");
            //}

            throw new NotImplementedException();
        }


        private async Task<(bool, Usuario?)> IsValidUser(UserLogin login)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");

            if (login == null)
            {
                return (false, null);
                //throw new NegocioException("No se recepcinaron las credenciales del Usuario a autenticarse.");
            }
            if (string.IsNullOrEmpty(login.UserName) || string.IsNullOrWhiteSpace(login.UserName) ||
                string.IsNullOrEmpty(login.Password) || string.IsNullOrWhiteSpace(login.Password) ||
                string.IsNullOrEmpty(login.Admid) || string.IsNullOrWhiteSpace(login.Admid))
            {
                return (false, null);
                //throw new NegocioException("Las credenciales no son correctas.");
            }
            var user = await _securityServicio.GetLoginByCredential(login);
            if (user == null)
            {
                return (false, null);
            }
            var isValid = _passwordService.Check(user.Usu_password, login.UserName, login.Password);
            if (!isValid)
            {
                return (false, null);
            }
            return (isValid, user);
        }

        private string GenerateToken(Usuario usuario, string admId)/**/
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod().Name}");
            //bool first = true;
            /******************************************************************************************************************************
             * SE DEBE REALIZAR LA CONSULTA A LA BASE PARA OBTENER EL ARREGLO CON EL MENU A GENERAR, PADRE CON HIJOS Y RUTAS DE CADA HIJO *
             ******************************************************************************************************************************/
            //string sRoles = string.Empty;
            ////debemos obtener los datos de un usuario y de sus roles            
            //var roles = _roleServicio.GetRolesForUser(usuario.UserName);
            //foreach (var rol in roles)
            //{
            //    if (first)
            //    {
            //        first = false;
            //    }
            //    else
            //    {
            //        sRoles += ',';
            //    }
            //    sRoles += rol;
            //}


            //token tiene 3 partes. Comenzamos por el Header
            var symetricSecurityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Authentication:SecretKey"] ?? ""));

            //credenciales
            var signingCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(symetricSecurityKey, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var header = new JwtHeader(signingCredentials);


            //Claims (informacion que queresmos validar y las caracteristicas del usuario
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Usu_id),
                new Claim("nya",usuario.Usu_apellidoynombre),
                new Claim(ClaimTypes.Email,usuario.Usu_email),
                new Claim("AdmId",admId),
                new Claim("expires",DateTime.Now.AddHours(1).Ticks.ToString()),
                new Claim("user",usuario.Usu_id),

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
    }
}
