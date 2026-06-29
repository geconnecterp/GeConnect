using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ReporteMovDeCuentaDirectaController : ReporteMovDeCuentaDirectaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		private readonly IConsultasServicio _consultasServicio;
		public ReporteMovDeCuentaDirectaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ReporteMovDeCuentaDirectaController> logger,
												   ITipoGastoServicio tipoGastoServicio, IConsultasServicio consultasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoGastoServicio = tipoGastoServicio;
			_consultasServicio = consultasServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroReporteMovDeCtaDtaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE MOVIMIENTOS DE CUENTAS DIRECTAS";
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
		public IActionResult BuscarMovimientoDeCtaDta(BuscarMovDeCuentaDirectaRequest request)
		{
			var model = new GridCoreSmart<MovimientoListaDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
				{
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));
				}

				//debo realizar la busqueda de los presupuestos
				var movimientos = _consultasServicio.ConsultaMovimientoLista(request, TokenCookie);

				if (movimientos == null)
				{
					throw new NegocioException("Hubo algun problema en la busqueda de Movimientos de Cuentas Directas.");
				}

				model = ObtenerGridCoreSmart<MovimientoListaDto>(movimientos);

				return PartialView("_gridMovimientos", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
			}
		}

		[HttpPost]
		public JsonResult BuscarCuentasDeGastos(string prefix)
		{
			var cta = TipoGastoLista.Where(x => x.ctag_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var cuentas = cta.Select(x => new ComboGenDto { Id = x.ctag_id, Descripcion = x.ctag_lista });
			return Json(cuentas);
		}

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroReporteMovDeCtaDtaModel model)
		{
			var hoy = DateTime.Today;
			model.Hasta = hoy;
			model.Desde = hoy.AddYears(-1);

			var listR01 = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

			if (TipoGastoLista.Count <= 0)
				ObtenerTipoGastos(_tipoGastoServicio);

		}

		#endregion
	}
}
