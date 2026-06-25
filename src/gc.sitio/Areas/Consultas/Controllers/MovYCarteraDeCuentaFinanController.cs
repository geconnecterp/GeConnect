using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class MovYCarteraDeCuentaFinanController : MovYCarteraDeCuentaFinanControladorBase
	{
		//************************
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		public MovYCarteraDeCuentaFinanController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<MovYCarteraDeCuentaFinanController> logger,
												  IFinancieroServicio financieroServicio, ITipoCuentaFinServicio tipoCuentaFinServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
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
		#endregion
	}
}
