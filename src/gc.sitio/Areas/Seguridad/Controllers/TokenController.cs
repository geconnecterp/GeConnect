using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Interfaces;
using gc.sitio.Controllers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Net;
using System.Security.Claims;
using System.Text;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Seguridad;

namespace gc.sitio.Areas.Seguridad.Controllers
{
    [Area("Seguridad")]
    public class TokenController : ControladorBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILoggerHelper _logger;
        private readonly AppSettings _appSettings;
       

        public TokenController(IConfiguration configuration, ILoggerHelper logger, IOptions<AppSettings> options) : base(options)
        {
            _configuration = configuration;
            _logger = logger;
            _appSettings = options.Value;
        }

        [HttpGet]
        public IActionResult Login()
        {
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto autenticar)
        {
            //obtengo IP del cliente
            var ip = ObtenerIpCliente(Request);

            // Acá debo invocar la api
            HelperAPI api = new HelperAPI();
            var cliente = api.InicializaCliente();

            //inyectamos la ip en el header del request
            cliente.DefaultRequestHeaders.Add("X-ClientUsr", ip.ToString());

            cliente.BaseAddress = new Uri(_configuration["AppSettings:RutaBase"]);
            var userModel = new { autenticar.UserName, autenticar.Password };
            var userJson = JsonConvert.SerializeObject(userModel);
            var contentData = new StringContent(userJson, Encoding.UTF8, "application/json");

            var response = await cliente.PostAsync("/api/token", contentData);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                //se debe leer el cuerpo del response
                string strJWT = await response.Content.ReadAsStringAsync();
                //obtenermos el TOKEN   
                AutenticacionDto auth = JsonConvert.DeserializeObject<AutenticacionDto>(strJWT);
                if (!string.IsNullOrEmpty(auth.Token))
                {
                    var token = auth.Token;
                    var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)

                    var tokenS = handler.ReadToken(token) as JwtSecurityToken;



                    var roleUser = tokenS.Claims.First(c => c.Type.Contains("role")).Value.Split(',');
                    var email = tokenS.Claims.First(c => c.Type.Contains("email")).Value;
                    var user = tokenS.Claims.First(c => c.Type.Contains("User")).Value;



                    //se comienza a armar  el usuario autenticado. Se resguardara en una cookie

                    var userClaims = new List<Claim>();
                    userClaims.AddRange(tokenS.Claims);

                    var identity = new ClaimsIdentity(userClaims, "User Identity");

                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        ExpiresUtc = tokenS.ValidTo,
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        //IsPersistent = true,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.

                    };

                    var cookieOptions = new CookieOptions
                    {
                        Expires = tokenS.ValidTo,
                        SameSite = SameSiteMode.Unspecified,

                    };

                    var etiqueta = $"{user}";

                    var principal = new ClaimsPrincipal(new[] { identity });
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                    HttpContext.Response.Cookies.Append(etiqueta, token, cookieOptions);
                    //if (roleUser[0].Equals(RolesUsuario.VENDEDOR.ToString()))
                    //{
                    //    return RedirectToAction("Venta", new RouteValueDictionary(new { area = "Salon", controller = "Bandeja", action = "Venta" }));
                    //}
                    return RedirectToAction("Index", new RouteValueDictionary(new { area = "", controller = "Home", action = "Index" }));
                }
                else
                {
                    TempData["error"] = "No se ha podido autenticar. Si el problema persiste avisé al administrador";
                }
            }
            else
            {
                TempData["error"] = "No se ha podido autenticar. El usuario o contraseña no son correctos.";
                var respuesta = await response.Content.ReadAsStringAsync();
                ExceptionValidation valid = JsonConvert.DeserializeObject<ExceptionValidation>(respuesta);
                _logger.Log(TraceEventType.Error, $"{valid.Title} - {valid.Status} - {valid.Detail}");
            }

            return View(autenticar);

        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Acá debo invocar la api
            HelperAPI api = new HelperAPI();
            var cliente = api.InicializaCliente();
            cliente.BaseAddress = new Uri(_configuration["AppSettings:RutaBase"]);
            string usuario = UserName;
            HttpResponseMessage response;
            var link = $"/api/token/Logoff?UserName={usuario}";
            response = await cliente.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {

            }
            else if (response.StatusCode == HttpStatusCode.Unauthorized)
            {

            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.Log(TraceEventType.Error, $"stringData: {stringData}");


            }

            //al desloguear redirecciona a HOME
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "", controller = "Home", action = "Index" }));
        }

        private string ObtenerIpCliente(HttpRequest request)
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "255.255.255.255";
        }

    }
}
