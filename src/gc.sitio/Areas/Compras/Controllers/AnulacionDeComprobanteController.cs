using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante;
using gc.infraestructura.Dtos.Almacen.AnulacionDeComprobante.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models.AnulacionDeComprobante;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class AnulacionDeComprobanteController : AnulacionDeComprobanteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		public AnulacionDeComprobanteController(ICuentaServicio cuentaServicio,
												IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AnulacionDeComprobanteController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "ANULAR CARGA DE COMPROBANTE Y/O VALORIZACIONES";
				CargarDatosIniciales(true);
				return View();
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
		public IActionResult InicializarComprobante(string cta_id)
		{
			var model = new AnulacionDeComprobanteModel
			{
				GrillaComprobantes = new GridCoreSmart<ComprobanteParaAnularDto>()
			};

			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(cta_id))
				{
					ListaComprobanteParaAnular = [];
					return PartialView("_vistaComprobantes", model);
				}

				var response = _cuentaServicio.ObtenerComprobanteParaAnular(cta_id, TokenCookie);
				if (response == null || response.Count == 0)
				{
					ListaComprobanteParaAnular = [];
					return PartialView("_vistaComprobantes", model);
				}

				model.GrillaComprobantes = ObtenerGridCoreSmart<ComprobanteParaAnularDto>(response);
				ListaComprobanteParaAnular = response;
				return PartialView("_vistaComprobantes", model);
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
		public IActionResult InicializarVistaNotasACuenta(InicializarNotaACuentaRequest request)
		{
			var model = new NotasACuentaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					ListaNotaACuenta = [];
					return PartialView("_vistaNotasACuenta", model);
				}

				var notas = _cuentaServicio.ObtenerNotaACuentaDeValorizacionParaAnular(request, TokenCookie);
				if (notas == null || notas.Count == 0)
				{
					ListaNotaACuenta = [];
					model.MostrarGrilla = false;
					return PartialView("_vistaNotasACuenta", model);
				}
				else
					model.MostrarGrilla = true;
				model.GrillaNotasACuenta = ObtenerGridCoreSmart<NotaACuentaDto>(notas);
				ListaNotaACuenta = notas;
				return PartialView("_vistaNotasACuenta", model);
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
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaComprobanteParaAnular = [];
				ListaNotaACuenta = [];
				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - ANULACIONDECOMPROBANT" });
			}
		}

		[HttpPost]
		public JsonResult ConfirmarAnulacion(ConfirmarAnulacionRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = "No se recibieron los datos necesarios para la anulación." });

				request.admId = AdministracionId;
				request.usuId = UserName;
				Console.WriteLine($"cta_id: {request.ctaId}");
				Console.WriteLine($"dia_movi: {request.diaMovi}");
				Console.WriteLine($"cm_compte: {request.cmCompte}");
				Console.WriteLine($"tco_id: {request.tcoId}");
				Console.WriteLine($"adm_id: {request.admId}");
				Console.WriteLine($"usu_id: {request.usuId}");
				Console.WriteLine($"opcion: {request.opcion}");
				var res = _cuentaServicio.AnulacionDeComprobanteConfirma(request, TokenCookie);
				if (res == null)
					return Json(new { error = true, warn = false, msg = "No se pudo procesar la anulación del comprobante." });

				return AnalizarRespuesta(res, "El Comprobante se ha anulado con Éxito");
				//return Json(new { error = false, warn = false, msg = "Anulación de comprobante correctamente." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar anular el comprobante - ANULACIONDECOMPROBANTE" });
			}
		}

		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
		}
		#endregion
	}
}
