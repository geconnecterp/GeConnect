using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class AnalisisDeValoresDeVentasController : AnalisisDeValoresDeVentasControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //ANALISIS_DE_VALORES_VENTA_MENSUAL
		private AppModulo _modulo_2; //ANALISIS_DE_VALORES_VENTA_DIARIO
		private AppModulo _modulo_3; //ANALISIS_DE_VALORES_VENTA_PV
		private AppModulo _modulo_4; //ANALISIS_DE_VALORES_VENTA_CB
		private string APP_MODULO_1 = AppModulos.ANALISIS_DE_VALORES_DE_VENTA_MENSUAL.ToString();
		private string APP_MODULO_2 = AppModulos.ANALISIS_DE_VALORES_DE_VENTA_DIARIO.ToString();
		private string APP_MODULO_3 = AppModulos.ANALISIS_DE_VALORES_DE_VENTA_PV.ToString();
		private string APP_MODULO_4 = AppModulos.ANALISIS_DE_VALORES_DE_VENTA_CASHBACK.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IApiVentasServicio _apiVentaServicio;

		public AnalisisDeValoresDeVentasController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnalisisDeValoresDeVentasController> logger,
												   IAdministracionServicio administracionServicio, IApiVentasServicio apiVentaServicio,
												   IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_apiVentaServicio = apiVentaServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_modulo_3 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_3);
			_modulo_4 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_4);
			_docMSv = docManager; //instancio el servicio de impresión
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

		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.Analisis_De_Valores_Venta_Mensual:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VALORES DE VENTA MENSUAL";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.Analisis_De_Valores_Venta_Diario:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VALORES DE VENTA DIARIO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					case TipoDeReporte.Analisis_De_Valores_Venta_Pv:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VALORES DE VENTA PV";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_3);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_3);
						#endregion
						break;
					case TipoDeReporte.Analisis_De_Valores_Venta_Cb:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VALORES VENTA CASHBACK";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					default:
						break;
				}

				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		#region Metodos Privados
		private enum TipoDeReporte
		{
			Analisis_De_Valores_Venta_Mensual = 1,
			Analisis_De_Valores_Venta_Diario = 2,
			Analisis_De_Valores_Venta_Pv = 3,
			Analisis_De_Valores_Venta_Cb = 4,
		}
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
