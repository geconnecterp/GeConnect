using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using X.PagedList;

namespace gc.caja.Controllers
{
    public class ControladorBaseCaja : Controller
    {
        private readonly AppSettings _options;
        protected readonly IHttpContextAccessor _context;
        internal readonly ILogger? _logger;

        public ControladorBaseCaja(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger logger)
        {
            _options = options.Value;
            _context = contexto;
            _logger = logger;
        }

        public ControladorBaseCaja(IOptions<AppSettings> options, IHttpContextAccessor contexto)
        {
            _options = options.Value;
            _context = contexto;
        }

        public string NombreSitio
        {
            get { return _options.Nombre; }
        }
        public string Etiqueta
        {
            get { return _context.HttpContext?.Session.GetString("Etiqueta") ?? string.Empty; }

            set { HttpContext.Session.SetString("Etiqueta", value); }
        }
        public string Token
        {
            get { return _context.HttpContext?.Session.GetString("JwtToken") ?? string.Empty; }

            set { HttpContext.Session.SetString("JwtToken", value); }
        }

        public string TokenCookie
        {
            get
            {
                //var nombre = User.Claims.First(c => c.Type.Contains("name")).Value;
                return _context.HttpContext?.Request.Cookies[Etiqueta] ?? string.Empty;
            }
        }

        protected List<AdministracionLoginDto> Administraciones
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("Administraciones") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<AdministracionLoginDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("Administraciones", json);
            }
        }

        public string ADMID
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ADMID") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json) ?? string.Empty;
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ADMID", json);
            }
        }

        protected List<PerfilUserDto> UserPerfiles
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("UserPerfiles") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<List<PerfilUserDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("UserPerfiles", json);
            }
        }

        public PerfilUserDto UserPerfilSeleccionado
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("UserPerfilSeleccionado") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<PerfilUserDto>(json) ?? new PerfilUserDto();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("UserPerfilSeleccionado", json);
            }
        }

        public CajaSettings CajaActual
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("CajaActual") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<CajaSettings>(json) ?? new CajaSettings();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("CajaActual", json);
            }
        }

        public string LP_Id
        {
            get
            {

                try
                {
                    // Solo intentar acceder a los claims si el usuario está autenticado
                    if (!(User.Identity?.IsAuthenticated ?? false))
                    {
                        return string.Empty;
                    }

                    var lpidClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("lp_id"));
                    if (lpidClaim == null || string.IsNullOrEmpty(lpidClaim.Value))
                    {
                        return string.Empty;
                    }
                    return lpidClaim.Value;
                }
                catch (Exception)
                {
                    // Manejo de excepciones
                    return string.Empty;
                }
            }
        }

        public string AdministracionId
        {
            get
            {
                try
                {
                    // Solo intentar acceder a los claims si el usuario está autenticado
                    if (!(User.Identity?.IsAuthenticated ?? false))
                    {
                        return string.Empty;
                    }

                    var admClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("AdmId"));
                    if (admClaim == null || string.IsNullOrEmpty(admClaim.Value))
                    {
                        return string.Empty;
                    }

                    var adm = admClaim.Value;
                    var parts = adm.Split('#');
                    _context.HttpContext?.Session.SetString("ADMID", parts[0]);
                    return parts[0];
                }
                catch
                {
                    // Manejo de excepciones
                    return string.Empty;
                }
            }
        }


        public string AdministracionName
        {
            get
            {
                var adm = User.Claims.First(c => c.Type.Contains("AdmId")).Value;
                if (string.IsNullOrEmpty(adm))
                {
                    return string.Empty;
                }

                var parts = adm.Split('#');

                return parts[1];
            }
        }

        public (bool, DateTime?) EstaAutenticado
        {
            get
            {
                DateTime? expira;
                var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
                try
                {
                    var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
                    if (tokenS == null)
                    {
                        throw new Exception("Token no valido");
                    }
                    var venc = tokenS.Claims.First(c => c.Type.Contains("expires")).Value;
                    expira = venc.ToDateTimeFromTicks();
                    if (!expira.HasValue || expira.Value < DateTime.Now)
                    {

                        return (false, null);
                    }
                }
                catch { return (false, null); }
                return (true, expira);
            }
        }

        public bool TieneRoles
        {
            get
            {
                var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
                var tokenS = handler.ReadToken(Token) as JwtSecurityToken;

                if (tokenS == null)
                    return false;
                var rolesUser = tokenS.Claims.First(c => c.Type.Contains("role")).Value;
                if (string.IsNullOrEmpty(rolesUser)) { return false; }
                return true;
            }
        }

        public string RolUsuario
        {
            get
            {
                var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
                var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
                if (tokenS == null)
                    return string.Empty;
                var rolesUser = tokenS.Claims.First(c => c.Type.Contains("role")).Value;

                #region codigo despreciable para saber el rol
                //if (User.Identity.IsAuthenticated)
                //{
                //    if (User.IsInRole(nameof(RolesUsuario.ADMINISTRACION)))
                //    {
                //        return nameof(RolesUsuario.ADMINISTRACION);
                //    }
                //    else if (User.IsInRole(nameof(RolesUsuario.ADMINISTRADOR)))
                //    {
                //        return nameof(RolesUsuario.ADMINISTRADOR);
                //    }
                //    else if (User.IsInRole(nameof(RolesUsuario.CAJERO)))
                //    {
                //        return nameof(RolesUsuario.CAJERO);
                //    }
                //    else if (User.IsInRole(nameof(RolesUsuario.CONSULTA)))
                //    {
                //        return nameof(RolesUsuario.CONSULTA);
                //    }
                //    else if (User.IsInRole(nameof(RolesUsuario.LABORATORISTA)))
                //    {
                //        return nameof(RolesUsuario.LABORATORISTA);
                //    }
                //    else if (User.IsInRole(nameof(RolesUsuario.VENDEDOR)))
                //    {
                //        return nameof(RolesUsuario.VENDEDOR);
                //    }
                //}
                #endregion
                if (string.IsNullOrEmpty(rolesUser)) { return string.Empty; }
                return rolesUser;

            }
        }

        public Guid IdUsuario
        {
            get
            {
                var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
                var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
                if (tokenS == null)
                    return Guid.Empty;
                var id = tokenS.Claims.First(c => c.Type.Contains("id")).Value;
                if (string.IsNullOrEmpty(id)) { return default; }
                return id.ToGuid();
            }
        }

        public string UserName
        {
            get
            {
                var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
                var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
                if (tokenS == null)
                    return string.Empty;
                var usuario = tokenS.Claims.First(c => c.Type.Contains("user")).Value;
                if (string.IsNullOrEmpty(usuario)) { return string.Empty; }
                return usuario;
            }
        }

        /// <summary>
        /// Lista de clientes encontrados en la última búsqueda (para grilla)
        /// </summary>
        protected List<CuentaBusquedaResultadoDto> ClientesBuscados
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ClientesBuscados") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new List<CuentaBusquedaResultadoDto>();
                }
                return JsonConvert.DeserializeObject<List<CuentaBusquedaResultadoDto>>(json) ?? new List<CuentaBusquedaResultadoDto>();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ClientesBuscados", json);
            }
        }

        /// <summary>
        /// Genera una grilla smart con paginación básica
        /// </summary>
        protected GridCoreSmart<T> GenerarGrillaSmart<T>(List<T>? lista, string sort, int cantReg = 999, int pagina = 1, int totalReg = 0, int totalPag = 1, string sortDir = "ASC")
        {
            lista ??= new List<T>();
            totalReg = lista.Count;
            
            var pagedList = new StaticPagedList<T>(lista, pagina, cantReg, totalReg);

            return new GridCoreSmart<T>
            {
                ListaDatos = pagedList,
                CantidadReg = cantReg,
                PaginaActual = pagina,
                CantidadPaginas = totalPag,
                Sort = sort,
                SortDir = sortDir
            };
        }

        /// <summary>
        /// Sobrecarga simplificada para generar grilla sin paginación compleja
        /// </summary>
        protected GridCoreSmart<T> GenerarGrillaSmart<T>(List<T>? lista, string sort)
        {
            return GenerarGrillaSmart(lista, sort, 999, 1, 0, 1, "ASC");
        }
    }
}
