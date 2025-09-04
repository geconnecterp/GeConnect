using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class BancosController : BancosControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public BancosController(IFinancieroServicio financieroServicio,
								IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<BancosController> logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
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
				var grid = ObtenerGridCoreSmart<FinancieroBcoExtractoDto>(lista);
				return PartialView("_grillaExtractoBancario", grid);
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

		#region Métodos privados
		private void CargarDatosIniciales(FiltroModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();
			ViewBag.CuentaBancoList = HelperMvc<ComboGenDto>.ListaGenerica(cuentaBancoList);
		}
		#endregion
	}
}
