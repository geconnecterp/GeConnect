using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class CargarExtractoBancarioController : CargarExtractoBancarioControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly string tipoCTAF = "BA";
		public CargarExtractoBancarioController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<CargarExtractoBancarioController> logger,
												  IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
												  IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;

		}
		public IActionResult Index()
		{
			var model = new FiltroExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGAR EXTRACTO BANCARIO";
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

		public JsonResult ObtenerCuentaBanco(string ctaf_id)
		{
			try
			{
				if (ctaf_id == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var lista = ListaCuentaBancos.Where(x => x.ctaf_id == ctaf_id);
				if (lista == null || !lista.Any())
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado la cuenta banco solicitada." });
				
				return Json(new { error = false, warn = false, msg = "", lista.First().ext_fecha });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener los datos de la cuenta banco seleccionada." });
			}
		}

		public IActionResult CargarExtractoBancarioCrud(FinancieroBcoExtractoRequest request)
		{
			var model = new CrudExtractoBancarioModel();
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
				var lista = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id);
				model.CuentaBanco = $"{lista.First().ctaf_denominacion} ({lista.First().ctaf_id})";
				model.GrillaExtracto = ObtenerGridCoreSmart<CrudExtractoBancarioDto>(new List<CrudExtractoBancarioDto>());
				return PartialView("_crudExtractoBancario", model);
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
		private void CargarDatosIniciales(FiltroExtractoModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			ListaCuentaBancos = ctfLista;
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();
			//ViewBag.CuentaBancoList = HelperMvc<ComboGenDto>.ListaGenerica(cuentaBancoList);
		}
		#endregion
	}
}
