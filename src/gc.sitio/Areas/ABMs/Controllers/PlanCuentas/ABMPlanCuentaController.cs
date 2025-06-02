using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Contabilidad;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.PlanCuenta
{
    [Area("ABMs")]
    public class ABMPlanCuentaController : ControladorPlanCuentaBase
    {
        private readonly AppSettings _settings;
        private readonly IAbmServicio _abmSv;
        private readonly IABMPlanCuentaServicio _pcuentaSv;
        private readonly ICompositeViewEngine _viewEngine;

        public ABMPlanCuentaController(IOptions<AppSettings> options, ILogger<ABMPlanCuentaController> logger,
             IHttpContextAccessor accessor, IAbmServicio abmServicio, IABMPlanCuentaServicio pcSv,
             IAbmServicio abmSv, ICompositeViewEngine viewEngine) : base(options, accessor, logger)
        {
            _settings = options.Value;
            _pcuentaSv = pcSv;
            _abmSv = abmSv;
            _viewEngine = viewEngine;
        }
        public IActionResult Index(bool actualizar)
        {

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return RedirectToAction("Login", "Token", new { area = "seguridad" });
                }

                PlanCuentasLista = [];

                ViewData["Titulo"] = "Gestión de Plan de Cuentas";
                return View();
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                return RedirectToAction("Index", "home", new { area = "" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "ccb_desc", string sortDir = "asc", int pag = 1, bool actualizar = false)
        {
            List<MenuRoot> arbol;
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }
                //traemos datos desde la base
                query.Sort = sort;
                query.SortDir = sortDir;
                //para el plan de cuentas necesito ver todos los registros. Actualmente son 1617
                //se arma el arbol.
                query.Registros = 9999;// _settings.NroRegistrosPagina; 
                query.Pagina = pag;

                var pc = await _pcuentaSv.ObtenerPlanCuentas(query, TokenCookie);
                if (pc.Item1.Count > 0)
                {
                    PlanCuentasLista = pc.Item1;
                }
                else
                {
                    throw new NegocioException("No se encontró el Plan de Cuentas. Verificar.");
                }

                arbol = GenerarArbolPlanCuenta(pc.Item1);
                var jarbol = JsonConvert.SerializeObject(arbol);
                return Json(new { error = false, warn = false, arbol = jarbol });
            }
            catch (Exception ex)
            {

                string msg = "Error en la invocación de la API - Busqueda del Plan de Cuentas";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return Json(new { error = true, warn = false, msg = PartialView("_gridMensaje", response) });
            }
        }

        [HttpPost]
        public async Task<JsonResult> BuscarCuenta([FromBody] RequestId req)
        {
            RespuestaGenerica<EntidadBase> response = new();

            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                var cta = await _pcuentaSv.ObtenerCuentaPorId(req.Dato, TokenCookie);

                if (!cta.Ok )
                {
                    throw new NegocioException(cta.Mensaje);
                }
                if(cta.Entidad== null)
                {
                    throw new NegocioException("No se logró obtener la cuenta solicitada. Verifique");
                }
                CuentaSeleccionada = cta.Entidad;
                var cuenta = RenderPartialViewToString("_cuenta", cta.Entidad, _viewEngine);
                return Json(new { error = false, warn = false, cuenta, entidad = cta.Entidad});

            }
            catch (NegocioException ex)
            {
                response.Mensaje = ex.Message;
                response.Ok = false;
                response.EsWarn = true;
                response.EsError = false;
                return Json(new { error = false, warn = true, msg = PartialView("_gridMensaje", response) });
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Busqueda de la Cuenta.";
                _logger?.LogError(ex, msg);
                response.Mensaje = msg;
                response.Ok = false;
                response.EsWarn = false;
                response.EsError = true;
                return Json(new { error = true, warn = false, msg = PartialView("_gridMensaje", response) });
            }
        }

        [HttpPost]
        public JsonResult NuevaCuenta()
        {
            RespuestaGenerica<EntidadBase> response = new();
            try
            {
                var ctaAct = CuentaSeleccionada;
                if(ctaAct.ccb_tipo.Equals('M'))
                {
                    throw new NegocioException("No se puede crear una cuenta hija de una cuenta movimiento.");
                }
                var nuevoId = ObtenerParteSignificativa(ctaAct.ccb_id);
                var cta = new PlanCuentaDto()
                {
                    id_padre = nuevoId,                    
                    ccb_id_padre = ctaAct.ccb_id
                };

                var cuenta = RenderPartialViewToString("_cuenta", cta, _viewEngine);
                return Json(new { error = false, warn = false, cuenta });
            }
            catch (NegocioException ex)
            {
                return Json(new { error = false, warn = true, msg = ex.Message });
                //_logger?.LogError(ex, ex.Message);
                //response.Mensaje = ex.Message;
                //response.Ok = false;
                //response.EsWarn = true;
                //response.EsError = false;
                //return PartialView("_gridMensaje", response);
            }
            catch (Exception ex)
            {
                string msg = "Error en la invocación de la API - Nueva CUENTA";
                _logger?.LogError(ex, msg);
                return Json(new { error = false, warn = true, msg });
                //response.Mensaje = msg;
                //response.Ok = false;
                //response.EsWarn = false;
                //response.EsError = true;
                //return PartialView("_gridMensaje", response);
            }
        }

        private string ObtenerParteSignificativa(string ccb_id)
        {
            if (string.IsNullOrEmpty(ccb_id))
            {
                throw new ArgumentException("El ID del padre no puede ser nulo o vacío.");
            }

            // Elimina los ceros finales del ID
            string parteSignificativa = ccb_id.TrimEnd('0');

            return parteSignificativa;
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmarAbmCuenta(PlanCuentaDto pc, char accion)
        {
            try
            {
                var auth = EstaAutenticado;
                if (!auth.Item1 || auth.Item2 < DateTime.Now)
                {
                    return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
                }

                pc = HelperGen.PasarAMayusculas(pc);
                //prod.P_Obs = prod.P_Obs.ToUpper();
                AbmGenDto abm = new AbmGenDto()
                {
                    Json = JsonConvert.SerializeObject(pc),
                    Objeto = "plancontable",
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
                            msg = $"EL PROCESAMIENTO DEL ALTA DE LA CUENTA {pc.ccb_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        case 'M':
                            msg = $"EL PROCESAMIENTO DE LA MODIFICIACION DE LA CUENTA {pc.ccb_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                        default:
                            msg = $"EL PROCESAMIENTO DE LA BAJA DE LA CUENTA {pc.ccb_id} SE REALIZO SATISFACTORIAMENTE";
                            break;
                    }
                    PlanCuentasLista = [];

                    if (abm.Abm.Equals('A'))
                    {
                        return Json(new { error = false, warn = false, msg, id = res.Entidad?.resultado_id });
                    }
                    return Json(new { error = false, warn = false, msg });
                }
                else
                {
                    return Json(new { error = false, warn = true, msg = res.Entidad?.resultado_msj, focus = res.Entidad?.resultado_setfocus });
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

        #region Método privado para la generacion del menu
        private List<MenuRoot> GenerarArbolPlanCuenta(List<PlanCuentaDto> listaEntidad)
        {
            List<MenuRoot> arbol = new List<MenuRoot>();
            foreach (var item in listaEntidad)
            {
                //busco si es opcion root
                if (item.ccb_id_padre.Equals("00000000"))
                {
                    arbol.Add(CargaItem(item));
                }
                else
                {
                    //busco cual es el padre
                    MenuRoot? rama = BuscarNodoPadre(arbol, item.ccb_id_padre);
                    if (rama == null)
                    {
                        continue;
                    }
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

        private MenuRoot? BuscarNodoPadre(List<MenuRoot> arbol, string item_padre)
        {
            foreach (var nodo in arbol)
            {
                if (nodo.id.Equals(item_padre))
                {
                    return nodo;
                }
                else
                {
                    if (nodo.children.Count() > 0)
                    {
                        var nodoPadre = BuscarNodoPadre(nodo.children, item_padre);
                        if (nodoPadre != null)
                        {
                            return nodoPadre;
                        }
                    }
                }

            }
            return null;
        }

        private static MenuRoot CargaItem(PlanCuentaDto item)
        {
            return new MenuRoot
            {
                id = item.ccb_id,
                text = item.ccb_lista,
                type = item.ccb_tipo.ToString(),
                state = new Estado
                {
                    opened = false,
                    selected = false,
                    disabled = false,
                },
                data = new MenuRootData
                {
                    item_padre = item.ccb_id_padre,
                    tipo = item.ccb_tipo.ToString(),
                    ajuste_inflacion = item.ccb_ajuste_inflacion.ToString(),
                    saldo = item.ccb_saldo,
                    cuenta = DistingueCuenta(item.ccb_id)
                }

            };
        }

        private static string DistingueCuenta(string ccb_id)
        {
            var cta = ccb_id.Substring(0, 1);
            switch (cta)
            {
                case "1":
                    return "activo";
                case "2":
                    return "pasivo";
                case "3":
                    return "patrimonio";
                case "4":
                    return "ingresos";
                case "5":
                    return "egresos";
                default:
                    return "";
            }
        }
        #endregion
    }
}
