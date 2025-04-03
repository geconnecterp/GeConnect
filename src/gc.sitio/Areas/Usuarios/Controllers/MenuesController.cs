using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    [Area("Usuarios")]
    public class MenuesController : ControladorUsuariosBase
    {
        private readonly AppSettings _settings;
        private readonly ILogger<MenuesController> _logger;
        private readonly IMenuesServicio _mnSrv;
        private readonly IAbmServicio _abmSv;


        public MenuesController(IOptions<AppSettings> options, IHttpContextAccessor accessor,
            ILogger<MenuesController> logger, IMenuesServicio menuesServicio, IAbmServicio abmServicio) : base(options, accessor, logger)

        {
            _logger = logger;
            _settings = options.Value;
            _mnSrv = menuesServicio;
            _abmSv = abmServicio;
        }

        public async Task<IActionResult> Index()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            PerfilesBuscados = [];
            PerfilIDSeleccionado = string.Empty;
            PerfilSeleccionado = new PerfilDto();
            //se carga el combo de tipos de menues
            ViewBag.MenuId = await ComboMenues(_mnSrv);
            ViewData["Titulo"] = "Gestión de Perfiles";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPerfiles(QueryFilters query, bool buscaNew, string sort = "p_desc", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<PerfilDto> lista;
            MetadataGrid metadata;
            GridCore<PerfilDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaActual == pag && !buscaNew && PerfilesBuscados.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = PerfilesBuscados.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    PerfilesBuscados = lista;
                }
                else
                {
                    PaginaActual = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _mnSrv.GetPerfiles(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    PerfilesBuscados = lista;
                }
                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrilla(PerfilesBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridPerfiles", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda de Perfiles";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda de Perfiles");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarPerfil(string id)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                RespuestaGenerica<PerfilDto> per = await GetPerfil(id);
                if (!per.Ok)
                {
                    throw new NegocioException(per.Mensaje);
                }
                //inicializo barrados
                UsuariosXPerfil = [];
                PerfilIDSeleccionado = per.Entidad.perfil_id;
                PerfilSeleccionado = new PerfilDto
                {
                    perfil_activo = per.Entidad.perfil_activo,
                    perfil_activo_desc = per.Entidad.perfil_activo_desc,
                    Perfilactivo = per.Entidad.Perfilactivo,
                    perfil_descripcion = per.Entidad.perfil_descripcion,
                    perfil_id = per.Entidad.perfil_id
                };


                return View("_n02panel01Perfil", PerfilSeleccionado);

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
                string msg = "Error en la invocación de la API - Busqueda Producto";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda Producto");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        private async Task<RespuestaGenerica<PerfilDto>> GetPerfil(string id)
        {
            return await _mnSrv.GetPerfil(id, TokenCookie);
        }

        [HttpPost]
        public async Task<IActionResult> buscarPerfilUsers(string id)
        {
            RespuestaGenerica<EntidadBase> response = new();
            GridCore<PerfilUserDto> grillaDatos;

            try
            {
                var per = await _mnSrv.GetPerfilUsers(id, TokenCookie);
                if (!per.Ok)
                {
                    throw new NegocioException(per.Mensaje);
                }
               
                UsuariosXPerfil = per.ListaEntidad;
                grillaDatos = GenerarGrilla(UsuariosXPerfil, "usu_apellidoynombre");


                return View("_gridPerfilUsers", grillaDatos);

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
        public IActionResult NuevoPerfil()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                string id = "000";
                PerfilSeleccionado = new PerfilDto() { perfil_activo = 'S', perfil_id = id };

                return View("_n02panel01Perfil", PerfilSeleccionado);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Inicializar Perfil";
                _logger.LogError(ex, "Error en la invocación de la API - al Inicializar Perfil");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }


        public async Task<IActionResult> ConfirmaPerfil(PerfilDto perfil, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                perfil = HelperGen.PasarAMayusculas(perfil);

                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(perfil),
                    Objeto = "Perfil",
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
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL PERFIL {perfil.perfil_descripcion} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL PERFIL {perfil.perfil_descripcion} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA/DISCONTINUAR DEL PERFIL {perfil.perfil_descripcion} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    PerfilesBuscados = [];
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
        public async Task<JsonResult> ObtenerMenuPerfil(string menuId, string perfil)
        {
            RespuestaGenerica<EntidadBase> response = new();
            List<MenuRoot> arbol;
            string _perfil = "";
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                var p = PerfilSeleccionado;
                if (p == null || string.IsNullOrEmpty(p.perfil_id))
                {
                    var per = PerfilIDSeleccionado;
                    if (string.IsNullOrEmpty(per))
                    {
                        if (string.IsNullOrEmpty(perfil))
                        {


                            throw new NegocioException("No se ha localizado el identificador del perfil. Verifique. De ser necesario reinicie el módulo.");
                        }
                        else
                        {
                            _perfil = perfil;
                        }
                    }
                    else
                    {
                        _perfil = per;
                    }
                }
                else
                {
                    _perfil = p.perfil_id;
                }
                //Procesamos la lista que nos devuelve y la convertimos en la estructura del menu
                var menu = await _mnSrv.GetMenuItems(menuId, _perfil, TokenCookie);

                if (!menu.Ok)
                {
                    throw new NegocioException(menu.Mensaje);
                }

                arbol = GenerarArbolMenu(menu.ListaEntidad);
                var jarbol = JsonConvert.SerializeObject(arbol);

                return Json(new { error = false, warn = false, arbol = jarbol });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });

            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Menu x Perfil";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda  Menu x Perfil");
                return Json(new { error = true, warn = false, msg });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarMenuPerfil(string json, string menu_id, string perfil_id)
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
                    string msg = "No se recepcionó el menú del perfil a confirmar. Verifique.";
                    return Json(new { error = false, warn = true, msg });
                }
                //Se procede a generar estructura esperada en Base de Datos a partir del menu 
                List<MenuItemsDto> menuPlano = GeneraMenuPlano(json, menu_id, perfil_id);
                var jsonp = JsonConvert.SerializeObject(menuPlano);
                _logger.LogInformation("#***************************************#");
                _logger.LogInformation(json);
                _logger.LogInformation(jsonp);
                _logger.LogInformation("#***************************************#");
                //armando request del confirmar
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = jsonp,
                    Objeto = "perfiles_items",
                    Administracion = AdministracionId,
                    Usuario = UserName,
                    Abm = 'A'
                };

                var perfil = PerfilSeleccionado;
                var res = await _abmSv.AbmConfirmar(abm, TokenCookie);
                if (res.Ok)
                {
                    string msg = $"EL PROCESAMIENTO DE LOS PERMISO DE ACCESO AL PERFIL {perfil.perfil_descripcion} SE REALIZO SATISFACTORIAMENTE";

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
                string msg = "Error en la invocación de la API - Busqueda Menu x Perfil";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda  Menu x Perfil");
                return Json(new { error = true, warn = false, msg });
            }
        }

        /// <summary>
        /// se asume que el menu tiene 2 niveles (Root - hijo) y por lo tanto se parseará 
        /// no recursivamente, sino detectando hijos y definiendo para cada nivel un foreach.
        /// </summary>
        /// <param name="djson"></param>
        /// <returns></returns>
        private List<MenuItemsDto> GeneraMenuPlano(string json, string menu_id, string perfil_id)
        {
            var menuP = new List<MenuItemsDto>();

            var menu = JsonConvert.DeserializeObject<List<MenuRoot>>(json);


            if (menu == null)
            {
                throw new NegocioException("No se recepcionó ningún menú");
            }

            foreach (var item in menu)
            {
                MenuItemsDto i = ParseaItem(item, menu_id, perfil_id);
                menuP.Add(i);
                if (item.children.Count > 0)
                {
                    foreach (var child in item.children)
                    {
                        var c = ParseaItem(child, menu_id, perfil_id);
                        menuP.Add(c);
                    }
                }
            }
            return menuP;
        }

        private static MenuItemsDto ParseaItem(MenuRoot item, string mnId, string perfil)
        {
            return new MenuItemsDto
            {
                asignado = item.state.selected,
                mnu_id = mnId,
                mnu_item = item.id,
                mnu_item_id = item.data.mnu_item_id,
                mnu_item_name = item.text,
                mnu_item_padre = item.data.mnu_item_padre,
                perfil_id = perfil,
            };
        }

        private List<MenuRoot> GenerarArbolMenu(List<MenuItemsDto>? listaEntidad)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            foreach (var item in listaEntidad)
            {
                //busco si es opcion root
                if (item.mnu_item_padre.Equals("00"))
                {
                    arbol.Add(CargaItem(item));
                }
                else
                {
                    //busco cual es el padre
                    MenuRoot rama = BuscarNodoPadre(arbol, item.mnu_item_padre);
                    
                    if (rama.children == null)
                    {
                        rama.children = new List<MenuRoot>();
                    }
                    rama.children.Add(CargaItem(item));
                }
            }

            //debo recorrer el arbol armado para deterctar marcas e hijos
            //1-si tiene marca asignado y no tiene hijos queda asignado..
            //2-si tiene marca asignado y tiene hijos, se pone en false el asignado del root. 
            //el jstree asignara valor al root dependiendo del valor asignado de los hijos.
            foreach (var item in arbol)
            {
                if (item.children.Count() > 0)
                {
                    item.data.asignado = false;
                    item.state.selected = false;
                }
            }

            return arbol;
        }

        private MenuRoot BuscarNodoPadre(List<MenuRoot> arbol, string mnu_item_padre)
        {
            foreach (var nodo in arbol)
            {
                if (nodo.id.Equals(mnu_item_padre))
                {
                    return nodo;
                }
                else
                {
                    if (nodo.children.Count() > 0)
                    {
                        var nodoPadre = BuscarNodoPadre(nodo.children, mnu_item_padre);
                        if (nodoPadre != null)
                        {
                            return nodoPadre;
                        }
                    }
                }

            }
            return null;
        }

        private static MenuRoot CargaItem(MenuItemsDto item)
        {
            return new MenuRoot
            {
                id = item.mnu_item,
                text = item.mnu_item_name,

                state = new Estado
                {
                    opened = true,
                    selected = item.asignado,
                    disabled = true,
                },
                data = new MenuRootData
                {
                    mnu_item_id = item.mnu_item_id,
                    asignado = item.asignado,
                    mnu_item_padre = item.mnu_item_padre,
                }

            };
        }
    }
}

