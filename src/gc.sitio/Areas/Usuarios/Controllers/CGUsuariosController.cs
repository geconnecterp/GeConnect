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
using System.Reflection;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class CGUsuariosController : ControladorUsuariosBase
    {
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

        public  IActionResult Index(bool actualizar)
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

                PerfilesDelUsuario = [];
                AdministracionesDelUsuario = [];
                DerechosDelUsuario = [];

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
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "Usu_apellidoynombre", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<UserDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<UserDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaGrid == pag && !buscaNew && ListaDeUsuarios.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ListaDeUsuarios.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ListaDeUsuarios = lista;
                }
                else
                {
                    PaginaGrid = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _userServicio.BuscarUsuarios(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    ListaDeUsuarios = lista;
                }
                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(ListaDeUsuarios, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

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
                UsuarioSeleccionado = usu.Entidad;


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

                UsuarioSeleccionado = new UserDto();
                ViewBag.tdoc_id = ComboTipoDoc();

                return View("_n02panel01Usuario", UsuarioSeleccionado);
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

                user = HelperGen.PasarAMayusculas(user);
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
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL USUARIO {user.usu_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL USUARIO {user.usu_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA DEL USUARIO {user.usu_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    ListaDeUsuarios = [];
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
        public async Task<JsonResult> PresentarPerfil()
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                string id = UsuarioSeleccionado.usu_id;
                PerfilesDelUsuario = [];
                // busco los perfiles
                var perf = await _userServicio.ObtenerPerfilesDelUsuario(id, TokenCookie);
                if (perf.Ok)
                {
                    PerfilesDelUsuario = perf.ListaEntidad;
                }
                else
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
        public async Task<JsonResult> PresentarAdmins()
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                string id = UsuarioSeleccionado.usu_id;
                AdministracionesDelUsuario = [];
                //busco administraciones
                var adms = await _userServicio.ObtenerAdministracionesDelUsuario(id, TokenCookie);
                if (adms.Ok)
                {
                    AdministracionesDelUsuario = adms.ListaEntidad;
                }
                else
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
        public async Task<JsonResult> PresentarDerecs()
        {
            List<MenuRoot> arbol;
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                string id = UsuarioSeleccionado.usu_id;
                DerechosDelUsuario = [];
                //busco derechos
                var ders = await _userServicio.ObtenerDerechosDelUsuario(id, TokenCookie);
                if (ders.Ok)
                {
                    DerechosDelUsuario = ders.ListaEntidad;
                }
                else
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
            foreach (PerfilUserDto item in lista)
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
                id = $"{item.perfil_id}-{item.usu_id}",
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
                    asignado = item.asignado
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
            foreach (AdmUserDto item in lista)
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
                id = $"{item.adm_id}-{item.usu_id}",
                text = $"{item.adm_id}-{item.adm_nombre}",

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true
                },

                data = new MenuRootData
                {
                    asignado = item.asignado
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
            foreach (DerUserDto item in lista)
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
                id = $"{item.der_codigo}-{item.usu_id}",
                text = $"{item.der_codigo.PadLeft(3, '0')}-{item.der_descripcion}",

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true
                },

                data = new MenuRootData
                {
                    asignado = item.asignado
                }
            };

            return mr;
        }
        #endregion
        #endregion

        #region Confirmaciones
        #region Perfil
        [HttpPost]
        public async Task<IActionResult> ConfirmarPerfsUser(string json)
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
                string usuId = UsuarioSeleccionado.usu_id;
                //Se procede a generar estructura esperada en Base de Datos a partir del menu 
                List<PerfilUserDto> perfiles = ConvierteDatosPerfilUsuario(json, usuId);
                var jsonp = JsonConvert.SerializeObject(perfiles);
                _logger?.LogInformation("#***************************************#");
                _logger?.LogInformation(json);
                _logger?.LogInformation(jsonp);
                _logger?.LogInformation("#***************************************#");
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
                    string msg = $"EL PROCESAMIENTO DE LOS PERFILES DEL USUARIO {usuId} SE REALIZO SATISFACTORIAMENTE.";

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
        public async Task<IActionResult> ConfirmarAdmsUser(string json)
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
                string usuId = UsuarioSeleccionado.usu_id;
                //Se procede a generar estructura esperada en Base de Datos a partir del menu 
                List<AdmUserDto> perfiles = ConvierteDatosAdmsUsuario(json, usuId);
                var jsonp = JsonConvert.SerializeObject(perfiles);
              
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
                    string msg = $"EL PROCESAMIENTO DE LAS ADMINISRACIONES DEL USUARIO {usuId} SE REALIZO SATISFACTORIAMENTE.";

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
        public async Task<IActionResult> ConfirmarDersUser(string json)
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
                string usuId = UsuarioSeleccionado.usu_id;
                //Se procede a generar estructura esperada en Base de Datos a partir del menu 
                List<DerUserDto> perfiles = ConvierteDatosDersUsuario(json, usuId);
                var jsonp = JsonConvert.SerializeObject(perfiles);

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
                    string msg = $"EL PROCESAMIENTO DE LOS DERECHOS DEL USUARIO {usuId} SE REALIZO SATISFACTORIAMENTE.";

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
        private List<PerfilUserDto> ConvierteDatosPerfilUsuario(string json, string usu_id)
        {
            List<PerfilUserDto> perfiles = new List<PerfilUserDto>();

            List<MenuRoot> pfs = JsonConvert.DeserializeObject<List<MenuRoot>>(json);

            foreach (var item in pfs.First().children)
            {
                perfiles.Add(ObtenerPerfil(item));
            }

            return perfiles;
        }

        private PerfilUserDto ObtenerPerfil(MenuRoot item)
        {
            var datapf = item.text.Split('-');
            PerfilUserDto pf = new PerfilUserDto()
            {
                asignado = item.state.selected,
                perfil_default = item.data.perfil_default,
                perfil_descripcion = datapf[1],
                perfil_id = datapf[0],
                usu_id = item.id.Split('-')[1]
            };
            return pf;
        }
        #endregion
        #region Administracion
        private List<AdmUserDto> ConvierteDatosAdmsUsuario(string json, string usu_id)
        {
            List<AdmUserDto> administraciones = new List<AdmUserDto>();

            List<MenuRoot> adms = JsonConvert.DeserializeObject<List<MenuRoot>>(json);

            foreach (var item in adms.First().children)
            {
                administraciones.Add(ObtenerAdm(item));
            }

            return administraciones;
        }

        private AdmUserDto ObtenerAdm(MenuRoot item)
        {
            var dataA = item.text.Split('-');
            AdmUserDto pf = new AdmUserDto()
            {
                asignado = item.state.selected,
                adm_id =  dataA[0], 
                adm_nombre = dataA[1],
                usu_id = item.id.Split('-')[1],
            };
            return pf;
        }
        #endregion
        #region Derechos
        private List<DerUserDto> ConvierteDatosDersUsuario(string json, string usu_id)
        {
            List<DerUserDto> derechos = new List<DerUserDto>();

            List<MenuRoot> drs = JsonConvert.DeserializeObject<List<MenuRoot>>(json);

            foreach (var item in drs.First().children)
            {
                derechos.Add(ObtenerDer(item));
            }

            return derechos;
        }

        private DerUserDto ObtenerDer(MenuRoot item)
        {
            var dataDer = item.text.Split('-');
            DerUserDto dr = new DerUserDto()
            {
                asignado = item.state.selected,
                usu_id = item.id.Split('-')[1],
                der_codigo= dataDer[0],
                der_descripcion = dataDer[1],
                
            };
            return dr;
        }
        #endregion
        #endregion

        #endregion
       
    }
}
