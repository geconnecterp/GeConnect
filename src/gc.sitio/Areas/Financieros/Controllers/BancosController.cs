using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class BancosController : BancosControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //VCE
		private AppModulo _modulo_2; //LBD
		private AppModulo _modulo_3; //LBR
		private AppModulo _modulo_4; //HLI
		private AppModulo _modulo_5; //EBA
		private string APP_MODULO_1 = AppModulos.VCE.ToString();
		private string APP_MODULO_2 = AppModulos.LBD.ToString();
		private string APP_MODULO_3 = AppModulos.LBR.ToString();
		private string APP_MODULO_4 = AppModulos.HLI.ToString();
		private string APP_MODULO_5 = AppModulos.EBA.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public BancosController(IFinancieroServicio financieroServicio,
								IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
								IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<BancosController> logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1); 
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_modulo_3 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_3);
			_modulo_4 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_4);
			_modulo_5 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_5);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "BANCOS";
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

		public IActionResult PosicionarseEnTabVencimientoChequeEmitido()
		{
			var model = new VencimientoChequeEmitidoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.FechaDesde = DateTime.Today;
				model.FechaHasta = DateTime.Today;
				
				if (ListaChequesAgrupados == null || ListaChequesAgrupados.Count == 0)
					model.GrillaCheques = new GridCoreSmart<FinancieroChequeDepositadoDto>();
				else
					model.GrillaCheques = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(ListaChequesAgrupados);
				
				if (ListaChequesDetalles == null || ListaChequesDetalles.Count == 0)
					model.GrillaChequesDetalle = new GridCoreSmart<FinancieroChequeDepositadoDto>();
				else
					model.GrillaChequesDetalle = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(ListaChequesDetalles);
				
				model.Total = 0;
				return PartialView("_tabVencimientoChequeEmitido", model);
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

		public IActionResult PosicionarseEnTabExtractoBancario(FinancieroBcoExtractoRequest request)
		{ 
			var model = new ExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				model.GrillaExtracto = new GridCoreSmart<FinancieroBcoExtractoDto>();
				return PartialView("_tabExtractoBancario", model);
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

		public IActionResult ObtenerExtractoBancario(FinancieroBcoExtractoRequest request)
		{
			var model = new ExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}
				var lista = _financieroServicio.GetFinancieroBcoExtracto(request, TokenCookie);
				model.GrillaExtracto = ObtenerGridCoreSmart<FinancieroBcoExtractoDto>(lista);
				return PartialView("_tabExtractoBancario", model);
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

		public IActionResult PosicionarseEnTabHistoricoLibro(FinancieroBcoCtaCteRequest request)
		{
			var model = new HistoricoLibroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				model.GrillaHistorico = new GridCoreSmart<FinancieroBcoCtaCteDto>();
				return PartialView("_tabHistoricoLibro", model);
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

		public IActionResult ObtenerHistoricoLibro(FinancieroBcoCtaCteRequest request)
		{
			var model = new HistoricoLibroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				var lista = _financieroServicio.GetFinancieroBcoCtaCte(request, TokenCookie);
				model.GrillaHistorico = ObtenerGridCoreSmart<FinancieroBcoCtaCteDto>(lista);
				return PartialView("_tabHistoricoLibro", model);
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

		/// <summary>
		/// Establece el tipo de reporte seleccionado por el usuario para la consulta de órdenes de pago.
		/// Inicializa el gestor de impresión y carga los documentos disponibles según el tipo de reporte.
		/// </summary>
		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.VencimientoChequeEmitido:
						break;
					case TipoDeReporte.LibroBancoDetalle:
						break;
					case TipoDeReporte.LibroBancoResumen:
						break;
					case TipoDeReporte.HistoricoLibro:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "HISTÓRICO LIBRO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					case TipoDeReporte.ExtractoBancario:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "EXTRACTO BANCARIO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_5);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_5);
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

		#region Métodos privados
		private void CargarDatosIniciales(FiltroModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();
			ViewBag.CuentaBancoList = HelperMvc<ComboGenDto>.ListaGenerica(cuentaBancoList);
		}

		enum TipoDeReporte
		{
			VencimientoChequeEmitido = 1,
			LibroBancoDetalle = 2,
			LibroBancoResumen = 3,
			HistoricoLibro = 4,
			ExtractoBancario = 5
		}
		#endregion
	}
}
