using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Compras.Controllers
{
    [Area("Compras")]
    public class ComprasController : ControladorBase
    {
        private readonly AppSettings _appSettings;
        private readonly ILogger<ComprasController> _logger;
        private readonly IProductoServicio _productoServicio;

        public ComprasController(ILogger<ComprasController> logger, IOptions<AppSettings> options, IProductoServicio productoServicio,
            IHttpContextAccessor context) : base(options, context)
        {
            _logger = logger;
            _appSettings = options.Value;
            _productoServicio = productoServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RPRAutorizaciones()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }
            List<RPRAutoComptesPendientesDto> pendientes;
            GridCore<RPRAutoComptesPendientesDto> grid;
            try
            {
                pendientes = await _productoServicio.RPRObtenerComptesPendiente(AdministracionId, TokenCookie);
                //resguardo lista de autorizaciones pendientes 
                //Veo que en  gc.pocket.site.Areas.PocketPpal.Controllers.RPRController.Index() almacena en una variable de sesión los datos.
                //Consultar si tengo que hacer lo mismo
                grid = ObtenerAutorizacionPendienteGrid(pendientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                grid = new();
            }
            return View(grid);
        }

        private GridCore<RPRAutoComptesPendientesDto> ObtenerAutorizacionPendienteGrid(List<RPRAutoComptesPendientesDto> pendientes)
        {

            var lista = new StaticPagedList<RPRAutoComptesPendientesDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<RPRAutoComptesPendientesDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }
    }
}
