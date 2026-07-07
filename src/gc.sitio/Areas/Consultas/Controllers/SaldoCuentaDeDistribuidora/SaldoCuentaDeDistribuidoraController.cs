using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
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
	public class SaldoCuentaDeDistribuidoraController : SaldoCuentaDeDistribuidoraControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //INV_REPO_STK_VS_CONTEO
		private AppModulo _modulo_2; //INV_REPO_VAL_X_SEC
		private string APP_MODULO_1 = AppModulos.SALDO_CTA_DISTR_DETALLE.ToString();
		private string APP_MODULO_2 = AppModulos.SALDO_CTA_DISTR_RESUMEN.ToString();
		private readonly IDocManagerServicio _docMSv;

		private readonly AppSettings _setting;
		private readonly IVendedorServicio _vendedorServicio;
		private readonly IConsultasServicio _consultasServicio;
		public SaldoCuentaDeDistribuidoraController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<SaldoCuentaDeDistribuidoraControladorBase> logger,
													IVendedorServicio vendedorServicio, IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
													IConsultasServicio consultasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_vendedorServicio = vendedorServicio;
			_consultasServicio = consultasServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroSaldoCuentaDeDistribuidoraModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "SALDO CUENTAS DE DISTRIBUIDORA";
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
		public IActionResult InicializarPantallPrincipal(string vendedoresText, string vendedoresIds)
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Titulo = $"Vendedores: - {vendedoresText}";
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
		public IActionResult BuscarDetalleDeSaldos(BuscarSaldoDetalleRequest request)
		{
			var model = new GridCoreSmart<SaldoDetalleDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarSaldoDetalleCtaDistribuidora(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Saldos Detalle de Cuentas de Distribuidora.");

				model = ObtenerGridCoreSmart<SaldoDetalleDto>(saldos);

				return PartialView("_gridSaldosDetalle", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener Saldos Detalle de Cuentas de Distribuidora."));
			}
		}

		[HttpPost]
		public IActionResult BuscarResumenDeSaldos(BuscarSaldoDetalleRequest request)
		{
			var model = new GridCoreSmart<SaldoResumenDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarSaldoResumenCtaDistribuidora(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Saldos Resumen de Cuentas de Distribuidora.");

				model = ObtenerGridCoreSmart<SaldoResumenDto>(saldos);

				return PartialView("_gridSaldosResumen", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener Saldos Resumen de Cuentas de Distribuidora."));
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
					case TipoDeReporte.RepoDetalleDeSaldos:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Reporte Rendición Cierre";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.RepoResumenDeSaldos:
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
		private void CargarDatosIniciales(FiltroSaldoCuentaDeDistribuidoraModel model)
		{
			var vendedores = _vendedorServicio.GetVendedorLista(TokenCookie);
			if (vendedores != null && vendedores.Count > 0)
				model.ListaVendedores = ObtenerLista(vendedores);
			else
			{
				model.ListaVendedores = HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
			var listR01 = new List<ComboGenDto>();
			ViewBag.VendedoresList = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
		}

		private SelectList ObtenerLista(List<VendedorDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.ve_id, Descripcion = x.ve_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		enum TipoDeReporte
		{
			RepoDetalleDeSaldos = 1,
			RepoResumenDeSaldos = 2
		}
		#endregion
	}
}
