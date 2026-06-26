using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class MovYCarteraDeCuentaFinanController : MovYCarteraDeCuentaFinanControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //INV_REPO_STK_VS_CONTEO
		private AppModulo _modulo_2; //INV_REPO_VAL_X_SEC
		private string APP_MODULO_1 = AppModulos.CONS_CTA_CORRIENTE_FINANCIERA.ToString();
		private string APP_MODULO_2 = AppModulos.DETALLE_VALORES_EN_CARTERA.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		public MovYCarteraDeCuentaFinanController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<MovYCarteraDeCuentaFinanController> logger,
												  IFinancieroServicio financieroServicio, ITipoCuentaFinServicio tipoCuentaFinServicio,
												  IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroMovYCarteraDeCuentaFinanModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "MOVIMIENTOS y CARTERA de CUENTAS FINANCIERAS";
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
		public IActionResult ActualizarListaCuentaFinanciera(string tcf_id) 
		{
			var model = new ListaCuentaFinModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var lista = _financieroServicio.ObtenerFinancieroCuentaLista(tcf_id, TokenCookie);
				if (lista != null && lista.Count > 0)
					model.ListaCuentaFinanciera = ObtenerListaCF(lista);
				else
					model.ListaCuentaFinanciera = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.CuentaFinancieraSeleccionada = "";
				ListaFinancieroCuenta = lista ?? [];
				return PartialView("_listaCuentaFin", model);

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
		public IActionResult InicializarPantallPrincipal(string TipoCuenta, string TipoCuentaTexto, string CuentaFinanciera, string CuentaFinancieraTexto, DateTime desde, DateTime hasta)
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				FinancieroCuentaSeleccionada = ListaFinancieroCuenta.Where(x => x.ctaf_id == CuentaFinanciera).First();
				model.Titulo = $"Tipo de Cuenta: {TipoCuentaTexto} - Cuentas Financieras: {CuentaFinancieraTexto} - Desde: {desde:dd/MM/yyyy} Hasta: {hasta:dd/MM/yyyy}";
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
		public IActionResult ObtenerMovimientosLista(FinancieroBcoCtaCteRequest request)
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
				return PartialView("_tablaMovimientos", model);
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
		public IActionResult ObtenerCarterasLista(string ctaf_id)
		{
			var model = new CarteraModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var ctfSeleccionada = FinancieroCuentaSeleccionada;
				if (ctfSeleccionada == null || !ctfSeleccionada.cartera)
					return PartialView("_tablaCartera", model);
				var carteraLista = _financieroServicio.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, TokenCookie);
				model.GrillaCartera = ObtenerGridCoreSmart<FinancieroCarteraDto>(carteraLista ?? []);
				return PartialView("_tablaCartera", model);
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
					case TipoDeReporte.ConsCtaCteFinanciera:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Reporte Rendición Cierre";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.DetalleDeValoresEnCartera:
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
		private void CargarDatosIniciales(FiltroMovYCarteraDeCuentaFinanModel model)
		{
			var hoy = DateTime.Today;
			model.Hasta = hoy;
			model.Desde = hoy.AddYears(-1);

			var tipoCuentas = _tipoCuentaFinServicio.ObtenerTipoCuentaFin(TokenCookie);
			if (tipoCuentas != null && tipoCuentas.Count > 0)
				model.ListaTipoCuenta = ObtenerLista(tipoCuentas);
			else
				model.ListaTipoCuenta = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.ListaCuentaFinanciera = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}

		private SelectList ObtenerLista(List<TipoCuentaFinDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.tcf_id, Descripcion = x.tcf_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaCF(List<FinancieroCuentaListaDto> cf)
		{
			var lista = cf.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		enum TipoDeReporte
		{
			ConsCtaCteFinanciera = 1,
			DetalleDeValoresEnCartera = 2
		}
		#endregion
	}
}
