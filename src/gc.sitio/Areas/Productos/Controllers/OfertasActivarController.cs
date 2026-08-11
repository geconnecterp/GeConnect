using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasActivarController : ControladorOfertaBase
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

        public OfertasActivarController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio,
            ICuentaServicio cuenta, IRubroServicio rubro, IOptions<DocsManager> docsManager,
            IDocManagerServicio docManagerServicio)
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
            //para la impresion
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

                OfertasSinActivar = [];

                string titulo = "Ofertas sin Activar";
                ViewData["Titulo"] = titulo;
                #region Gestor Impresion - Inicializacion de variables

                //Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
                DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
                ViewBag.ImpresionId = _modulo.Reportes[0].Id; //siempre el primer 

                _logger?.LogInformation($"Generando Arbol de Archivos del módulo. {MethodBase.GetCurrentMethod()?.Name}");

                //en este mismo acto se cargan los posibles documentos
                //que se pueden imprimir, exportar, enviar por email o whatsapp
                ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

                #endregion

                return View();
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas sin Activar");
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de Ofertas sin Activar");
                TempData["error"] = "Hubo un problema al cargar la vista de Ofertas sin Activar. Si el problema persiste, contacte al administrador.";
                return View();
            }
        }


        [HttpPost]
        public async Task<IActionResult> PresentarOfertasSinActivar(string admId = "0000", string lp_id = "001", int pag = 1)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                RespuestaGenerica<OfertaDto> respuesta = await _ofertaServicio.ObtenerOfertasSinActivar(admId, lp_id, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    TempData["error"] = respuesta.Mensaje ?? "Error al obtener ofertas sin activar";
                    throw new NegocioException(respuesta.Mensaje ?? "Error al obtener ofertas sin activar");
                }
             

                OfertasSinActivar = respuesta.ListaEntidad ?? [];

                var ofertas = OfertasSinActivar;
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
                    DatoAux01 = $"Ofertas sin activar cargadas: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridOfertaActivar", grid);
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
        public async Task<JsonResult> ActualizarOfertaVencidaSinActivar(string admId, string lp_id)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });
                AbmGenDto req = new AbmGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.ActualizarOfertaVencidaSinActivar(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al actualizar las ofertas vencidas sin activar");
                }
                OfertasSinActivar = [];
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = string.IsNullOrEmpty(respuesta.Mensaje) ? "Ofertas actualizadas correctamente." : respuesta.Mensaje,
                    adm_Id = admId,
                    lp_id
                });
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al actualizar ofertas vencidas sin activar");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al actualizar ofertas vencidas sin activar");
                return Json(new { error = false, warn = true, msg = "Error interno al actualizar ofertas vencidas sin activar" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> CargarActivosASinActivar(string admId, string lp_id)
        {
            string msg = "Error interno al Cargar los Activos a sin activar";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });
                AbmGenDto req = new AbmGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.CargarActivasASinActivar(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al Cargar los Activos a sin activar");
                }
                OfertasSinActivar = [];
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = string.IsNullOrEmpty(respuesta.Mensaje) ? "Carga realizada correctamente." : respuesta.Mensaje,
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
                return Json(new { error = false, warn = true, msg});
            }
        }

        [HttpPost]
        public async Task<JsonResult> ActivarOferta(List<string> ids, string admId, string lp_id)
        {
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });


                if (ids == null || ids.Count == 0)
                {
                    return Json(new { error = true, msg = "Debe al menos seleccionar una oferta para activar." });
                }

                var idsSolicitados = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
                var estadoActual = await _ofertaServicio.ObtenerOfertasSinActivar(admId, lp_id, TokenCookie);
                if (!estadoActual.Ok || estadoActual.EsError)
                    return Json(new { error = true, msg = estadoActual.Mensaje ?? "No se pudo validar el estado actual de las ofertas." });

                var lista = estadoActual.ListaEntidad ?? [];
                var ofertasSeleccionadas = lista
                    .Where(o => idsSolicitados.Contains(o.p_id) && o.adm_id == admId && o.lp_id == lp_id)
                    .ToList();

                if (ofertasSeleccionadas.Count != idsSolicitados.Count)
                    return Json(new { error = true, msg = "La selección ya no coincide con el canal consultado. Actualice la grilla e intente nuevamente." });

                var ofertas = ofertasSeleccionadas.Select(p => new { p_id = p.p_id }).ToList();

                AbmPlusGenDto req = new AbmPlusGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Json = JsonConvert.SerializeObject(ofertas),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.ActivacionDeOferta(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al activar la oferta");
                }

                OfertasSinActivar = lista
                    .Where(o => !ofertasSeleccionadas.Any(s => s.p_id == o.p_id && s.adm_id == o.adm_id && s.lp_id == o.lp_id))
                    .ToList();
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = idsSolicitados.Count == 1
                        ? "La oferta se activó exitosamente."
                        : $"Las {idsSolicitados.Count} ofertas se activaron exitosamente.",
                    adm_Id = admId,
                    lp_id
                });

            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error interno al activar oferta");
                return Json(new { error = false, warn = true, msg = ex.Message });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error interno al activar oferta");
                return Json(new { error = false, warn = true, msg = "Error interno al activar oferta" });
            }
        }

        [HttpPost]
        public async Task<JsonResult> EliminarOfertasSinActivar(List<string> ids, string admId, string lp_id)
        {
            string msg = "Error interno al eliminar ofertas";
            try
            {
                // ✅ VALIDACIÓN: Autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return Json(new { error = true, msg = "Sesión expirada" });


                if (ids == null || ids.Count == 0)
                {
                    return Json(new { error = true, msg = "Debe al menos seleccionar una oferta para eliminar." });
                }

                var idsSolicitados = ids.Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
                var estadoActual = await _ofertaServicio.ObtenerOfertasSinActivar(admId, lp_id, TokenCookie);
                if (!estadoActual.Ok || estadoActual.EsError)
                    return Json(new { error = true, msg = estadoActual.Mensaje ?? "No se pudo validar el estado actual de las ofertas." });

                var lista = estadoActual.ListaEntidad ?? [];
                var ofertasSeleccionadas = lista
                    .Where(o => idsSolicitados.Contains(o.p_id) && o.adm_id == admId && o.lp_id == lp_id)
                    .ToList();

                if (ofertasSeleccionadas.Count != idsSolicitados.Count)
                    return Json(new { error = true, msg = "La selección ya no coincide con el canal consultado. Actualice la grilla e intente nuevamente." });

                var ofertas = ofertasSeleccionadas.Select(p => new { p_id = p.p_id }).ToList();

                AbmPlusGenDto req = new AbmPlusGenDto
                {
                    Objeto = $"{admId}#{lp_id}",
                    Json = JsonConvert.SerializeObject(ofertas),
                    Usuario = UserName,
                    Administracion = AdministracionId
                };
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.EliminarOfertas(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al Eliminar la(s) oferta(s)");
                }

                OfertasSinActivar = lista
                    .Where(o => !ofertasSeleccionadas.Any(s => s.p_id == o.p_id && s.adm_id == o.adm_id && s.lp_id == o.lp_id))
                    .ToList();
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
