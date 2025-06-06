﻿using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Areas.Usuarios.Controllers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Vendedor
{
    [Area("ABMs")]
    public class ABMVendedorController : ControladorVendedorBase
    {
        private readonly AppSettings _settings;
        private readonly ILogger<ABMVendedorController> _logger;
        private readonly IAbmServicio _abmSv;
        private readonly IABMVendedorServicio _abmveSv;

        public ABMVendedorController(IOptions<AppSettings> options, ILogger<ABMVendedorController> logger,
             IHttpContextAccessor accessor, IAbmServicio abmServicio, IABMVendedorServicio abmveSv,
             IAbmServicio abmSv) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _logger = logger;
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

                VendedoresBuscardo = [];

                ViewData["Titulo"] = "Gestión de Vendedores";
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
            List<ABMVendedorDto> lista;
            MetadataGrid metadata;
            GridCoreSmart<ABMVendedorDto> grillaDatos;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                if (PaginaGrid == pag && !buscaNew && VendedoresBuscardo.Count() > 0)
                {
                    //es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
                    lista = VendedoresBuscardo.ToList();
                    lista = OrdenarEntidad(lista, sortDir, sort);
                    VendedoresBuscardo = lista;
                }
                else
                {
                    PaginaGrid = pag;
                    //traemos datos desde la base
                    query.Sort = sort;
                    query.SortDir = sortDir;
                    query.Registros = _settings.NroRegistrosPagina;
                    query.Pagina = pag;

                    var res = await _abmveSv.ObtenerVendedores(query, TokenCookie);
                    lista = res.Item1 ?? [];
                    MetadataGeneral = res.Item2 ?? new MetadataGrid();
                    VendedoresBuscardo = lista;
                }
                metadata = MetadataGeneral;

                //no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
                grillaDatos = GenerarGrillaSmart(VendedoresBuscardo, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

                //string volver = Url.Action("index", "home", new { area = "" });
                //ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

                return View("_gridVendedor", grillaDatos);
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda de Vendedores";
                _logger?.LogError(ex, "Error en la invocación de la API - Busqueda de Vendedores");
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> BuscarVendedorDatos(string id)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var ve = await _abmveSv.ObtenerVendedorPorId(id, TokenCookie);
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
                VendedorSeleccionado = ve.Entidad;

                return View("_n02panel01Vendedor", ve.Entidad);

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
        public IActionResult NuevoVendedor()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var ve = new ABMVendedorDatoDto() { ve_activo='S'};
                    
                VendedorSeleccionado = ve;

                return View("_n02panel01Vendedor", ve);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Inicializar Vendedor";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return PartialView("_gridMensaje", response);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmVendedor(ABMVendedorDatoDto ve, char accion)
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
                    Objeto = "vendedores",
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
                            msg = $"EL PROCESAMIENTO DEL ALTA DEL VENDEDOR {ve.ve_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DEL VENDEDOR {ve.ve_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA DEL VENDEDOR {ve.ve_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    VendedoresLista = [];

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
