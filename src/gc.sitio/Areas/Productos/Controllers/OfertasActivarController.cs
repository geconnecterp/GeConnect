using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class OfertasActivarController : ControladorOfertaBase
    {
        private readonly AppSettings        _configuracion;
        private readonly IOfertaServicio    _ofertaServicio;
        private readonly ICuentaServicio    _cuentaServicio;
        private readonly IRubroServicio     _rubroServicio;

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
        public async Task<IActionResult> PresentarOfertasSinActivar(string admId="0000",string lp_id="001", int pag = 1)
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
                var ofertas = respuesta.ListaEntidad;
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
                return View("_gridOfertaActivar",grid);
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
    }
}
