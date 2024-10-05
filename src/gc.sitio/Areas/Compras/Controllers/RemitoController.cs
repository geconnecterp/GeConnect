using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
    [Area("Compras")]
    public class RemitoController : ControladorBase
    {
        private readonly AppSettings _appSettings;
        private readonly IRemitoServicio _remitoServicio;
        private readonly ILogger<CompraController> _logger;

        public RemitoController(ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context, IRemitoServicio remitoServicio) : base(options, context)
        {
            _logger = logger;
            _appSettings = options.Value;
            _remitoServicio = remitoServicio;
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> RemitosTransferidosLista()
        {
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

            GridCore<RemitoTransferidoDto> grid;
            try
            {
                // Carga por default al iniciar la pantalla
                var items = await _remitoServicio.ObtenerRemitosTransferidos(AdministracionId, TokenCookie);
                grid = ObtenerGridCore<RemitoTransferidoDto>(items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los remitos transferidos pendientes.");
                TempData["error"] = "Hubo algun problema al intentar obtener los remitos transferidos pendientes. Si el problema persiste informe al Administrador";
                grid = new();
            }
            return View(grid);
        }
    }
}
