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
			var model = new TRCRUDDto();
			try
			{
				var itemsAutSucursales = await _productoServicio.TRObtenerAutSucursales(AdministracionId, TokenCookie);
				model.ListaAutSucursales = ObtenerGridCore<TRAutSucursalesDto>(itemsAutSucursales);
				var itemsAutPI = await _productoServicio.TRObtenerAutPI(AdministracionId, "", TokenCookie);
				model.ListaAutPI = ObtenerGridCore<TRAutPIDto>(itemsAutPI);
				model.ListaPedidosIncluidos = ObtenerGridCore<TRAutPIDto>(new List<TRAutPIDto>());
				model.ListaDepositosDeEnvio = ObtenerGridCore<TRAutDepoDto>(new List<TRAutDepoDto>());
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener las Sucursales en transferencias pendientes.");
				TempData["error"] = "Hubo algun problema al intentar obtener las Sucursales en transferencias pendientes. Si el problema persiste informe al Administrador";
			}
			return PartialView("TRCrudAutorizacion", model);
		}

		public async Task<IActionResult> CargarPedidosPorSucursal(string admId)
		{
			var model = new GridCore<TRAutPIDto>();
			try
			{
				var itemsAutPI = await _productoServicio.TRObtenerAutPI(admId, "", TokenCookie);
				model = ObtenerGridCore<TRAutPIDto>(itemsAutPI);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener los Pedidos Internos por Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener los Pedidos Internos por Sucursal. Si el problema persiste informe al Administrador";
			}
			return PartialView("_rprPedidosPorSucursal", model);
		}

		public async Task<IActionResult> CargarDepositosInclPorSucursal(string admId)
		{
			var model = new GridCore<TRAutDepoDto>();
			try
			{
				var itemsAutDepo = await _productoServicio.TRObtenerAutDepositos(admId, TokenCookie);
				model = ObtenerGridCore<TRAutDepoDto>(itemsAutDepo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener los Depositos Incluidos por Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener los Depositos Incluidos por Sucursal. Si el problema persiste informe al Administrador";
			}
			return PartialView("_rprDepositosInclPorSucursal", model);
		}
	}
}
