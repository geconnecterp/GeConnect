using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers.ComisionesDeRepartidores
{
	[Area("Consultas")]
	public class ComisionesDeRepartidoresController : ComisionesDeRepartidoresControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //INV_REPO_STK_VS_CONTEO
		private AppModulo _modulo_2; //INV_REPO_VAL_X_SEC
		private string APP_MODULO_1 = AppModulos.COMISIONES_REPARTIDORES_DETALLE.ToString();
		private string APP_MODULO_2 = AppModulos.COMISIONES_REPARTIDORES_RESUMEN.ToString();
		private readonly IDocManagerServicio _docMSv;

		private readonly AppSettings _setting;
		private readonly IConsultasServicio _consultasServicio;
		public ComisionesDeRepartidoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ComisionesDeRepartidoresControladorBase> logger,
												  IConsultasServicio consultasServicio, IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_consultasServicio = consultasServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroComisionesDeRepartidoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "COMISIONES DE REPARTIDORES";
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
		public IActionResult InicializarPantallPrincipal(DateTime desde, DateTime hasta)
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Titulo = $"Fechas: {desde.ToShortDateString()} hasta {hasta.ToShortDateString()}";
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
		public IActionResult BuscarComisionesVentasDetalle(ComisionesDeRepartidoresRequest request)
		{
			var model = new GridCoreSmart<ComisionesDeRepartidoresDetalleDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarComisionDeRepartidorDetalle(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Comisiones Detalle de Vendedores.");

				model = ObtenerGridCoreSmart<ComisionesDeRepartidoresDetalleDto>(saldos);

				return PartialView("_gridComisionesVentasDetalle", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener Comisiones Detalle de Vendedores."));
			}
		}

		[HttpPost]
		public IActionResult BuscarComisionesVentasResumen(ComisionesDeRepartidoresRequest request)
		{
			var model = new GridCoreSmart<ComisionesDeRepartidoresResumenDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarComisionDeRepartidorResumen(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Comisiones Resumen de Vendedores.");

				model = ObtenerGridCoreSmart<ComisionesDeRepartidoresResumenDto>(saldos);

				return PartialView("_gridComisionesVentasResumen", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener Comisiones Resumen de Vendedores."));
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
					case TipoDeReporte.ComisionesRepartidoresDetalle:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Reporte Rendición Cierre";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.ComisionesRepartidoresResumen:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Reporte Analítico de Operaciones";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
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
		private void CargarDatosIniciales(FiltroComisionesDeRepartidoresModel model)
		{
			var hoy = DateTime.Today;
			model.Hasta = hoy;
			model.Desde = hoy.AddDays(-60);
		}

		enum TipoDeReporte
		{
			ComisionesRepartidoresDetalle = 1,
			ComisionesRepartidoresResumen = 2
		}
		#endregion
	}
}
