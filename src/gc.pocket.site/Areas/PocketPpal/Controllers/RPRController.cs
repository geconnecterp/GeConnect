using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.pocket.site.Controllers;
using gc.pocket.site.Models.ViewModels;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.pocket.site.Areas.PocketPpal.Controllers
{
    [Area("PocketPpal")]
    public class RPRController : ControladorBase
    {
        private readonly MenuSettings _menuSettings;
        private readonly ILogger<InfoProdController> _logger;
        private readonly IProductoServicio _productoServicio;

        public RPRController(IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<MenuSettings> options1,
            ILogger<InfoProdController> logger, IProductoServicio productoServicio) : base(options, context)
        {
            _menuSettings = options1.Value;
            _logger = logger;
            _productoServicio = productoServicio;
        }

        public async Task<IActionResult> Index()
        {
            List<RPRAutorizacionPendienteDto> pendientes;
            GridCore<RPRAutorizacionPendienteDto> grid;
            try
            {
                //se buscará todas las autorizaciones pendientes
                pendientes = await _productoServicio.RPRObtenerAutorizacionPendiente(AdministracionId, TokenCookie);
                //resguardo lista de autorizaciones pendientes 
                AutorizacionesPendientes = pendientes;
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

        private GridCore<RPRAutorizacionPendienteDto> ObtenerAutorizacionPendienteGrid(List<RPRAutorizacionPendienteDto> pendientes)
        {
           
            var lista = new StaticPagedList<RPRAutorizacionPendienteDto>(pendientes, 1, 999, pendientes.Count);

            return new GridCore<RPRAutorizacionPendienteDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
        }

        [HttpGet]
        public async Task<IActionResult> CargarProductos(string rp)
        {
            RPRAutorizacionPendienteDto auto=new();
            try
            {
                auto = AutorizacionesPendientes.SingleOrDefault(x => x.Rp.Equals(rp));
                if (auto == null)
                {
                    TempData["Warn"] = $"No se pudo seleccionar la Autorización {rp}. Intente de nuevo.";
                    return RedirectToAction("Index");
                }
                //este viewbag es para que aparezca en la segunda fila del encabezado la leyenda que se quiera.
                //en este caso presenta el numero de autorización pendiente y el proveedor al que le pertenece.
                ViewBag.AppItem = new AppItem { Nombre = $"Auto:{auto.Rp}-{auto.Cta_denominacion}" };
                AutorizacionPendienteSeleccionada = auto;              
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
                TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
                return RedirectToAction("Index");
            }

            return View(auto);
        }
    }
}
