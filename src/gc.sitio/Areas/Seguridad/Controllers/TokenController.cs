using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Seguridad;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Users;
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

namespace gc.sitio.Areas.Seguridad.Controllers
{
    [Area("Seguridad")]
    public class TokenController : ControladorBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAdministracionServicio _admSv;
        private readonly IMenuesServicio _mnSv;
        private readonly AppSettings _appSettings;
        private new readonly IHttpContextAccessor _context;
        private readonly DocsManager _docsManager;

        public TokenController(IConfiguration configuration, IAdministracionServicio servicio, ILogger<TokenController> logger,
            IOptions<AppSettings> options, IHttpContextAccessor context, IMenuesServicio menuesServicio,
            IOptions<DocsManager> options1) : base(options, context)
        {
            _configuration = configuration;
            _appSettings = options.Value;
            _admSv = servicio;
            _context = context;
            _mnSv = menuesServicio;
            _docsManager = options1.Value;
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
                _logger?.LogError(ex, "Error en Login");
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
            Administraciones = adms;
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
                        if (tokenS == null)
                        {
                            throw new NegocioException("El Token no es válido. Debe autenticarse nuevamente.");
                        }
                        var user = tokenS.Claims.First(c => c.Type.Contains("name")).Value;
                        var email = tokenS.Claims.First(c => c.Type.Contains("email")).Value;
                        var nombre = tokenS.Claims.First(c => c.Type.Contains("nya")).Value;
                        var jsonp = tokenS.Claims.First(c => c.Type.Contains("perfiles")).Value.ToString();
                        ADMID = tokenS.Claims.First(c => c.Type.Contains("AdmId")).Value;

                        //29/10/2024 Ñoquis - se resguarda etiqueta, que sera la que almacene los datos en la cookie
                        Etiqueta = $"{user}GCSitio";

                        //se comienza a armar  el usuario autenticado. Se resguardara en una cookie
                        var userClaims = new List<Claim>();
                        userClaims.AddRange(tokenS.Claims);

                        var identity = new ClaimsIdentity(userClaims, "User Identity");
                        var authProperties = new AuthenticationProperties
                        {
                            ExpiresUtc = tokenS.ValidTo,
                        };

                        var cookieOptions = new CookieOptions
                        {
                            Expires = tokenS.ValidTo,
                            SameSite = SameSiteMode.Unspecified,

                        };

                        var principal = new ClaimsPrincipal(new[] { identity });
                        await _context.HttpContext?.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
                        _context.HttpContext?.Response.Cookies.Append(Etiqueta, token, cookieOptions); //se resguarda el token con el nombre del usuario

                        #region resguardamos los perfiles del usuario. 11/02/2025

                        UserPerfiles = JsonConvert.DeserializeObject<List<PerfilUserDto>>(jsonp);

                        if (UserPerfiles == null || UserPerfiles.Count() == 0)
                        {
                            throw new NegocioException("El usuario no tiene perfiles para operar en el sistema. PERMISO DENEGADO PARA ACCEDER. ");
                        }
                        //verifico si tiene un valor predeterminado.
                        if (UserPerfiles.Any(x => x.perfil_default.Equals("S")))
                        {
                            UserPerfilSeleccionado = UserPerfiles.First(x => x.perfil_default.Equals("S"));
                        }
                        else
                        {
                            //no tiene perfil predeterminado. tomare el primero de la lista.
                            //luego se le asigna ese perfil como SU determinado.
                            PerfilUserDto up = UserPerfiles.First();
                            UserPerfilSeleccionado = up;

                            RespuestaGenerica<RespuestaDto> res = await _mnSv.DefinePerfilDefault(up, token);
                        }

                        //se procede a buscar el menu inicial del sistema


                        #endregion

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

                    _logger?.LogError($"Error al autenticar: {respuesta}");
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


        [HttpPost]
        public JsonResult CambiarPerfilUsuario(string perId)
        {

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var userPerfSelec = UserPerfiles.Single(x => x.perfil_id.Equals(perId));
                UserPerfilSeleccionado = userPerfSelec;


                string msg;

                msg = $"Se modifica presentación de menú para el Perfil {userPerfSelec.perfil_descripcion}.";

                return Json(new { error = false, warn = false, msg });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (UnauthorizedException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                return Json(new { error = true, warn = false, msg = ex.Message });
            }
        }


        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            //// Acá debo invocar la api
            //HelperAPI api = new HelperAPI();
            //var cliente = api.InicializaCliente();
            //cliente.BaseAddress = new Uri(_configuration["AppSettings:RutaBase"]);
            //string usuario = UserName;
            //HttpResponseMessage response;
            //var link = $"/api/token/Logoff?UserName={usuario}";
            //response = await cliente.GetAsync(link);

            //if (response.StatusCode == HttpStatusCode.OK)
            //{

            //}
            //else if (response.StatusCode == HttpStatusCode.Unauthorized)
            //{

            //}
            //else
            //{
            //    string stringData = await response.Content.ReadAsStringAsync();
            //    _logger?.LogError($"stringData: {stringData}");


            //}

            //al desloguear redirecciona a HOME
            return RedirectToAction("Index", new RouteValueDictionary(new { area = "", controller = "Home", action = "Index" }));
        }

        private async Task<List<UsuarioMenu>> ObtenerPermisosAMenuPorUsuario(string user, HttpClient cliente)
        {

            var link = $"{_appSettings.RutaBase}/api/apisecurity/ObtenerMenuPorUsuario?usuId={user}";
            var response = await cliente.GetAsync(link);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                    _logger?.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda usuId:{user}");

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UsuarioMenu>>>(stringData);
                return apiResponse?.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger?.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new List<UsuarioMenu>();
            }
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
