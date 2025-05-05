using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.ABMs.Controllers.Zona;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Zona
{
    [Area("ABMs")]
    public class ABMZonaController : ControladorZonaBase
    {
        private readonly AppSettings _settings;
        private readonly IAbmServicio _abmSv;
        private readonly IABMZonaServicio _abmveSv;

        public ABMZonaController(IOptions<AppSettings> options, ILogger<ABMZonaController> logger,
             IHttpContextAccessor accessor, IAbmServicio abmServicio, IABMZonaServicio abmveSv,
             IAbmServicio abmSv) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _abmveSv = abmveSv;
            _abmSv = abmSv;
        }
        public async Task<IActionResult> Index(bool actualizar)
        {

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                ZonasBuscardo = [];

                ViewData["Titulo"] = "Gestión de Zonaes";
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "home", new { area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "ve_nombre", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<ABMZonaDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<ABMZonaDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaGrid == pag && !buscaNew && ZonasBuscardo.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = ZonasBuscardo.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    ZonasBuscardo = lista;
                }
                else
                {
                    PaginaGrid = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _abmveSv.ObtenerZonas(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    ZonasBuscardo = lista;
                }
                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(ZonasBuscardo, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridZona", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda de Zonaes";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Zonaes");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarZonaDatos(string id)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var ve = await _abmveSv.ObtenerZonaPorId(id, TokenCookie);
                if (ve == null || !ve.Ok)
                {
                    if (ve == null)
                    {
                        throw new NegocioException("No se recepcionó el usuario buscado.");
                    }
                    else
                    {
                        throw new NegocioException(ve.Mensaje);
                    }
                }
                ZonaSeleccionada = ve.Entidad;

                return View("_n02panel01Zona", ve.Entidad);

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
        public IActionResult NuevoZona()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var ve = new ZonaDto();
                    
                ZonaSeleccionada = ve;

                return View("_n02panel01Zona", ve);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Inicializar Zona";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmZona(ZonaDto ve, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                ve = HelperGen.PasarAMayusculas(ve);
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(ve),
                    Objeto = "zonas",
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
                            msg = $"EL PROCESAMIENTO DEL ALTA DE LA ZONA {ve.zn_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DE LA ZONA {ve.zn_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA DE LA ZONA {ve.zn_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    ZonasLista = [];

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

    }
}
