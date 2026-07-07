using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
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
	public class AnalisisDeVentasController : AnalisisDeVentasControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //ANALISIS_VENTA_MENSUAL
		private AppModulo _modulo_2; //ANALISIS_VENTA_DIARIO
		private AppModulo _modulo_3; //ANALISIS_VENTA_OP_VTA_DIARIO
		private AppModulo _modulo_4; //ANALISIS_VENTA_SUCURSAL
		private AppModulo _modulo_5; //ANALISIS_VENTA_CIERRES
		private AppModulo _modulo_6; //ANALISIS_VENTA_ANUAL
		private string APP_MODULO_1 = AppModulos.ANALISIS_VENTA_MENSUAL.ToString();
		private string APP_MODULO_2 = AppModulos.ANALISIS_VENTA_DIARIO.ToString();
		private string APP_MODULO_3 = AppModulos.ANALISIS_VENTA_OP_VTA_DIARIO.ToString();
		private string APP_MODULO_4 = AppModulos.ANALISIS_VENTA_SUCURSAL.ToString();
		private string APP_MODULO_5 = AppModulos.ANALISIS_VENTA_CIERRES.ToString();
		private string APP_MODULO_6 = AppModulos.ANALISIS_VENTA_ANUAL.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IApiVentasServicio _apiVentaServicio;

		public AnalisisDeVentasController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnalisisDeVentasController> logger,
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
			_modulo_5 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_5);
			_modulo_6 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_6);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroAnalisisDeVentasModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "ANÁLISIS DE VENTAS";
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
			var model = new AnalisisDeVentasMesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var request = new AnaVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesLista(request, TokenCookie);
				model.ListaAnaVtaMes = ObtenerGridCoreSmart<AnaVtaMesDto>(lista);

				return PartialView("_partialAnalisisDeVentasMes", model);
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
		public IActionResult CargarDetalleMes(int mes, int periodo, string sucursales)
		{
			var model = new AnalisisDeVentaDetalleMesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.sucursales = sucursales;
				model.mes = mes;
				model.periodo = periodo;
				return PartialView("_partialAnalisisDeVentasDetalle", model);
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
		public IActionResult CargarDetalleMesDiario(int mes, int periodo, string sucursales)
		{
			var model = new GridCoreSmart<AnaVtaMesDetalleDiarioDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var (desde, hasta) = ObtenerRangoMes(periodo, mes);
				var request = new AnaVtaMesRequest
				{
					desde = desde,
					hasta = hasta,
					adm_list = sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesDetalleDiaLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaVtaMesDetalleDiarioDto>(lista);
				return PartialView("_partialAnalisisDeVentasDetalleDiario", model);
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
		public IActionResult CargarDetalleMesHora(int mes, int periodo, string sucursales)
		{
			var model = new GridCoreSmart<AnaVtaMesDetalleHoraDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var (desde, hasta) = ObtenerRangoMes(periodo, mes);
				var request = new AnaVtaMesRequest
				{
					desde = desde,
					hasta = hasta,
					adm_list = sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesDetalleHoraLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaVtaMesDetalleHoraDto>(lista);
				return PartialView("_partialAnalisisDeVentasDetalleHora", model);
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
		public IActionResult CargarDetalleMesSucursal(int mes, int periodo, string sucursales)
		{
			var model = new GridCoreSmart<AnaVtaMesDetalleSucursalDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var (desde, hasta) = ObtenerRangoMes(periodo, mes);
				var request = new AnaVtaMesRequest
				{
					desde = desde,
					hasta = hasta,
					adm_list = sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesDetalleSucursalLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaVtaMesDetalleSucursalDto>(lista);
				return PartialView("_partialAnalisisDeVentasDetalleSucursal", model);
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
		public IActionResult CargarDetalleMesCierre(int mes, int periodo, string sucursales)
		{
			var model = new GridCoreSmart<AnaVtaMesDetalleCierreDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var (desde, hasta) = ObtenerRangoMes(periodo, mes);
				var request = new AnaVtaMesRequest
				{
					desde = desde,
					hasta = hasta,
					adm_list = sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesDetalleCierreLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaVtaMesDetalleCierreDto>(lista);
				return PartialView("_partialAnalisisDeVentasDetalleCierre", model);
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
		public IActionResult BuscarAnalisisDeVentasAnual(DateTime Desde, DateTime Hasta, string Sucursales)
		{
			var model = new GridCoreSmart<AnaVtaMesDetalleAnualDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var request = new AnaVtaMesRequest
				{
					desde = Desde,
					hasta = Hasta,
					adm_list = Sucursales
				};
				var lista = _apiVentaServicio.ObtenerAnaVtaMesDetalleAnualLista(request, TokenCookie);
				model = ObtenerGridCoreSmart<AnaVtaMesDetalleAnualDto>(lista);
				return PartialView("_partialAnalisisDeVentasDetalleAnual", model);
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
					case TipoDeReporte.Analisis_Venta_Mensual:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VENTA MENSUAL";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.Analisis_Venta_Diario:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VENTA DIARIO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					case TipoDeReporte.Analisis_Venta_Op_Vta_Diario:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS OPERATIVO DE VENTA DIARIO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_3);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_3);
						#endregion
						break;
					case TipoDeReporte.Analisis_Venta_Sucursal:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VENTA POR SUCURSAL";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					case TipoDeReporte.Analisis_Venta_Cierres:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VENTA DE CIERRES";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_5);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_5);
						#endregion
						break;
					case TipoDeReporte.Analisis_Venta_Anual:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANÁLISIS DE VENTA ANUAL";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_6);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_6);
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

		public static (DateTime Desde, DateTime Hasta) ObtenerRangoMes(int periodo, int mes)
		{
			var desde = new DateTime(periodo, mes, 1);
			var hasta = desde.AddMonths(1).AddDays(-1);

			return (desde, hasta);
		}

		#region Metodos Privados
		private enum TipoDeReporte
		{
			Analisis_Venta_Mensual = 1,
			Analisis_Venta_Diario = 2,
			Analisis_Venta_Op_Vta_Diario = 3,
			Analisis_Venta_Sucursal = 4,
			Analisis_Venta_Cierres = 5,
			Analisis_Venta_Anual = 6,
		}

		private void CargarDatosIniciales(FiltroAnalisisDeVentasModel model)
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
