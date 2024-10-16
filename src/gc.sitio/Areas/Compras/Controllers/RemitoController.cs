using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
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

			GridCore<RemitoGenDto> grid;
			try
			{
				// Carga por default al iniciar la pantalla
				var items = await _remitoServicio.ObtenerRemitosTransferidos(AdministracionId, TokenCookie);
				grid = ObtenerGridCore<RemitoGenDto>(items);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener los remitos transferidos pendientes.");
				TempData["error"] = "Hubo algun problema al intentar obtener los remitos transferidos pendientes. Si el problema persiste informe al Administrador";
				grid = new();
			}
			return View(grid);
		}

		public async Task<JsonResult> SetearEstadoDeRemito(string remCompte, string estado)
		{
			try
			{
				if (string.IsNullOrEmpty(remCompte))
					return Json(new { error = true, warn = false, msg = "Faltan datos (id de remito)." });
				if (string.IsNullOrEmpty(estado))
					return Json(new { error = true, warn = false, msg = "Faltan datos (estado)." });
				var respuesta = await _remitoServicio.SetearEstado(remCompte, estado, TokenCookie);
				if (respuesta == null)
					return Json(new { error = false, warn = true, msg = "Algo salió mal al intentar cambiar el estado del remito, intente nuevamente mas tarde." });
				if (respuesta.Count > 1 && string.IsNullOrWhiteSpace(respuesta.First().resultado_msj))
					return Json(new { error = false, warn = true, msg = "Algo salió mal al intentar cambiar el estado del remito, intente nuevamente mas tarde." });
				if (!string.IsNullOrWhiteSpace(respuesta.First().resultado_msj) && respuesta.First().resultado != 0)
					return Json(new { error = false, warn = true, msg = $"Error ({respuesta.First().resultado}) {respuesta.First().resultado_msj}" });
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar setear el estado del remito.");
				TempData["error"] = "Hubo algun problema al intentar setear el estado del remito. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, msg = "Error al intentar setear el estado del remito." });
			}

		}

		public async Task<JsonResult> ConfirmarRecepcion(string remCompte)
		{
			try
			{
				var result = Json(new { });
				if (string.IsNullOrWhiteSpace(remCompte))
					result = Json(new { error = false, warn = true, msg = "Debe seleccionar un remito para confirmar selección." });
				var respuesta = await _remitoServicio.ConfirmarRecepcion(remCompte, UserName, TokenCookie);
				if (respuesta == null)
					result = Json(new { error = false, warn = true, msg = "Algo salió mal al intentar confirmar la recepción del remito, intente nuevamente mas tarde." });
				if (respuesta.Count > 1 || respuesta.Count == 0)
					result = Json(new { error = false, warn = true, msg = "Algo salió mal al intentar confirmar la recepción del remito, intente nuevamente mas tarde." });
				if (string.IsNullOrWhiteSpace(respuesta.First().resultado_msj) && respuesta.First().resultado != 0)
					result = Json(new { error = false, warn = true, msg = "Algo salió mal al intentar confirmar la recepción del remito, intente nuevamente mas tarde." });
				if (!string.IsNullOrWhiteSpace(respuesta.First().resultado_msj) && respuesta.First().resultado != 0)
					result = Json(new { error = false, warn = true, msg = $"Error ({respuesta.First().resultado}) {respuesta.First().resultado_msj}" });
				if (respuesta.First().resultado == 0)
					result = Json(new { error = false, warn = false, msg = $"Confirmación exitosa." });
				//else
				//	result = Json(new { error = false, warn = true, msg = "Algo salió mal al intentar confirmar la recepción del remito, intente nuevamente mas tarde." });
				Thread.Sleep(500);
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar setear el estado del remito.");
				TempData["error"] = "Hubo algun problema al intentar setear el estado del remito. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, msg = "Error al intentar setear el estado del remito." });
			}
		}

		public async Task<IActionResult> VerConteosLista(string remCompte, string quienEnvia)
		{
			RemitoDetalle remito = new();
			try
			{
				if (remCompte == null)
					return ObtenerMensajeDeError("Debe proporcionar un identificador de remito válido. Si el problema persiste informe al Administrador.");
				var items = await _remitoServicio.VerConteos(remCompte, TokenCookie);
				foreach (var item in items)
				{
					item.Row_color = ObtenerColor(item.diferencia);
				}
				var grid = ObtenerGridCore<RemitoVerConteoDto>(items);
				remito.Productos = grid;
				remito.Remito = $"Remito N°: {remCompte}";
				remito.QuienEnvia = $"Enviado por: {quienEnvia}";
				remito.rem_compte = remCompte;

			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener el detalle del remito.");
				TempData["error"] = "Hubo algun problema al obtener el detalle del remito. Si el problema persiste informe al Administrador";
				remito = new();
			}
			return View(remito);
		}

		private static string ObtenerColor(decimal diferencia)
		{
			if (diferencia == 0)
				return "#ffffff";
			else
				return "#ff4500";
		}
	}
}
