using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class AnalisisDeValoresDeVentasController : AnalisisDeValoresDeVentasControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IApiVentasServicio _apiVentaServicio;

		public AnalisisDeValoresDeVentasController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnalisisDeValoresDeVentasController> logger,
												   IAdministracionServicio administracionServicio, IApiVentasServicio apiVentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_apiVentaServicio = apiVentaServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroAnalisisDeValoresDeVentasModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "ANÁLISIS DE VALORES DE VENTAS";
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
		public IActionResult InicializarPantallPrincipal(string sucursalesText, DateTime desde, DateTime hasta)
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Titulo = $"Sucursales - {sucursalesText} - Desde: {desde:dd/MM/yyyy} Hasta: {hasta:dd/MM/yyyy}";
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
		public IActionResult BuscarAnalisisDeVentasMensual(DateTime Desde, DateTime Hasta, string Sucursales)
		{
			var model = new AnalisisDeValoresDeVentasMesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var request = new AnaDeValDeVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaDeValDeVtaMesLista(request, TokenCookie);
				model.ListaAnaDeValDeVtaMes = ObtenerGridCoreSmart<AnaValDeVtaMesDto>(lista);

				return PartialView("_partialAnalisisDeValoresDeVentasMes", model);
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
		public IActionResult CargarDetalleMes()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				return PartialView("_partialAnalisisDeValoresDeVentasDet");
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
		public IActionResult CargarDetalleMesDiario(DateTime Desde, DateTime Hasta, string Sucursales)
		{
			var model = new GridCoreSmart<AnaValDeVtaDetDiarioDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var request = new AnaDeValDeVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaDeValDeVtaDetDiarioLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaValDeVtaDetDiarioDto>(lista);
				return PartialView("_partialAnalisisDeValoresDeVentasDetDia", model);
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
		public IActionResult CargarDetalleMesPV(DateTime Desde, DateTime Hasta, string Sucursales)
		{
			var model = new GridCoreSmart<AnaValDeVtaDetPVDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var request = new AnaDeValDeVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaDeValDeVtaDetPVLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaValDeVtaDetPVDto>(lista);
				return PartialView("_partialAnalisisDeValoresDeVentasDetPV", model);
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
		public IActionResult CargarDetalleMesCB(DateTime Desde, DateTime Hasta, string Sucursales)
		{
			var model = new GridCoreSmart<AnaValDeVtaDetCBDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var request = new AnaDeValDeVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaDeValDeVtaDetCBLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaValDeVtaDetCBDto>(lista);
				return PartialView("_partialAnalisisDeValoresDeVentasDetCB", model);
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

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroAnalisisDeValoresDeVentasModel model)
		{
			var hoy = DateTime.Today;
			model.Desde = new DateTime(hoy.Year - 1, hoy.Month, 1);
			model.Hasta = DateTime.Today;

			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var tImpuestosList = new List<ComboGenDto>();
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
