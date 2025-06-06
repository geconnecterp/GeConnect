﻿using gc.api.Controllers.Base;
using gc.api.core.Contratos.Servicios;
using gc.api.core.Entidades;
using gc.api.core.Interfaces.Servicios;
using gc.api.infra.Datos.Contratos.Security;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Users;
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
            if(string.IsNullOrEmpty(login.Admid) )
            {
                return BadRequest("La sucursal no es valida");
            }
            if (validation.Item1)
            {   //el usuario es valido. Verificamos si esta logueado o no.
                var adm = _adminServicio.Find(login.Admid??"");

                //Obtener los perfiles de acceso del usuario.
                List<PerfilUserDto> perfiles = _usuSv.GetUserPerfiles(login.UserName);
                var token = GenerateToken(validation.Item2 ?? new(),login.Admid ?? "",adm.Adm_nombre,perfiles);
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

        [HttpPost, Authorize, Route("[action]")]
        public async Task<IActionResult> CambioClave(CambioClaveDto cambio)
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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

            ////obtenemos el usuario desde el token
            //var res = ObtenerTokenDesdeRequestAsync(false);
            //var validacion = await IsValidUser(new UserLogin { UserName = res.Item2, Password = cambio.PassAct });
            var validacion = IsValidUser(new UserLogin { UserName = cambio.UserName, Password = cambio.PassAct },true);
            if (validacion.Item1)
            {
                //    //la clave fue valida.. ahora se modifica la clave.
                var pass = _passwordService.CalculaClave(new RegistroUserDto { User=cambio.UserName, Password=cambio.PassNew });
                var usu = validacion.Item2;
                usu.Usu_password = pass;
                usu.Usu_email = $"{usu.Usu_id.Trim()}@geco.com.ar";
                var res = await _securityServicio.RegistrerUser(usu, true);

                if (res)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("No se pudo modificar la clave del operador.");
                }

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
            }
            else
            {
                return BadRequest("El usuario o clave o ambos son erroreos.");
            }

            throw new NotImplementedException();
        }


        private (bool, Usuario?) IsValidUser(UserLogin login,bool esUp=false)
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
                string.IsNullOrEmpty(login.Password) || string.IsNullOrWhiteSpace(login.Password) )
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
            var user = _securityServicio.GetLoginByCredential(login,esUp);
            if (user == null)
            {
                return (false, null);
            }
            bool isValid;
            //es un usuario existente pero se le fuerza el cambio de contraseña.
            if (login.Password.Equals("##newuser##"))
            {
                return (true, user);
            }
            isValid = _passwordService.Check(user.Usu_password, login.UserName, login.Password);
            if (!isValid)
            {
                return (false, null);
            }
            return (isValid, user);
        }

        private string GenerateToken(Usuario usuario, string admId,string admName, List<PerfilUserDto> perfiles)/**/
        {
            _logger.LogInformation($"{this.GetType().Name} - {MethodBase.GetCurrentMethod()?.Name}");
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

            //serializando los perfiles del usuario
            var jsonp = JsonConvert.SerializeObject(perfiles);

            //Claims (informacion que queresmos validar y las caracteristicas del usuario
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Usu_id),
                new Claim("nya",usuario.Usu_apellidoynombre),
                new Claim(ClaimTypes.Email,usuario.Usu_email??"sin@mail.com"),
                new Claim("AdmId", $"{admId}#{admName}"),
                new Claim("expires",DateTime.Now.AddHours(1).Ticks.ToString()),
                new Claim("user",usuario.Usu_id),
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
    }
}
