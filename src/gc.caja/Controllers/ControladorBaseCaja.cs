using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Dynamic.Core;
using X.PagedList;

namespace gc.caja.Controllers
{
    public class ControladorBaseCaja : Controller
    {
        private readonly AppSettings _options; 
        private readonly AppSettings _setting;
        protected readonly IHttpContextAccessor _context;
        internal readonly ILogger? _logger;

        public ControladorBaseCaja(IOptions<AppSettings> options, IHttpContextAccessor contexto,
            ILogger logger)
        {
            _options = options.Value;
            _setting = options.Value; 
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

        protected ProductoBusquedaDto ProductoBase
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ProductoBase") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return new();
                }
                return JsonConvert.DeserializeObject<ProductoBusquedaDto>(json) ?? new();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductoBase", json);
            }
        }

        public List<ProductoBusquedaDto> ProductosSeleccionados
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ProductosSeleccionados") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ProductoBusquedaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProductosSeleccionados", json);
            }
        }

        public string LP_Id
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("LP_Id") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return string.Empty;
                }
                return JsonConvert.DeserializeObject<string>(json) ?? string.Empty;
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("LP_Id", json);
            }
            //get
            //{

            //    try
            //    {
            //        // Solo intentar acceder a los claims si el usuario está autenticado
            //        if (!(User.Identity?.IsAuthenticated ?? false))
            //        {
            //            return string.Empty;
            //        }

            //        var lpidClaim = User.Claims.FirstOrDefault(c => c.Type.Contains("lp_id"));
            //        if (lpidClaim == null || string.IsNullOrEmpty(lpidClaim.Value))
            //        {
            //            return string.Empty;
            //        }
            //        return lpidClaim.Value;
            //    }
            //    catch (Exception)
            //    {
            //        // Manejo de excepciones
            //        return string.Empty;
            //    }
            //}
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

        //public string UserName
        //{
        //    get
        //    {
        //        var handler = new JwtSecurityTokenHandler(); //Libreria System.IdentityModel.Token.Jwt (6.7.1)
        //        var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;
        //        if (tokenS == null)
        //            return string.Empty;
        //        var usuario = tokenS.Claims.First(c => c.Type.Contains("user")).Value;
        //        if (string.IsNullOrEmpty(usuario)) { return string.Empty; }
        //        return usuario;
        //    }
        //}

        // ✅ ACTUALIZADO: Propiedad UserName con protección contra tokens nulos
        protected string UserName
        {
            get
            {
                try
                {
                    // ✅ CRÍTICO: Validar que TokenCookie no sea nulo/vacío
                    if (string.IsNullOrWhiteSpace(TokenCookie))
                    {
                        _logger?.LogWarning("Intento de acceso a UserName con TokenCookie nulo/vacío");
                        return string.Empty;
                    }

                    var handler = new JwtSecurityTokenHandler();

                    // ✅ NUEVO: Validar que el token sea legible antes de intentar leerlo
                    if (!handler.CanReadToken(TokenCookie))
                    {
                        _logger?.LogWarning("Intento de acceso a UserName con token ilegible");
                        return string.Empty;
                    }

                    var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;

                    if (tokenS == null)
                    {
                        _logger?.LogWarning("No se pudo convertir el token a JwtSecurityToken");
                        return string.Empty;
                    }

                    var userName = tokenS?.Claims.First(claim => claim.Type == "user").Value;
                    return userName ?? string.Empty;
                }
                catch (ArgumentNullException ex)
                {
                    // ✅ NUEVO: Captura específica para tokens nulos
                    _logger?.LogError(ex, "Error ArgumentNullException al obtener UserName. TokenCookie: {HasToken}",
                        !string.IsNullOrEmpty(TokenCookie));
                    return string.Empty;
                }
                catch (SecurityTokenException ex)
                {
                    // ✅ NUEVO: Captura específica para errores de seguridad
                    _logger?.LogError(ex, "Error de seguridad al obtener UserName");
                    return string.Empty;
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error inesperado al obtener UserName desde token JWT");
                    return string.Empty;
                }
            }
        }

        // ✅ NUEVO: Método de validación rápida de sesión
        protected bool TieneTokenValido()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TokenCookie))
                {
                    return false;
                }

                var handler = new JwtSecurityTokenHandler();

                if (!handler.CanReadToken(TokenCookie))
                {
                    return false;
                }

                var tokenS = handler.ReadToken(TokenCookie) as JwtSecurityToken;

                return tokenS != null && tokenS.ValidTo >= DateTime.UtcNow;
            }
            catch
            {
                return false;
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

        /// <summary>
        /// ✅ NUEVO: Cliente actualmente seleccionado (datos completos)
        /// Almacena el cliente único encontrado o seleccionado desde la grilla
        /// Incluye datos básicos + datos fiscales completos
        /// </summary>
        protected CuentaDatosResultadoDto? ClienteActual
        {
            get
            {
                string json = _context.HttpContext?.Session.GetString("ClienteActual") ?? string.Empty;
                if (string.IsNullOrEmpty(json))
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<CuentaDatosResultadoDto>(json);
            }
            set
            {
                if (value == null)
                {
                    _context.HttpContext?.Session.Remove("ClienteActual");
                }
                else
                {
                    var json = JsonConvert.SerializeObject(value);
                    _context.HttpContext?.Session.SetString("ClienteActual", json);
                }
            }
        }

        // ✅ ACTUALIZADO: Método de verificación de autenticación mejorado
        protected bool VerificarAutenticacion(out IActionResult redirectResult)
        {
            redirectResult = null;

            // Validación 1: Usuario autenticado
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                _logger?.LogWarning("Usuario no autenticado intentando acceder al controlador");
                redirectResult = RedirectToAction("Login", "Token", new { area = "seguridad" });
                return false;
            }

            // ✅ NUEVO: Validación 2: Token válido presente
            if (!TieneTokenValido())
            {
                _logger?.LogWarning("Token inválido o ausente para usuario autenticado: {User}", HttpContext.User.Identity.Name);
                redirectResult = RedirectToAction("Login", "Token", new { area = "seguridad" });
                return false;
            }

            // Validación 3: Sesión de caja configurada
            if (CajaActual == null || string.IsNullOrEmpty(CajaActual.CajaId))
            {
                _logger?.LogWarning("Sesión de caja no configurada para usuario: {User}", UserName);
                TempData["error"] = "No se ha configurado una caja para esta estación.";
                redirectResult = RedirectToAction("Login", "Token", new { area = "seguridad" });
                return false;
            }

            return true;
        }

        public List<ProductoListaDto> ProductosBuscados
        {
            get
            {
                var json = _context.HttpContext?.Session?.GetString("ProductosBuscados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ProductoListaDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session?.SetString("ProductosBuscados", json);
            }
        }

        public int PaginaGrid
        {
            get
            {
                var txt = _context.HttpContext?.Session.GetString("PaginaGrid") ?? string.Empty;
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return 0;
                }
                return txt.ToInt();
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext?.Session.SetString("PaginaGrid", valor);
            }
        }

        public List<ProductoFactJsonDto> FacturaProductos
        {
            get
            {
                var json = _context.HttpContext?.Session?.GetString("FacturaProductos");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ProductoFactJsonDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session?.SetString("FacturaProductos", json);
            }
        }

        public List<FactSubtotalJsonDto> FacturaSubtotales
        {
            get
            {
                var json = _context.HttpContext?.Session?.GetString("FacturaSubtotales");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<FactSubtotalJsonDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session?.SetString("FacturaSubtotales", json);
            }
        }

        public List<FactSorteoJsonDto> FacturaSorteos
        {
            get
            {
                var json = _context.HttpContext?.Session?.GetString("FacturaSorteos");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<FactSorteoJsonDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session?.SetString("FacturaSorteos", json);
            }
        }

        public List<T> OrdenarEntidad<T>(List<T> lista, string sortdir, string sort) where T : Dto
        {
            IQueryable<T> result;
            result = lista.AsQueryable().OrderBy($"{sort} {sortdir}");
            return result.ToList();
        }

        public MetadataGrid MetadataProd
        {
            get
            {
                var txt = _context.HttpContext?.Session?.GetString("MetadataProd");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return new MetadataGrid();
                }
                return JsonConvert.DeserializeObject<MetadataGrid>(txt) ?? new MetadataGrid();
            }
            set
            {
                var valor = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session?.SetString("MetadataProd", valor);
            }

        }


        protected async Task<IActionResult> BusquedaAvanzada(string ri01, string ri02, bool act, bool dis, bool ina, bool cstk, bool sstk, string search, bool buscaNew, IProductoFactServicio _productoServicio, string sort = "p_id", string sortDir = "asc", int pag = 1, string ri03 = "%")
        {
            List<ProductoListaDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<ProductoListaDto> grillaDatos;
            RespuestaGenerica<Dto> response = new();
            try
            {
                if (!buscaNew && PaginaGrid == pag)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ProductosBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ProductosBuscados = lista;
                }
                else
                {
                    PaginaGrid = pag;
                    //traemos datos desde la base
                    var busc = new BusquedaProducto
                    {
                        Busqueda = search,
                        ConStock = cstk,
                        SinStock = sstk,
                        CtaProveedorId = ri01,
                        RubroId = ri02,
                        FamiliaId = ri03,
                        EstadoActivo = act,
                        EstadoDiscont = dis,
                        EstadoInactivo = ina,
                        Registros = _setting.NroRegistrosPagina,
                        Pagina = pag,
                        Sort = sort,
                        SortDir = sortDir,
                        Administracion = AdministracionId,
                        ListaPrecio = "003"
                    };

                    var res = await _productoServicio.BusquedaListaProductos(busc, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataProd = res.Item2 ?? new();
                    //metadata = MetadataProd;
                    ProductosBuscados = lista;
                }
                metadata = MetadataProd;

                //grillaDatos = GenerarGrilla<ProductoListaDto>(ProductosBuscados, "p_desc");
                grillaDatos = GenerarGrillaSmart<ProductoListaDto>(ProductosBuscados, sort, _setting.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, sortDir);
                return PartialView("_gridProdsAdv", grillaDatos);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Avanzada";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda Avanzada");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }



        /// <summary>
        /// Búsqueda avanzada V02 que devuelve JsonResult con ProductoListaDto completo
        /// </summary>
        protected async Task<JsonResult> BusquedaAvanzadaV02(string ri01, string ri02, string ri03, bool act, bool dis, bool ina, bool cstk, bool sstk, string search, string lp_id, bool buscaNew, IProductoFactServicio _productoServicio, string sort = "p_id", string sortDir = "asc", int pag = 1)
        {
            try
            {
                List<ProductoListaDto> lista;
                MetadataGrid metadata;

                if (!buscaNew && PaginaGrid == pag)
                {
                    // Es la misma página y hay registros, realizar reordenamiento
                    lista = ProductosBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ProductosBuscados = lista;
                    metadata = MetadataProd;
                }
                else
                {
                    PaginaGrid = pag;
                    if (search.ToIntOrNull() != null && search.Trim().Length < 6)
                    {
                        search = search.PadLeft(6, '0');
                    }
                    // ✅ BÚSQUEDA: Obtener datos desde la base
                    var busc = new BusquedaProducto
                    {
                        Busqueda = search,
                        ConStock = cstk,
                        SinStock = sstk,
                        CtaProveedorId = ri01,
                        RubroId = ri02,
                        FamiliaId = ri03,
                        EstadoActivo = act,
                        EstadoDiscont = dis,
                        EstadoInactivo = ina,
                        Registros = _setting.NroRegistrosPagina,
                        Pagina = pag,
                        Sort = sort,
                        SortDir = sortDir,
                        Administracion = AdministracionId,
                        ListaPrecio = lp_id
                    };

                    var res = await _productoServicio.BusquedaListaProductos(busc, TokenCookie);
                    lista = res.Item1 ?? [];
                    metadata = res.Item2 ?? new();

                    // Guardar en sesión para paginación
                    ProductosBuscados = lista;
                    MetadataProd = metadata;
                }

                // ✅ RETORNAR JSON: Lista completa de ProductoListaDto con metadata
                return new JsonResult(new
                {
                    error = false,
                    productos = lista,
                    metadata = new
                    {
                        totalCount = metadata.TotalCount,
                        totalPages = metadata.TotalPages,
                        pageSize = metadata.PageSize,
                        currentPage = pag,
                        sort = sort,
                        sortDir = sortDir
                    },
                    paginacion = new
                    {
                        paginaActual = pag,
                        totalRegistros = metadata.TotalCount,
                        registrosPorPagina = _setting.NroRegistrosPagina,
                        primerRegistro = ((pag - 1) * _setting.NroRegistrosPagina) + 1,
                        ultimoRegistro = Math.Min(pag * _setting.NroRegistrosPagina, metadata.TotalCount)
                    }
                });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Búsqueda Avanzada V02";
                _logger?.LogError(ex, "Error en la invocación de la API - Búsqueda Avanzada V02: {Error}", ex.Message);

                return new JsonResult(new
                {
                    error = true,
                    msg,
                    productos = new List<ProductoListaDto>(),
                    metadata = new { totalCount = 0, totalPages = 0, pageSize = 0, currentPage = pag }
                });
            }
        }
    }
}
