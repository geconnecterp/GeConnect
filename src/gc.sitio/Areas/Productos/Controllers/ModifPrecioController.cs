using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Ofertas;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class ModifPrecioController : ControladorOfertaBase
    {
        private readonly AppSettings _configuracion;
        private readonly IProducto2Servicio _prodSv;

        public ModifPrecioController(IOptions<AppSettings> options, 
            IHttpContextAccessor contexo,
            ILogger<OfertasController> logger,
            IProducto2Servicio prodSv) : base(options, contexo, logger)
        {
            _prodSv = prodSv;
            _configuracion = options.Value;
        }
        public IActionResult Index()
        {
            // Versión optimizada del código de autenticación
            if (!VerificarAutenticacion(out IActionResult redirectResult))
                return redirectResult;

            string titulo = "Modificaciones de Precios en Menos";
            ViewData["Titulo"] = titulo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ObtenerProductoTrace([FromBody] TraceReqDto req)
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                RespuestaGenerica<ProductoTraceDto> respuesta = await _prodSv.ObtenerProductoTrace(req.Desde,req.Hasta, TokenCookie);
                if (!respuesta.Ok || respuesta.EsError)
                {
                    var msg = respuesta.Mensaje ?? "Error al obtener los datos consultados";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }

                var lista = respuesta.ListaEntidad ?? [];
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<ProductoTraceDto>(
                    lista.OrderBy(o => o.p_desc).ToList(),
                    1,
                    registrosPorPagina,
                    lista.Count
                );
                var grid = new GridCoreSmart<ProductoTraceDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = lista.Count,
                    PrimerRegistro = ((1 - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(1 * registrosPorPagina, lista.Count),
                    RegistroFinal = lista.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)lista.Count / registrosPorPagina),
                    PaginaActual = 1,
                    Sort = "p_desc",
                    SortDir = "ASC",
                    DatoAux01 = $"Cargado: {DateTime.Now:HH:mm:ss}"
                };
                return View("_gridProdTrace", grid);
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
