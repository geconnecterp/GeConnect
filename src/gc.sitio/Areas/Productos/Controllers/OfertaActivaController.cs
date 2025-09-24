using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertaActivaController : ControladorOfertaBase
    {
        // variables para manerjar modulo de impresión
        private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
        private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
        private string APP_MODULO = AppModulos.OF_SINACT.ToString();
        private readonly IDocManagerServicio _docMSv;

        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public OfertaActivaController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio,
            ICuentaServicio cuenta, IRubroServicio rubro, IOptions<DocsManager> docsManager,
            IDocManagerServicio docManagerServicio) : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
            _docsManager = docsManager.Value;
            _modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
            _docMSv = docManagerServicio;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                //DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);

                //_logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                ////en este mismo acto se cargan los posibles documentos
                ////que se pueden imprimir, exportar, enviar por email o whatsapp
                //ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                OfertasActivas = [];

                ViewData["Titulo"] = "Ofertas Activas";
                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas Activas");
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas Activas");
                TempData["error"] = "Hubo un problema al cargar la vista de Ofertas Activas. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> PresentarOfertasActivas(string admId = "0000", string lp_id = "001", int pag = 1)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                RespuestaGenerica<OfertaDto> respuesta = await _ofertaServicio.ObtenerOfertasActivas(admId, lp_id, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    TempData["error"] = respuesta.Mensaje ?? "Error al obtener ofertas sin activar";
                    return View();
                }
                if (respuesta.ListaEntidad == null || !respuesta.ListaEntidad.Any())
                {
                    TempData["warning"] = "No se encontraron ofertas sin activar";
                    return View();
                }

                OfertasActivas = respuesta.ListaEntidad;

                var ofertas = OfertasActivas;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<OfertaDto>(
                    ofertas.OrderBy(o => o.p_desc).ToList(),
                    pag,
                    registrosPorPagina,
                    ofertas.Count
                );
                var grid = new GridCoreSmart<OfertaDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = ofertas.Count,
                    PrimerRegistro = ((pag - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(pag * registrosPorPagina, ofertas.Count),
                    RegistroFinal = ofertas.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)ofertas.Count / registrosPorPagina),
                    PaginaActual = pag,
                    Sort = "p_desc",
                    SortDir = "ASC",
                    DatoAux01 = $"Ofertas activas cargadas: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridOfertaActiva", grid);
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas sin activar");
                return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al cargar ofertas sin activar");
                return PartialView("_gridMensaje", CrearRespuestaError("Error interno al cargar ofertas sin activar"));
            }
        }

        [HttpPost]
        public async Task<JsonResult> EliminarOfertasActivas(List<string> ids, string admId, string lp_id)
        {
            string msg = "Error interno al eliminar ofertas activas";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });


                if (ids == null)
                {
                    return Json(new { error = true, msg = "Debe al menos seleccionar una oferta para eliminar." });
                }
                //var productosIds = OfertasSinActivar.Where(x=>ids.Contains(x.p_id)).Select(p => new { p_id = p.p_id }).ToList();
                var lista = OfertasActivas;
                var ofertas = lista.Where(o => ids.Contains(o.p_id)).Select(p => new { p_id = p.p_id }).ToList();

                AbmPlusGenDto req = new AbmPlusGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Json = JsonConvert.SerializeObject(ofertas),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.EliminaOfertasActivas(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al Eliminar la(s) oferta(s)");
                }
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = ids.Count == 1 ?
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Oferta Eliminada correctamente." : respuesta.Mensaje :
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Ofertas Eliminadas correctamente." : respuesta.Mensaje,
                    adm_Id = admId,
                    lp_id
                });

            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = false, warn = true, msg });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CopiarACanal(List<string> ids, string admId, string lp_id)
        {
            string msg = "Error interno al eliminar ofertas activas";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });


                if (ids == null)
                {
                    return Json(new { error = true, msg = "Debe al menos seleccionar una oferta para eliminar." });
                }
                //var productosIds = OfertasSinActivar.Where(x=>ids.Contains(x.p_id)).Select(p => new { p_id = p.p_id }).ToList();
                var lista = OfertasSinActivar;
                var ofertas = lista.Where(o => ids.Contains(o.p_id)).Select(p => new { p_id = p.p_id }).ToList();

                AbmPlusGenDto req = new AbmPlusGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Json = JsonConvert.SerializeObject(ofertas),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.CopiarACanal(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al Eliminar la(s) oferta(s)");
                }
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = ids.Count == 1 ?
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Oferta Eliminada correctamente." : respuesta.Mensaje :
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Ofertas Eliminadas correctamente." : respuesta.Mensaje,
                    adm_Id = admId,
                    lp_id
                });

            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, msg);
                return Json(new { error = false, warn = true, msg });
            }
        }
    }
}
