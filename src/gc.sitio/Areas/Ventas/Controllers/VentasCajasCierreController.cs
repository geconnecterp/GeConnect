using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Ventas.Models.VentasCajasCierre;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentasCajasCierreController : VentasCajasCierreControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICajaServicio _iCajaSrv;
		public VentasCajasCierreController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCierreController> logger,
										 ICajaServicio cajaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
		}

		public IActionResult Index()
		{
			var model = new IndexModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CIERRE GENERAL";
				ViewData["Titulo"] = titulo;

				var cajas = _iCajaSrv.ObtenerPVAbiertos(AdministracionId, TokenCookie).Result;
				model.ListaCajasAbiertas = ObtenerGridCoreSmart<CajaPVAbiertosDto>(cajas);

				return View(model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult CerrarCajas()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });

				var resultado = _iCajaSrv.CierreCajaGral(UserName, AdministracionId, TokenCookie).Result;
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
					return AnalizarRespuesta(resultado, "");
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en el cierre de las cajas: {Mensaje}", resultado.Mensaje);
					return Json(new
					{
						ok = false,
						error = resultado.EsError,
						warn = resultado.EsWarn,
						msg = resultado.Mensaje ?? "Error al procesar el cierre de las cajas"
					});
				}
			}
			catch (NegocioException ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
		}
	}
}
