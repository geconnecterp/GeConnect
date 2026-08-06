using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.Areas.Productos.Models.ListaDePreciosGestionar;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Productos.Controllers.ListaDePreciosGestionar
{
	[Area("Productos")]
	public class ListaDePreciosGestionarController : ListaDePreciosGestionarControladorBase
	{
		private readonly IPrecioListaServicio _precioListaSrv;
		private readonly IListaDePrecioServicio _listaPrcSrv;
		private const string lp_principal = "001";
		public ListaDePreciosGestionarController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ListaDePreciosGestionarController> logger, 
												 IPrecioListaServicio precioListaSrv, IListaDePrecioServicio listaPrcSrv) : base(options, contexto, logger)
		{
			_precioListaSrv = precioListaSrv;
			_listaPrcSrv = listaPrcSrv;
		}

		public IActionResult Index()
		{
			var model = new ListaDePreciosGestionarModel();
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var titulo = "LISTA DE PRECIOS";
				ViewData["Titulo"] = titulo;

				var lp = _listaPrcSrv.GetListaPrecio(TokenCookie);
				model.GrillaListaDePrecios = ObtenerGridCoreSmart<ListaPrecioDto>(lp);
				ListaPrecio = lp;
				return View(model);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la vista de Gestión de Lista de Precios");
				TempData["error"] = "Hubo un problema al cargar la vista de Gestión de Lista de Precios. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public IActionResult CargarDatosDeListaDePrecio(string lp_id)
		{
			var model = new ListaPrecioDto();
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (ListaPrecio==null || !ListaPrecio.Any())
					return PartialView("_gridMensaje", CrearRespuestaWarning("No se encontraron listas de precios en la sesión. Por favor, recargue la página."));

				var item = ListaPrecio.FirstOrDefault(x => x.lp_id == lp_id);
				if (item == null)
					return PartialView("_gridMensaje", CrearRespuestaWarning($"No se encontró la lista de precios con ID '{lp_id}' en la sesión."));
				return PartialView("_partialMargenLP", item);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la sección de datos de la lista de precios.");
				TempData["error"] = "Hubo un problema al cargar la sección de datos de la lista de precios.. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}

		public IActionResult CargarDatosDeListaDePrecioRubCta(string lp_id)
		{
			try
			{
				// Versión optimizada del código de autenticación
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (string.IsNullOrEmpty(lp_id))
					return PartialView("_gridMensaje", CrearRespuestaWarning("Debe especificar el ID de la lista de precios."));

				var lista = _precioListaSrv.ObtenerListaPreciosRubCta(lp_id, TokenCookie).Result.ListaEntidad;
				
				if (lista == null)
					return PartialView("_gridMensaje", CrearRespuestaWarning($"No se encontró la lista de precios para Rub/Cta con ID '{lp_id}' en la sesión."));
				var listaPRubCta = ObtenerGridCoreSmart<ListaPrecioRubCtaDto>(lista);
				return PartialView("_partialLPRubrosProv", listaPRubCta);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al cargar la sección de datos de la lista de precios para Rub/Cta especificado.");
				TempData["error"] = "Hubo un problema al cargar la sección de datos de la lista de precios para Rub/Cta especificado. Si el problema persiste, contacte al administrador.";
				return View();
			}
		}
	}
}
