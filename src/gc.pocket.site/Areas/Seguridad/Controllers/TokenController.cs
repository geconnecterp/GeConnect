using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.Helpers;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;

namespace gc.pocket.site.Areas.Seguridad.Controllers
{
    [Area("Seguridad")]
    public class TokenController : ControladorBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenController> _logger;
        private readonly IAdministracionServicio _admSv;
        private readonly AppSettings _appSettings;
        //private readonly IHttpContextAccessor _context;

        public TokenController(IConfiguration configuration, ILogger<TokenController> logger,
            IOptions<AppSettings> options, IAdministracionServicio servicio, IOptions<AppSettings> options1,
            IHttpContextAccessor context) : base(options, context, logger)
        {
            _configuration = configuration;
            _logger = logger;
            _admSv = servicio;
            _appSettings = options1.Value;
        }

        [HttpGet]
        public IActionResult Login()
        {
            try
            {
                ComboAdministracion();
                var login = new LoginDto { Fecha = DateTime.Now };
                return View(login);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en Login");
                TempData["error"] = "Hubo algún error al intentar cargar la vista de autenticación. Si el problema persiste, avise al administardor.";
                var lv = new List<AdministracionLoginDto>();
                ViewBag.Admid = HelperMvc<AdministracionLoginDto>.ListaGenerica(lv);
                var login = new LoginDto { Fecha = DateTime.Now };
                return View(login);
            }
        }

        private void ComboAdministracion()
        {
            var adms = _admSv.GetAdministracionLogin();
            ViewBag.Admid = HelperMvc<AdministracionLoginDto>.ListaGenerica(adms);
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto autenticar)
        {
            try
            {
                //obtengo IP del cliente
                var ip = ObtenerIpCliente(Request);

                // Acá debo invocar la api
                HelperAPI api = new HelperAPI();
                var cliente = api.InicializaCliente();

                //inyectamos la ip en el header del request
                cliente.DefaultRequestHeaders.Add("X-ClientUsr", ip.ToString());

                //cliente.BaseAddress = new Uri(_configuration["AppSettings:RutaBase"]);
                var userModel = new { autenticar.UserName, autenticar.Password, autenticar.Admid };
                var userJson = JsonConvert.SerializeObject(userModel);
                var contentData = new StringContent(userJson, Encoding.UTF8, "application/json");
                var link = $"{_appSettings.RutaBase}/api/apitoken";
                var response = await cliente.PostAsync(link, contentData);

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



                        var user = tokenS.Claims.First(c => c.Type.Contains("name")).Value;
                        var email = tokenS.Claims.First(c => c.Type.Contains("email")).Value;
                        var nombre = tokenS.Claims.First(c => c.Type.Contains("nya")).Value;
                        //29/10/2024 Ñoquis - se resguarda etiqueta, que sera la que almacene los datos en la cookie
                        Etiqueta = $"{user.Trim()}GCPocket";

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

                        //var etiqueta = $"{user}";//{HelperGen.ConvierteStrToHex(user)}

                        var principal = new ClaimsPrincipal(new[] { identity });
                        await _context.HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                        _context.HttpContext.Response.Cookies.Append(Etiqueta, token, cookieOptions); //se resguarda el token con el nombre del usuario
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

                    var respuesta = await response.Content.ReadAsStringAsync();

                    //ExceptionValidation valid = JsonConvert.DeserializeObject<ExceptionValidation>(respuesta);
                    _logger.LogError($"Error al autenticar: {respuesta}");
                    throw new NegocioException("No se ha podido autenticar. El usuario o contraseña no son correctos.");
                }

                return View(autenticar);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                ComboAdministracion();
                var login = new LoginDto { Fecha = DateTime.Now };
                return View(login);
            }

        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //TENGO QUE HACER LAS ACTUALIZACIONES EN LA API SOBRE EL LOGOUT DEL SISTEMA.

            ////////////////// Acá debo invocar la api
            ////////////////HelperAPI api = new HelperAPI();
            ////////////////var cliente = api.InicializaCliente();
            ////////////////cliente.BaseAddress = new Uri(_configuration["AppSettings:RutaBase"]);
            ////////////////string usuario = UserName;
            ////////////////HttpResponseMessage response;
            ////////////////var link = $"/api/token/Logoff?UserName={usuario}";
            ////////////////response = await cliente.GetAsync(link);

            ////////////////if (response.StatusCode == HttpStatusCode.OK)
            ////////////////{

            ////////////////}
            ////////////////else if (response.StatusCode == HttpStatusCode.Unauthorized)
            ////////////////{

            ////////////////}
            ////////////////else
            ////////////////{
            ////////////////    string stringData = await response.Content.ReadAsStringAsync();
            ////////////////    _logger.LogError( $"stringData: {stringData}");


            ////////////////}

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
