using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ChequeDeTerceroRechazadoController : ChequeDeTerceroRechazadoControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly string tipoCTAF = "BA";
		public ChequeDeTerceroRechazadoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ChequeDeTerceroRechazadoController> logger,
												  IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			var model = new ChequeRechazadoPasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CHEQUES DEPOSITADOS RECHAZADOS";
				ViewData["Titulo"] = titulo;

				var listaCuentasBancos = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCTAF, AdministracionId, TokenCookie);
				ListaCuentaBancos = listaCuentasBancos;
				model.ListaCuentasBancarias = ComboCuentaBancos(ListaCuentaBancos);

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

		public IActionResult BuscarChequesDepositados(string ctaf_id, DateTime fechaDesde, DateTime fechaHasta)
		{ 
			var model = new ChequesDepositadosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaDeCheques = _financieroServicio.GetFinancieroChequeDepositado(ctaf_id, fechaDesde, fechaHasta, TokenCookie);
				model.GrillaChequesDepositados = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(listaDeCheques);
				model.FechaRechazado = DateTime.Today;

				return PartialView("_chequesDepositados", model);
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

		public IActionResult VolverPasoUno()
		{
			var model = new ChequeRechazadoPasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaCuentasBancos = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCTAF, AdministracionId, TokenCookie);
				ListaCuentaBancos = listaCuentasBancos;
				model.ListaCuentasBancarias = ComboCuentaBancos(ListaCuentaBancos);

				return PartialView("_pasoUno", model);
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

		#region Métodos Privados
		protected SelectList ComboCuentaBancos(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
