using AspNetCoreGeneratedDocument;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Users;
using gc.sitio.core.Servicios.Implementacion;
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
        private readonly ILogger<CGUsuariosController> _logger;
        private readonly ITipoDocumentoServicio _tDocSv;
        private readonly IUserServicio _userServicio;
        private readonly ITipoNegocioServicio _tipoNegocioServicio;
        private readonly ICuentaServicio _ctaSv;
        private readonly IAbmServicio _abmSv;

        public CGUsuariosController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<CGUsuariosController> logger, ITipoNegocioServicio tipoNegocioServicio,
            ITipoDocumentoServicio tipoDocumento, IUserServicio userServicio,
            ICuentaServicio ctaSv,IAbmServicio abmServicio) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _logger = logger;
            _tDocSv = tipoDocumento;
            _userServicio = userServicio;
            _tipoNegocioServicio = tipoNegocioServicio;
            _ctaSv = ctaSv;
            _abmSv = abmServicio;
        }

        public async Task<IActionResult> Index(bool actualizar)
        {
            //se definen variables iniciales
            List<UserDto> lista;
            MetadataGrid metadata;
            GridCore<UserDto> grillaDatos;
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
            GridCore<UserDto> grillaDatos;
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
                grillaDatos = GenerarGrilla(ListaDeUsuarios, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridUsers", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda Producto";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Usuarios");
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
                _logger.LogError(ex, "Error en la invocación de la API - al Inicializar el Usuario");
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
                _logger.LogError(ex, $"Error en {this.GetType().Name}-{MethodBase.GetCurrentMethod().Name}");
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
        public async Task< JsonResult> PresentarPerfil()
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
            catch(NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de los Perfiles del Usuario.";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de los Perfiles del Usuario.");
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de las Administraciones del Usuario.");
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
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de los Derechos del Usuario.");
                return Json(new { error = true, warn = false, msg });
            }
        }

        #region Mapeo de Arboles
        #region Mapeo de Arbol Perfil
        private List<MenuRoot> GenerarArbolPerfil(List<PerfilUserDto>? lista)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            foreach (PerfilUserDto item in lista)
            {
                arbol.Add(CargarItemPerfil(item));
            }
            return arbol;
        }

        private MenuRoot CargarItemPerfil(PerfilUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = item.usu_id,
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
            foreach (AdmUserDto item in lista)
            {
                arbol.Add(CargarItemAdm(item));
            }
            return arbol;
        }

        private MenuRoot CargarItemAdm(AdmUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = item.usu_id,
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
            foreach (DerUserDto item in lista)
            {
                arbol.Add(CargarItemDer(item));
            }
            return arbol;
        }

        private MenuRoot CargarItemDer(DerUserDto item)
        {
            var mr = new MenuRoot()
            {
                id = item.usu_id,
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

        //[HttpPost]
        //public async Task<IActionResult> BuscarBarrados(string p_id)
        //{
        //    RespuestaGenerica<EntidadBase> response = new();
        //    GridCore<ProductoBarradoDto> grillaDatos;
        //    try
        //    {
        //        await ActualizaBarrados(p_id);
        //        grillaDatos = GenerarGrilla(ProductoBarrados, "P_Id_barrado");
        //        return View("_gridBarrado", grillaDatos);
        //    }
        //    catch (NegocioException ex)
        //    {
        //        response.Mensaje = ex.Message;
        //        response.Ok = false;
        //        response.EsWarn = true;
        //        response.EsError = false;
        //        return PartialView("_gridMensaje", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = "Error en la invocación de la API - Busqueda de Barrados";
        //        _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
        //        response.Mensaje = msg;
        //        response.Ok = false;
        //        response.EsWarn = false;
        //        response.EsError = true;
        //        return PartialView("_gridMensaje", response);
        //    }
        //}

        //private async Task ActualizaBarrados(string p_id)
        //{
        //    RespuestaGenerica<ProductoBarradoDto>? barr = await BuscarBarradosGen(p_id);
        //    if (barr == null || !barr.Ok)
        //    {
        //        throw new NegocioException(barr.Mensaje);
        //    }
        //    else if (barr.Ok && barr.ListaEntidad.Count() == 0)
        //    {
        //        throw new NegocioException("No se encontraron barrados para el producto.");
        //    }
        //    ProductoBarrados = barr.ListaEntidad;
        //}

        //private async Task<RespuestaGenerica<ProductoBarradoDto>?> BuscarBarradosGen(string p_id)
        //{
        //    return await _prodSv.ObtenerBarradoDeProd(p_id, TokenCookie);
        //}

        //[HttpPost]
        //public async Task<IActionResult> PresentarBarrado()
        //{
        //    RespuestaGenerica<EntidadBase> response = new();
        //    GridCore<ProductoBarradoDto> grillaDatos;
        //    int cont = 0;
        //    int tope = 2;
        //    bool continuar = true;
        //    bool encontrado = false;
        //    try
        //    {
        //        var barr = ProductoBarrados;

        //        if (barr.Count == 0)
        //        {
        //            while (continuar)
        //            {
        //                var b = await BuscarBarradosGen(ProductoABMSeleccionado.p_id);
        //                if (b.Ok && b.ListaEntidad.Count > 0)
        //                {
        //                    ProductoBarrados = b.ListaEntidad;
        //                    continuar = false;
        //                    encontrado = true;
        //                }
        //                cont++;
        //                if (cont > tope)
        //                {
        //                    continuar = false;
        //                }
        //            }
        //            if (!encontrado)
        //            {
        //                throw new NegocioException("No se encontraron barrados para este producto");
        //            }
        //        }

        //        grillaDatos = GenerarGrilla(ProductoBarrados, "P_Id_barrado");
        //        return View("_gridBarrado", grillaDatos);
        //    }
        //    catch (NegocioException ex)
        //    {
        //        response.Mensaje = ex.Message;
        //        response.Ok = false;
        //        response.EsWarn = true;
        //        response.EsError = false;
        //        return PartialView("_gridMensaje", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = "Error la Presentar  - Busqueda de Barrados";
        //        _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
        //        response.Mensaje = msg;
        //        response.Ok = false;
        //        response.EsWarn = false;
        //        response.EsError = true;
        //        return PartialView("_gridMensaje", response);
        //    }
        //}

        ///// <summary>
        ///// Se buscan los datos del barrado
        ///// </summary>
        ///// <param name="barradoId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<IActionResult> BuscarBarrado(string barradoId)
        //{
        //    try
        //    {
        //        var auth = EstaAutenticado;
        //        if (!auth.Item1 || auth.Item2 < DateTime.Now)
        //        {
        //            return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
        //        }
        //        var barr = await _prodSv.ObtenerBarrado(ProductoABMSeleccionado.p_id, barradoId, TokenCookie);
        //        if (barr == null || !barr.Ok)
        //        {
        //            throw new NegocioException(barr.Mensaje);
        //        }

        //        return Json(new { error = false, warn = false, datos = barr.Entidad });
        //    }
        //    catch (NegocioException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (UnauthorizedException ex)
        //    {
        //        return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = true, warn = false, msg = ex.Message });
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> ObtenerLimiteStk(string p_id)
        //{
        //    RespuestaGenerica<EntidadBase> response = new();
        //    GridCore<LimiteStkDto> grillaDatos;
        //    try
        //    {
        //        RespuestaGenerica<LimiteStkDto>? lim = await ObtenerLimiteStkGen(p_id);
        //        if (lim == null || !lim.Ok)
        //        {
        //            throw new NegocioException(lim.Mensaje);
        //        }
        //        else if (lim.Ok && lim.ListaEntidad.Count() == 0)
        //        {
        //            throw new NegocioException("No se recepcionaron los limites de stock.");
        //        }
        //        LimitesStk = lim.ListaEntidad;
        //        grillaDatos = GenerarGrilla(lim.ListaEntidad, "Adm_Id");
        //        return View("_gridLimStk", grillaDatos);
        //    }
        //    catch (NegocioException ex)
        //    {
        //        response.Mensaje = ex.Message;
        //        response.Ok = false;
        //        response.EsWarn = true;
        //        response.EsError = false;
        //        return PartialView("_gridMensaje", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = "Error en la invocación de la API - Busqueda Producto";
        //        _logger.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
        //        response.Mensaje = msg;
        //        response.Ok = false;
        //        response.EsWarn = false;
        //        response.EsError = true;
        //        return PartialView("_gridMensaje", response);
        //    }
        //}

        //private async Task<RespuestaGenerica<LimiteStkDto>?> ObtenerLimiteStkGen(string p_id)
        //{
        //    return await _prodSv.ObtenerLimiteStk(p_id, TokenCookie);
        //}

        ////
        //[HttpPost]
        //public async Task<IActionResult> PresentarLimiteStk()
        //{
        //    RespuestaGenerica<EntidadBase> response = new();
        //    GridCore<LimiteStkDto> grillaDatos;
        //    int cont = 0;
        //    int tope = 2;
        //    bool continuar = true;
        //    bool encontrado = false;
        //    try
        //    {
        //        var lim = LimitesStk;
        //        if (lim.Count == 0)
        //        {
        //            while (continuar)
        //            {
        //                var l = await ObtenerLimiteStkGen(ProductoABMSeleccionado.p_id);
        //                if (l.Ok && l.ListaEntidad.Count > 0)
        //                {
        //                    LimitesStk = l.ListaEntidad;
        //                    continuar = false;
        //                    encontrado = true;
        //                }
        //                cont++;
        //                if (cont > tope)
        //                {
        //                    continuar = false;
        //                }
        //            }
        //            if (!encontrado)
        //            {
        //                throw new NegocioException("No se encontraron los limites de stock para este producto");
        //            }
        //        }

        //        grillaDatos = GenerarGrilla(lim, "Adm_Nombre");
        //        return View("_gridLimStk", grillaDatos);
        //    }
        //    catch (NegocioException ex)
        //    {
        //        response.Mensaje = ex.Message;
        //        response.Ok = false;
        //        response.EsWarn = true;
        //        response.EsError = false;
        //        return PartialView("_gridMensaje", response);
        //    }
        //    catch (Exception ex)
        //    {
        //        string msg = "Error la Presentar  - Busqueda de Barrados";
        //        _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Barrados");
        //        response.Mensaje = msg;
        //        response.Ok = false;
        //        response.EsWarn = false;
        //        response.EsError = true;
        //        return PartialView("_gridMensaje", response);
        //    }
        //}

        ///// <summary>
        ///// Se buscan los datos del barrado
        ///// </summary>
        ///// <param name="admId"></param>
        ///// <returns></returns>
        //[HttpPost]
        //public async Task<IActionResult> BuscarLimite(string admId)
        //{
        //    try
        //    {
        //        var auth = EstaAutenticado;
        //        if (!auth.Item1 || auth.Item2 < DateTime.Now)
        //        {
        //            return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
        //        }
        //        RespuestaGenerica<LimiteStkDto> lim = await _prodSv.BuscarLimite(ProductoABMSeleccionado.p_id, admId, TokenCookie);
        //        if (lim == null || !lim.Ok)
        //        {
        //            throw new NegocioException(lim.Mensaje);
        //        }

        //        return Json(new { error = false, warn = false, datos = lim.Entidad });
        //    }
        //    catch (NegocioException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (UnauthorizedException ex)
        //    {
        //        return Json(new { error = false, warn = true, auth = true, msg = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = true, warn = false, msg = ex.Message });
        //    }
        //}






        //[HttpPost]
        //public async Task<JsonResult> ConfirmarAbmBarrado(ProductoBarradoDto barr, char accion)
        //{
        //    try
        //    {
        //        var auth = EstaAutenticado;
        //        if (!auth.Item1 || auth.Item2 < DateTime.Now)
        //        {
        //            return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
        //        }

        //        //agrego el id del producto que actulmente esta seleccionado
        //        barr.p_id = ProductoABMSeleccionado.p_id;

        //        barr = HelperGen.PasarAMayusculas(barr);
        //        //prod.P_Obs = prod.P_Obs.ToUpper();
        //        AbmGenDto abm = new AbmGenDto()
        //        {
        //            Json = JsonConvert.SerializeObject(barr),
        //            Objeto = "productos_barrados",
        //            Administracion = AdministracionId,
        //            Usuario = UserName,
        //            Abm = accion
        //        };

        //        var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
        //        if (res.Ok)
        //        {
        //            string msg;
        //            switch (accion)
        //            {
        //                case 'A':
        //                    msg = $"EL PROCESAMIENTO DEL ALTA DEL BARRADO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";
        //                    break;
        //                case 'M':
        //                    msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL BARRADO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";

        //                    break;
        //                default:
        //                    msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PRODUCTO {barr.p_id_barrado} SE REALIZO SATISFACTORIAMENTE";
        //                    break;
        //            }
        //            await ActualizaBarrados(barr.p_id);
        //            return Json(new { error = false, warn = false, msg });
        //        }
        //        else
        //        {
        //            return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
        //        }
        //    }
        //    catch (NegocioException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (UnauthorizedException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = true, warn = false, msg = ex.Message });
        //    }
        //}

        //[HttpPost] //
        //public async Task<JsonResult> confirmarAbmLimite(LimiteStkDto lim, char accion)
        //{
        //    try
        //    {
        //        var auth = EstaAutenticado;
        //        if (!auth.Item1 || auth.Item2 < DateTime.Now)
        //        {
        //            return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
        //        }
        //        if (string.IsNullOrEmpty(lim.adm_id))
        //        {
        //            throw new NegocioException("No se recepcionaron datos importantes del Limite de Stock de la Sucursal. Verifique.");
        //        }
        //        if (lim.p_stk_min > lim.p_stk_max)
        //        {
        //            throw new NegocioException("Es Stock Mínimo núnca puede ser mayor al Stock Máximo. Verifique.");
        //        }
        //        if (lim.p_stk_max < 1 || lim.p_stk_min < 1 || lim.p_stk_max > 99999 || lim.p_stk_min > 99999)
        //        {
        //            throw new NegocioException("El Stock mínimo y el máximo siempre deben ser mayores a 1 y menores a 99999. Verifique.");
        //        }
        //        //agrego el id del producto que actulmente esta seleccionado
        //        lim.p_id = ProductoABMSeleccionado.p_id;

        //        lim = HelperGen.PasarAMayusculas(lim);
        //        //prod.P_Obs = prod.P_Obs.ToUpper();
        //        AbmGenDto abm = new AbmGenDto()
        //        {
        //            Json = JsonConvert.SerializeObject(lim),
        //            Objeto = "productos_administraciones_stk",
        //            Administracion = AdministracionId,
        //            Usuario = UserName,
        //            Abm = accion
        //        };

        //        var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
        //        if (res.Ok)
        //        {
        //            string msg;
        //            switch (accion)
        //            {
        //                case 'A':
        //                    msg = $"EL PROCESAMIENTO DEL ALTA DEL Limite de Stock en {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
        //                    break;
        //                case 'M':
        //                    msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL BARRADO {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
        //                    break;
        //                default:
        //                    msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PRODUCTO {lim.adm_nombre} SE REALIZO SATISFACTORIAMENTE";
        //                    break;
        //            }
        //            return Json(new { error = false, warn = false, msg });
        //        }
        //        else
        //        {
        //            return Json(new { error = false, warn = true, msg = res.Entidad.resultado_msj, focus = res.Entidad.resultado_setfocus });
        //        }
        //    }
        //    catch (NegocioException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (UnauthorizedException ex)
        //    {
        //        return Json(new { error = false, warn = true, msg = ex.Message });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { error = true, warn = false, msg = ex.Message });
        //    }
        //}




        //[HttpPost]
        //public JsonResult ComboProveedorFamilia(string cta_id)
        //{
        //    try
        //    {
        //        var lista = ComboProveedoresFamilia(cta_id, _ctaSv);
        //        return Json(new { error = false, lista });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, $"Error en {this.GetType().Name}-{MethodBase.GetCurrentMethod().Name}");
        //        return Json(new { error = true, msg = ex.Message });
        //    }
        //}

    }
}
