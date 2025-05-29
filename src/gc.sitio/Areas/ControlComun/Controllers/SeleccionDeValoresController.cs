using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.ViewModels;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class SeleccionDeValoresController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly IFinancieroServicio _financieroServicio;

		public SeleccionDeValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<SeleccionDeValoresController> logger, 
											ITipoCuentaFinServicio tipoCuentaFinServicio, IFinancieroServicio financieroServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AbrirComponente()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var tipoCuentaFinLista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores("OPP", TokenCookie);
				var model = new SeleccionDeValoresViewModel()
				{
					GrillaTipoCuentaFin = ObtenerGridCoreSmart<TipoCuentaFinDto>(tipoCuentaFinLista)
				};
				return View("~/areas/ControlComun/views/SeleccionDeValores/_index.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult CargarCuentasFinancieras(string tcf_id)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var finLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tcf_id, TokenCookie);
				var model = ObtenerGridCoreSmart<FinancieroDesdeSeleccionDeTipoDto>(finLista);
				return View("~/areas/ControlComun/views/SeleccionDeValores/_grillaFinancieros.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
	}
}
