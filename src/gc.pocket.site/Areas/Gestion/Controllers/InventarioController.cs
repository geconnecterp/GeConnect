using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.Gestion.Controllers
{
    [Area("Gestion")]
    public class InventarioController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly AppSettings _configuracion;
        private readonly ILogger<AlmacenController> _logger;
        private readonly IInventarioServicio _invSv;

        public InventarioController(ILogger<AlmacenController> logger,
            IOptions<MenuSettings> options,
            IOptions<AppSettings> options2,
            IInventarioServicio invSv,
            IOptions<AppSettings> options1, IHttpContextAccessor context) : base(options1, options, context, logger)
        {
            _logger = logger;
            _menuSettings = options.Value;
            _invSv = invSv;
            _configuracion = options2.Value;
        }
        public IActionResult Index()
        {
            var sigla = "inv";
            var modulo = _menuSettings.Aplicaciones.SingleOrDefault(x => x.Sigla.Equals(sigla, StringComparison.OrdinalIgnoreCase));
            if (modulo == null)
            {
                throw new NegocioException("No se logro encontrar la configuración del Módulo. Si el problema persiste informe al Administrador");
            }
            return View(modulo);
        }

        [HttpPost]
        public IActionResult ObtenerInventarioLista()
        {
            try
            {
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;
                GetInventarioListaRequest req = new GetInventarioListaRequest
                {
                    desde = new(2020, 1, 1),
                    hasta = DateTime.Now,
                    adm_id = AdministracionId,
                    usu_id = "%",//UserName,
                    inve_id = "S"
                };
                var respuesta = _invSv.GetInventarioLista(req, TokenCookie);
                if (respuesta == null)
                {
                    var msg = "Error al obtener los inventarios";
                    TempData["error"] = msg;
                    throw new NegocioException(msg);
                }


                var lista = respuesta;
                int registrosPorPagina = _configuracion.NroRegistrosPagina;
                var pagedList = new StaticPagedList<InventarioListaDto>(
                    lista.OrderBy(o => o.inv_nro).ToList(),
                    1,
                    registrosPorPagina,
                    lista.Count
                );
                var grid = new GridCoreSmart<InventarioListaDto>
                {
                    ListaDatos = pagedList,
                    CantidadReg = lista.Count,
                    PrimerRegistro = ((1 - 1) * registrosPorPagina) + 1,
                    UltimoRegistro = Math.Min(1 * registrosPorPagina, lista.Count),
                    RegistroFinal = lista.Count,
                    CantidadPaginas = (int)Math.Ceiling((double)lista.Count / registrosPorPagina),
                    PaginaActual = 1,
                    Sort = "cta_denominacion",
                    SortDir = "ASC",
                    DatoAux01 = $"Cargado: {DateTime.Now:HH:mm:ss}"
                };
                return PartialView("_gridInventarios", grid);
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
