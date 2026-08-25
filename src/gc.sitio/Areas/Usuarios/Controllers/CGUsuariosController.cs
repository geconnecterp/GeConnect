using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Mail;
using System.Reflection;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class CGUsuariosController : ControladorUsuariosBase
    {
        private const string MetadataSessionPrefix = "CGUsuarios:Metadata:";
        private readonly AppSettings _settings;
        //private readonly ILogger<CGUsuariosController> _logger;
        private readonly ITipoDocumentoServicio _tDocSv;
        private readonly IUserServicio _userServicio;
        private readonly ITipoNegocioServicio _tipoNegocioServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IAbmServicio _abmSv;

        public CGUsuariosController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<CGUsuariosController> logger, ITipoNegocioServicio tipoNegocioServicio,
            ITipoDocumentoServicio tipoDocumento, IUserServicio userServicio,
            ICuentaServicio ctaSv, IAbmServicio abmServicio) : base(options, accessor, logger)
        {
            _settings = options.Value;
            //  _logger = logger;
            _tDocSv = tipoDocumento;
            _userServicio = userServicio;
            _tipoNegocioServicio = tipoNegocioServicio;
            _ctaSv = ctaSv;
            _abmSv = abmServicio;
        }

        public async Task<IActionResult> Index(bool actualizar)
        {
            //se definen variables iniciales

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                CargarDatosIniciales(actualizar);

                LimpiarEstadoLegacyUsuarios();
                ViewBag.ModuleInstanceId = Guid.NewGuid().ToString("N");
                var operaciones = await _userServicio.ObtenerOperacionesSeguridad(TokenCookie);
                ViewBag.PuedeBlanquearClave = operaciones.PuedeBlanquearClave;
                ViewBag.PuedeDesbloquearUsuario = operaciones.PuedeDesbloquearUsuario;

                ViewData["Titulo"] = "Gestión de Usuarios";
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "home", new { area = "" });
            }
        }

        public void CargarDatosIniciales(bool actualizar)
        {
            //
            if (TipoNegocioLista.Count == 0 || actualizar)
                ObtenerTiposNegocio(_tipoNegocioServicio);

            if (TipoDocumentoLista.Count == 0 || actualizar)
            {
                ObtenerTiposDocumento(_tDocSv);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string moduleInstanceId, string sort = "Usu_apellidoynombre", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<UserDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<UserDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                ValidarInstancia(moduleInstanceId);

                // La consulta se resuelve siempre contra la API. No se comparte la lista entre
                // pestañas ni se reutiliza una selección guardada en la sesión del navegador.
                query.Sort = sort;
                query.SortDir = sortDir;
                query.Registros = _settings.NroRegistrosPagina;
                query.Pagina = pag;

                var res = await _userServicio.BuscarUsuarios(query, TokenCookie);
                lista = res.Item1 ?? [];
                metadata = res.Item2 ?? new MetadataGrid();
                GuardarMetadata(moduleInstanceId, metadata);

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(lista, sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridUsers", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda de Usuarios";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Usuarios");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public JsonResult ObtenerDatosPaginacionUsuarios(string moduleInstanceId)
        {
            try
            {
                ValidarInstancia(moduleInstanceId);
                return Json(new { error = false, Metadata = ObtenerMetadata(moduleInstanceId) });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo obtener la paginación de Gestión de Usuarios.");
                return Json(new { error = true, msg = "No se pudo obtener la información de paginación." });
            }
        }

        private void LimpiarEstadoLegacyUsuarios()
        {
            // Estas claves pertenecían a la implementación anterior y eran compartidas por
            // todas las pestañas del navegador.
            string[] claves =
            [
                "PerfilesDelUsuario",
                "AdministracionesDelUsuario",
                "DerechosDelUsuario",
                "ListaDeUsuarios",
                "UsuarioSeleccionado"
            ];

            foreach (string clave in claves)
            {
                HttpContext.Session.Remove(clave);
            }
        }

        private static void ValidarInstancia(string moduleInstanceId)
        {
            if (string.IsNullOrWhiteSpace(moduleInstanceId) ||
                !Guid.TryParseExact(moduleInstanceId, "N", out _))
            {
                throw new NegocioException("La instancia de Gestión de Usuarios no es válida. Inicialice nuevamente el módulo.");
            }
        }

        private void GuardarMetadata(string moduleInstanceId, MetadataGrid metadata)
        {
            HttpContext.Session.SetString(
                MetadataSessionPrefix + moduleInstanceId,
                JsonConvert.SerializeObject(metadata));
        }

        private MetadataGrid ObtenerMetadata(string moduleInstanceId)
        {
            string? json = HttpContext.Session.GetString(MetadataSessionPrefix + moduleInstanceId);
            return string.IsNullOrWhiteSpace(json)
                ? new MetadataGrid()
                : JsonConvert.DeserializeObject<MetadataGrid>(json) ?? new MetadataGrid();
        }

        private static void ValidarUsuarioSeleccionado(string usuId)
        {
            if (string.IsNullOrWhiteSpace(usuId) || usuId.Trim().Length > 10)
            {
                throw new NegocioException("El usuario seleccionado no es válido.");
            }
        }

        private static void ValidarUsuario(UserDto user, char accion)
        {
            if (user == null)
            {
                throw new NegocioException("No se recibieron los datos del usuario.");
            }

            if (accion is not ('A' or 'M' or 'B'))
            {
                throw new NegocioException("La operación solicitada no es válida.");
            }

            if (string.IsNullOrWhiteSpace(user.usu_id) || user.usu_id.Trim().Length > 10)
            {
                throw new NegocioException("El Logon es obligatorio y admite hasta 10 caracteres.");
            }

            if (accion != 'B' && (string.IsNullOrWhiteSpace(user.usu_apellidoynombre) || user.usu_apellidoynombre.Trim().Length > 50))
            {
                throw new NegocioException("Apellido y Nombre es obligatorio y admite hasta 50 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(user.usu_documento) && user.usu_documento.Trim().Length > 11)
            {
                throw new NegocioException("El documento admite hasta 11 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(user.usu_email) && user.usu_email.Trim().Length > 80)
            {
                throw new NegocioException("El email admite hasta 80 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(user.usu_email) &&
                !MailAddress.TryCreate(user.usu_email.Trim(), out _))
            {
                throw new NegocioException("Ingrese un email válido.");
            }

            if (!string.IsNullOrWhiteSpace(user.usu_celu) && user.usu_celu.Trim().Length > 80)
            {
                throw new NegocioException("El celular admite hasta 80 caracteres.");
            }

            if (!string.IsNullOrWhiteSpace(user.cta_id) && user.cta_id.Trim().Length > 8)
            {
                throw new NegocioException("La cuenta de cliente seleccionada no es válida.");
            }
        }

        [HttpPost]
        public JsonResult InicializarModulo(string moduleInstanceId)
        {
            try
            {
                ValidarInstancia(moduleInstanceId);
                HttpContext.Session.Remove(MetadataSessionPrefix + moduleInstanceId);
                return Json(new { error = false });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarUsuarioDatos(string id)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var usu = await _userServicio.BuscarUsuarioDatos(id, TokenCookie);
                if (usu == null || !usu.Ok)
                {
                    if (usu == null)
                    {
                        throw new NegocioException("No se recepcionó el usuario buscado.");
                    }
                    else
                    {
                        throw new NegocioException(usu.Mensaje);
                    }
                }
                //busca combo familia
                //aca debo armar el combo de tipoDocumento
                ViewBag.Tdoc_Id = ComboTipoDoc();


                return View("_n02panel01Usuario", usu.Entidad);

            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de Usuarios";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Usuarios");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public IActionResult NuevoUsuario()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {

                var nuevoUsuario = new UserDto();
                ViewBag.tdoc_id = ComboTipoDoc();

                return View("_n02panel01Usuario", nuevoUsuario);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Inicializar Usuario";
                _logger?.LogError(ex, "Error en la invocación de la API - al Inicializar el Usuario");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ComboListaClientes(string search)
        {
            try
            {
                var lista = await _ctaSv.ObtenerListaClientes(search, TokenCookie);
                if (lista.Ok)
                {
                    var listaCli = lista.ListaEntidad.Select(x => new { x.Cta_Id, x.Cta_Denominacion, x.Ctac_habilitada });
                    return Json(listaCli);
                }
                else
                {
                    throw new NegocioException(lista.Mensaje);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error en {this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name}");
                return Json(new { error = true, msg = ex.Message });
            }

        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmUsuario(UserDto user, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                ValidarUsuario(user, accion);
                string usuIdNormalizado = user.usu_id.Trim().ToLowerInvariant();
                string? emailNormalizado = user.usu_email?.Trim().ToLowerInvariant();

                user = HelperGen.PasarAMayusculas(user);
                user.usu_id = usuIdNormalizado;
                user.usu_email = emailNormalizado;
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(user),
                    Objeto = "usuarios",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = accion
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg;
                    switch (accion)
                    {
                        case 'A':
                            msg = $"El alta del usuario {user.usu_id} se realizó satisfactoriamente.";
                            break;
                        case 'M':
                            msg = $"La modificación del usuario {user.usu_id} se realizó satisfactoriamente.";
                            break;
                        default:
                            msg = $"La baja del usuario {user.usu_id} se realizó satisfactoriamente.";
                            break;
                    }
                    if (abm.Abm.Equals('A'))
                    {
                        return Json(new { error = false, warn = false, msg, id = res.Entidad.resultado_id });
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
                }
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

        [HttpPost]
        public async Task<JsonResult> PresentarPerfil(string usuId)
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                ValidarUsuarioSeleccionado(usuId);
                // busco los perfiles
                var perf = await _userServicio.ObtenerPerfilesDelUsuario(usuId, TokenCookie);
                if (!perf.Ok)
                {
                    throw new NegocioException("No se encontraron los perfiles del usuario.");
                }
                arbol = GenerarArbolPerfil(perf.ListaEntidad);
                var jarbol = JsonConvert.SerializeObject(arbol);
                return Json(new { error = false, warn = false, arbol = jarbol });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de los Perfiles del Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de los Perfiles del Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }


        [HttpPost]
        public JsonResult ObtenerUsuarioParaLista(string prefix)
        {
            var users = ObtenerUsuarioParaListaBase(prefix, _userServicio);
            var clientes = users.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
            return Json(clientes);
        }


        [HttpPost]
        public async Task<JsonResult> PresentarAdmins(string usuId)
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                ValidarUsuarioSeleccionado(usuId);
                //busco administraciones
                var adms = await _userServicio.ObtenerAdministracionesDelUsuario(usuId, TokenCookie);
                if (!adms.Ok)
                {
                    throw new NegocioException("No se encontraron las Administraciones del usuario.");
                }

                arbol = GenerarArbolAdm(adms.ListaEntidad);
                var jarbol = JsonConvert.SerializeObject(arbol);
                return Json(new { error = false, warn = false, arbol = jarbol });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de las Administraciones del Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de las Administraciones del Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }
        [HttpPost]
        public async Task<JsonResult> PresentarDerecs(string usuId)
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                ValidarUsuarioSeleccionado(usuId);
                //busco derechos
                var ders = await _userServicio.ObtenerDerechosDelUsuario(usuId, TokenCookie);
                if (!ders.Ok)
                {
                    throw new NegocioException("No se encontraron los Derechos del usuario.");
                }
                arbol = GenerarArbolDer(ders.ListaEntidad);
                var jarbol = JsonConvert.SerializeObject(arbol);
                return Json(new { error = false, warn = false, arbol = jarbol });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de los Derechos del Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de los Derechos del Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }

        #region Mapeo de Arboles
        #region Mapeo de Arbol Perfil
        private List<MenuRoot> GenerarArbolPerfil(List<PerfilUserDto>? lista)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            MenuRoot root = new MenuRoot
            {
                id = "00",
                text = "PERFILES",
                state = new Estado { disabled = true, opened = true, selected = false },
                children = new List<MenuRoot>()
            };
            foreach (PerfilUserDto item in lista ?? [])
            {
                root.children.Add(CargarItemPerfil(item));
            }
            arbol.Add(root);
            return arbol;
        }

        private MenuRoot CargarItemPerfil(PerfilUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = $"perfil-{item.perfil_id}",
                text = $"{item.perfil_id}-{item.perfil_descripcion}",

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true
                },

                data = new MenuRootData
                {
                    perfil_default = item.perfil_default,
                    asignado = item.asignado,
                    item_id = item.perfil_id,
                    tipo = item.perfil_descripcion
                }
            };

            return mr;
        }
        #endregion
        #region Mapeo de Arbol Administracion
        private List<MenuRoot> GenerarArbolAdm(List<AdmUserDto>? lista)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            MenuRoot root = new MenuRoot
            {
                id = "00",
                text = "Administraciones",
                state = new Estado { disabled = true, opened = true, selected = false },
                children = new List<MenuRoot>()
            };
            foreach (AdmUserDto item in lista ?? [])
            {
                root.children.Add(CargarItemAdm(item));
            }
            arbol.Add(root);
            return arbol;
        }

        private MenuRoot CargarItemAdm(AdmUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = $"administracion-{item.adm_id}",
                text = $"{item.adm_id}-{item.adm_nombre}",

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true
                },

                data = new MenuRootData
                {
                    asignado = item.asignado,
                    item_id = item.adm_id,
                    tipo = item.adm_nombre
                }
            };

            return mr;
        }
        #endregion
        #region Mapeo de Arbol Derechos
        private List<MenuRoot> GenerarArbolDer(List<DerUserDto>? lista)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            MenuRoot root = new MenuRoot
            {
                id = "00",
                text = "Derechos",
                state = new Estado { disabled = true, opened = true, selected = false },
                children = new List<MenuRoot>()

            };
            foreach (DerUserDto item in lista ?? [])
            {
                root.children.Add(CargarItemDer(item));
            }
            arbol.Add(root);
            return arbol;
        }

        private MenuRoot CargarItemDer(DerUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = $"derecho-{item.der_codigo}",
                text = $"{item.der_codigo.PadLeft(3, '0')}-{item.der_descripcion}",

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true
                },

                data = new MenuRootData
                {
                    asignado = item.asignado,
                    item_id = item.der_codigo,
                    tipo = item.der_descripcion
                }
            };

            return mr;
        }
        #endregion
        #endregion

        #region Confirmaciones
        #region Perfil
        [HttpPost]
        public async Task<IActionResult> ConfirmarPerfsUser(string json, string usuId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrEmpty(json))
                {
                    string msg = "No se recepcionaron los perfiles del Usuario. Verifique.";
                    return Json(new { error = false, warn = true, msg });
                }
                ValidarUsuarioSeleccionado(usuId);
                var perfilesActuales = await _userServicio.ObtenerPerfilesDelUsuario(usuId, TokenCookie);
                if (!perfilesActuales.Ok || perfilesActuales.ListaEntidad == null)
                {
                    throw new NegocioException("No fue posible validar el catálogo de perfiles del usuario.");
                }

                // El catálogo del servidor es la fuente autoritativa. Del navegador solamente
                // se toman las marcas seleccionadas.
                List<PerfilUserDto> perfiles = ConvierteDatosPerfilUsuario(json, usuId, perfilesActuales.ListaEntidad);
                var jsonp = JsonConvert.SerializeObject(perfiles);
                //armando request del confirmar
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = jsonp,
                    Objeto = "usuarios_perfil",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = 'A'
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg = $"Los perfiles del usuario {usuId} se actualizaron satisfactoriamente.";

                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj });
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Carga de Perfiles de Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Carga de Perfiles de Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }

        #endregion
        #region Administracion
        [HttpPost]
        public async Task<IActionResult> ConfirmarAdmsUser(string json, string usuId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrEmpty(json))
                {
                    string msg = "No se recepcionaron las Administraciones del Usuario. Verifique.";
                    return Json(new { error = false, warn = true, msg });
                }
                ValidarUsuarioSeleccionado(usuId);
                var administracionesActuales = await _userServicio.ObtenerAdministracionesDelUsuario(usuId, TokenCookie);
                if (!administracionesActuales.Ok || administracionesActuales.ListaEntidad == null)
                {
                    throw new NegocioException("No fue posible validar el catálogo de sucursales del usuario.");
                }

                List<AdmUserDto> administraciones = ConvierteDatosAdmsUsuario(json, usuId, administracionesActuales.ListaEntidad);
                var jsonp = JsonConvert.SerializeObject(administraciones);

                //armando request del confirmar
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = jsonp,
                    Objeto = "usuarios_adm",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = 'A'
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg = $"Las sucursales del usuario {usuId} se actualizaron satisfactoriamente.";

                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj });
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Carga de Administraciones de Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Carga de Administraciones de Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }

        #endregion
        #region Derechos
        [HttpPost]
        public async Task<IActionResult> ConfirmarDersUser(string json, string usuId)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                if (string.IsNullOrEmpty(json))
                {
                    string msg = "No se recibieron los derechos del usuario. Verifique.";
                    return Json(new { error = false, warn = true, msg });
                }
                ValidarUsuarioSeleccionado(usuId);
                var derechosActuales = await _userServicio.ObtenerDerechosDelUsuario(usuId, TokenCookie);
                if (!derechosActuales.Ok || derechosActuales.ListaEntidad == null)
                {
                    throw new NegocioException("No fue posible validar el catálogo de derechos del usuario.");
                }

                List<DerUserDto> derechos = ConvierteDatosDersUsuario(json, usuId, derechosActuales.ListaEntidad);
                var jsonp = JsonConvert.SerializeObject(derechos);

                //armando request del confirmar
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = jsonp,
                    Objeto = "usuarios_Der",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = 'A'
                };

                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg = $"Los derechos del usuario {usuId} se actualizaron satisfactoriamente.";

                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj });
                }
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Carga de Derechos de Usuario.";
                _logger?.LogError(ex, "Error en la invocación de la API - Carga de Derechos de Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }
        #endregion



        #region Mapeos
        #region Perfil
        private List<PerfilUserDto> ConvierteDatosPerfilUsuario(string json, string usuId, List<PerfilUserDto> catalogo)
        {
            var seleccion = ObtenerSeleccion(json, "perfiles");
            ValidarCatalogo(seleccion, catalogo.Select(x => x.perfil_id), "perfiles");

            return catalogo.Select(item => new PerfilUserDto
            {
                asignado = seleccion[item.perfil_id],
                perfil_default = item.perfil_default,
                perfil_descripcion = item.perfil_descripcion,
                perfil_id = item.perfil_id,
                usu_id = usuId
            }).ToList();
        }

        [HttpPost]
        public async Task<JsonResult> BlanquearClave(string usuId)
        {
            try
            {
                ValidarUsuarioSeleccionado(usuId);
                if (string.Equals(usuId.Trim(), UserName, StringComparison.OrdinalIgnoreCase))
                    throw new NegocioException("No puede blanquear su propia contraseña.");

                var resultado = await _userServicio.BlanquearClave(usuId.Trim(), TokenCookie,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return Json(new { error = resultado.resultado < 0, warn = resultado.resultado > 0, msg = resultado.resultado_msj });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo blanquear la contraseña del usuario {Usuario}.", usuId);
                return Json(new { error = true, warn = false, msg = "No se pudo completar el blanqueo de contraseña." });
            }
        }

        [HttpPost]
        public async Task<JsonResult> DesbloquearUsuario(string usuId)
        {
            try
            {
                ValidarUsuarioSeleccionado(usuId);
                if (string.Equals(usuId.Trim(), UserName, StringComparison.OrdinalIgnoreCase))
                    throw new NegocioException("No puede desbloquear su propio usuario.");

                var resultado = await _userServicio.DesbloquearUsuario(usuId.Trim(), TokenCookie,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
                return Json(new { error = resultado.resultado < 0, warn = resultado.resultado > 0, msg = resultado.resultado_msj });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "No se pudo desbloquear el usuario {Usuario}.", usuId);
                return Json(new { error = true, warn = false, msg = "No se pudo completar el desbloqueo del usuario." });
            }
        }
        #endregion
        #region Administracion
        private List<AdmUserDto> ConvierteDatosAdmsUsuario(string json, string usuId, List<AdmUserDto> catalogo)
        {
            var seleccion = ObtenerSeleccion(json, "sucursales");
            ValidarCatalogo(seleccion, catalogo.Select(x => x.adm_id), "sucursales");

            return catalogo.Select(item => new AdmUserDto
            {
                asignado = seleccion[item.adm_id],
                adm_id = item.adm_id,
                adm_nombre = item.adm_nombre,
                usu_id = usuId
            }).ToList();
        }
        #endregion
        #region Derechos
        private List<DerUserDto> ConvierteDatosDersUsuario(string json, string usuId, List<DerUserDto> catalogo)
        {
            var seleccion = ObtenerSeleccion(json, "derechos");
            ValidarCatalogo(seleccion, catalogo.Select(x => x.der_codigo), "derechos");

            return catalogo.Select(item => new DerUserDto
            {
                asignado = seleccion[item.der_codigo],
                usu_id = usuId,
                der_codigo = item.der_codigo,
                der_descripcion = item.der_descripcion
            }).ToList();
        }

        private static Dictionary<string, bool> ObtenerSeleccion(string json, string nombreCatalogo)
        {
            var arbol = JsonConvert.DeserializeObject<List<MenuRoot>>(json);
            var nodos = arbol?.FirstOrDefault()?.children
                ?? throw new NegocioException($"No se recibió el catálogo de {nombreCatalogo}.");

            var seleccion = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
            foreach (var nodo in nodos)
            {
                string itemId = nodo.data?.item_id?.Trim() ?? string.Empty;
                if (string.IsNullOrEmpty(itemId) || !seleccion.TryAdd(itemId, nodo.state.selected))
                {
                    throw new NegocioException($"La selección de {nombreCatalogo} contiene datos inválidos o repetidos.");
                }
            }

            return seleccion;
        }

        private static void ValidarCatalogo(Dictionary<string, bool> seleccion, IEnumerable<string> idsCatalogo, string nombreCatalogo)
        {
            var ids = idsCatalogo.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            if (seleccion.Count != ids.Count || ids.Any(id => !seleccion.ContainsKey(id)))
            {
                throw new NegocioException($"El catálogo de {nombreCatalogo} cambió o está incompleto. Recárguelo antes de confirmar.");
            }
        }
        #endregion
        #endregion

        #endregion

    }
}
