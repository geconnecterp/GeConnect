using gc.api.core.Contratos.Servicios;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Rpr;
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
    public class TransferenciaController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly ILogger<CompraController> _logger;
		private readonly IProductoServicio _productoServicio;

		public TransferenciaController(ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context, IProductoServicio productoServicio) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> TRAutorizacionesLista()
		{
            var auth = EstaAutenticado;
            if (!auth.Item1 || auth.Item2 < DateTime.Now)
            {
                return RedirectToAction("Login", "Token", new { area = "seguridad" });
            }

			GridCore<TRPendienteDto> grid;
			try
			{
				// Carga por default al iniciar la pantalla
				var items = await _productoServicio.TRObtenerPendientes(AdministracionId, "%", "S", TokenCookie);
				grid = ObtenerGridCore<TRPendienteDto>(items);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener Autorizaciones de transferencias pendientes.");
				TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones de transferencias pendientes. Si el problema persiste informe al Administrador";
				grid = new();
			}
			return View(grid);
        }

		public async Task<IActionResult> NuevaTR(string ti)
		{
			return PartialView("TRNuevaAutorizacion");
		}

	}
}
