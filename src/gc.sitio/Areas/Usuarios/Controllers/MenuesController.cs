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
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

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
            //se carga el combo de tipos de menues
            ViewBag.MenuId = await ComboMenues(_mnSrv);

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
                var per = await _mnSrv.GetPerfil(id, TokenCookie);
                if (!per.Ok)
                {
                    throw new NegocioException(per.Mensaje);
                }
                //inicializo barrados
                UsuariosXPerfil = [];

                PerfilSeleccionado = per.Entidad;

                return View("_n02panel01Perfil", per.Entidad);

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
                //inicializo barrados
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
        public async Task<JsonResult> ObtenerMenuPerfil(string menuId)
        {
            RespuestaGenerica<EntidadBase> response = new();
            List<MenuRoot> arbol;
            try
            {

                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                //Procesamos la lista que nos devuelve y la convertimos en la estructura del menu
                var menu = await _mnSrv.GetMenuItems(menuId, PerfilSeleccionado.perfil_id, TokenCookie);

                if (!menu.Ok)
                {
                    throw new NegocioException(menu.Mensaje);
                }

                arbol = GenerarArbolMenu(menu.ListaEntidad);

                return Json(new { error = false, warn = false, arbol });

            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
              
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda Menu x Perfil";
                _logger.LogError(ex, "Error en la invocación de la API - Busqueda  Menu x Perfil");
                return Json(new { error = true, warn = false, msg  });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmarMenuPerfil(string json)
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
                    return Json(new { error = false,warn=true,msg});
                }

                AbmGenDto abm = new AbmGenDto()
                {
                    Json = json,
                    Objeto = "Perfil",
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

        private List<MenuRoot> GenerarArbolMenu(List<MenuItemsDto>? listaEntidad)
        {
            List<MenuRoot> arbol= new List<MenuRoot>();
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
                    var rama = arbol.Single(x=>x.id.Equals(item.mnu_item_padre));
                    if (rama.children == null)
                    {
                        rama.children = new List<MenuRoot>();
                    }
                    rama.children.Add(CargaItem(item));
                }
            }

            return arbol;
        }

        private static MenuRoot CargaItem(MenuItemsDto item)
        {
            return new MenuRoot
            {
                id = item.mnu_item,
                text = item.mnu_item_name,
                ruta = item.mnu_item_id,
                asignado = item.asignado                
            };
        }
    }
}

