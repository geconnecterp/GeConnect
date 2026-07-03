using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ComisionesDeVendedoresController : SaldoCuentaDeDistribuidoraControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IConsultasServicio _consultasServicio;
		public ComisionesDeVendedoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<SaldoCuentaDeDistribuidoraControladorBase> logger,
												IConsultasServicio consultasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_consultasServicio = consultasServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroComisionesDeVendedoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "COMISIONES DE VENDEDORES";
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
		public IActionResult BuscarComisionesVentasDetalle(ComisionesDeVendedoresRequest request)
		{
			var model = new GridCoreSmart<ComisionesDeVendedoresDetalleDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarComisionDeVendedorDetalle(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Comisiones Detalle de Vendedores.");

				model = ObtenerGridCoreSmart<ComisionesDeVendedoresDetalleDto>(saldos);

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
		public IActionResult BuscarComisionesVentasResumen(ComisionesDeVendedoresRequest request)
		{
			var model = new GridCoreSmart<ComisionesDeVendedoresResumenDto>();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (request == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				//debo realizar la busqueda de los presupuestos
				var saldos = _consultasServicio.BuscarComisionDeVendedorResumen(request, TokenCookie);

				if (saldos == null)
					throw new NegocioException("Hubo algun problema en la busqueda de Comisiones Resumen de Vendedores.");

				model = ObtenerGridCoreSmart<ComisionesDeVendedoresResumenDto>(saldos);

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

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroComisionesDeVendedoresModel model)
		{
			var hoy = DateTime.Today;
			model.Hasta = hoy;
			model.Desde = hoy.AddDays(-60);
		}
		#endregion
	}
}
