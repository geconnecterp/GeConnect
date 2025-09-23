using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasActivarController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;
        private readonly IOfertaServicio _ofertaServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IRubroServicio _rubroServicio;

        public OfertasActivarController(IOptions<AppSettings> options, IHttpContextAccessor contexo,
            ILogger<OfertasController> logger, IOfertaServicio ofertaServicio,
            ICuentaServicio cuenta, IRubroServicio rubro)
            : base(options, contexo, logger)
        {
            _configuracion = options.Value;
            _ofertaServicio = ofertaServicio;
            _cuentaServicio = cuenta;
            _rubroServicio = rubro;
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                ViewData["Titulo"] = "Ofertas Sin Activar";

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
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
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

                RespuestaGenerica<OfertaSinActivarDto> respuesta = await _ofertaServicio.ObtenerOfertasSinActivar(admId, lp_id, TokenCookie);
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

                OfertasSinActivar = respuesta.ListaEntidad;

                var ofertas = OfertasSinActivar;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<OfertaSinActivarDto>(
                    ofertas.OrderBy(o => o.p_desc).ToList(),
                    pag,
                    registrosPorPagina,
                    ofertas.Count
                );
                var grid = new GridCoreSmart<OfertaSinActivarDto>
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


                if (ids == null)
                {
                    return Json(new { error = true, msg = "Debe al menos seleccionar una oferta para activar." });
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
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.ActivacionDeOferta(req, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    throw new NegocioException(respuesta.Mensaje ?? "Error al activar la oferta");
                }
                return Json(new
                {
                    error = false,
                    warn = false,
                    msg = ids.Count == 1 ?
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Oferta Activada correctamente." : respuesta.Mensaje :
                            string.IsNullOrEmpty(respuesta.Mensaje) ? "Ofertas activadas correctamente." : respuesta.Mensaje,
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
                RespuestaGenerica<RespuestaDto> respuesta = await _ofertaServicio.EliminarOfertas(req, TokenCookie);
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
