using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models;
using gc.sitio.Areas.Ventas.Models.VentasFalDeCajaCarga;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Reflection;
using static gc.sitio.Areas.Compras.Controllers.ControlYCorreccionDeRP.ControlYCorreccionDeRPController;

namespace gc.sitio.Areas.Ventas.Controllers.VentasFalDeCajaCarga
{
	[Area("Ventas")]
	public class VentasFalDeCajaCargaController : VentasFalDeCajaCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICajaServicio _iCajaSrv;
		private readonly IAdministracionServicio _administracionServicio;

		public VentasFalDeCajaCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasFalDeCajaCargaController> logger,
											  ICajaServicio cajaServicio, IAdministracionServicio administracionServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
			_administracionServicio = administracionServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroVentasFalDeCajaCargaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "FALTANTE DE CAJAS - CARGA";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(model);

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
		public IActionResult InicializarPantallaPrincipal()
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				return PartialView("_pantallaPrincipal", model);
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
		public IActionResult CargarFaltanteDeCaja(FaltanteDeCajaRequest request)
		{
			var model = new GridCoreSmart<FaltanteDeCajaDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var datos = _iCajaSrv.BuscarFaltanteDeCaja(request, TokenCookie).Result;
				if (datos == null || datos.Count <= 0)
				{
					FaltanteDeCajaLista = [];
					return PartialView("_partial_Faltante", model);
				}
				FaltanteDeCajaLista = datos;
				return PartialView("_partial_Faltante", ObtenerGridCoreSmart<FaltanteDeCajaDto>(datos));
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

		public JsonResult InicializarFechaProcesoFaltante(string admId)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (string.IsNullOrEmpty(admId))
					return Json(new { error = true, msg = "No se recibieron los datos para obtener la última fecha de proceso de faltantes." });

				var ultFalProc = _iCajaSrv.FaltanteDeCajaUltimaFecha(admId, TokenCookie).Result;
				if (ultFalProc != null && ultFalProc.FirstOrDefault() != null)
					return Json(new { error = false, warn = false, fecha = ultFalProc.FirstOrDefault().fecha.ToString("dd/MM/yyyy") });
				else
					return Json(new { error = false, warn = false, fecha = DateTime.Now.ToString("dd/MM/yyyy") });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar cambiar el TCO en la RPR");
				msg = $"Hubo algún inconveniente al intentar cambiar el TCO en la RPR.";
			}

			return Json(new { error = true, msg });
		}

		[HttpPost]
		public IActionResult ActualizarAcumulado([FromBody] ActualizarAcumuladoDto dto)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (dto == null)
					return Json(new { ok = false, error = true, msg = "No se recibieron los datos para la actualizacion." });

				var lista = FaltanteDeCajaLista;
				var item = lista.FirstOrDefault(x => x.usu_id == dto.usu_id);

				if (item == null)
					return Json(new { ok = false });

				item.acumulado = dto.Valor;

				FaltanteDeCajaLista = lista;

				return Json(new { ok = true });
			}
			catch (NegocioException ex)
			{
				return Json(new { ok = false, error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar actualizar");
				msg = $"Error al intentar actualizar.";
				return Json(new { ok = false, error = false, warn = true, msg = ex.Message });
			}
		}

		#region Métodos Privados
		private void CargarDatosIniciales(FiltroVentasFalDeCajaCargaModel model)
		{
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var ultFalProc = _iCajaSrv.FaltanteDeCajaUltimaFecha(AdministracionId, TokenCookie).Result;
			if (ultFalProc != null && ultFalProc.FirstOrDefault() != null)
				model.UltFaltanteProcesado = ultFalProc.FirstOrDefault().fecha;
			else
				model.UltFaltanteProcesado = DateTime.Now;
			model.ProcesarHasta = DateTime.Now;
			model.MostrarUltFaltanteProcesado = false;

		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion

		#region clases
		public class ActualizarAcumuladoDto
		{
			public string usu_id { get; set; } = string.Empty;
			public string adm_id { get; set; } = string.Empty;
			public decimal Valor { get; set; }
		}
		#endregion
	}
}
