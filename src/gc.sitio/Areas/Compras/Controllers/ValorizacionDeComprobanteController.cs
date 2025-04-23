using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class ValorizacionDeComprobanteController : ValorizacionDeComprobanteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;


		public ValorizacionDeComprobanteController(ICuentaServicio cuentaServicio, IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ValorizacionDeComprobanteController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			MetadataGrid metadata;
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "VALORIZACIÓN DE COMPROBANTE";
				CargarDatosIniciales(true);
				return View();
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
		public IActionResult CargarComprobantesDelProveedorSeleccionado(string ctaId)
		{
			var model = new ListaComptePendienteDeValorizarModel();
			try
			{
				CtaIdSelected = ctaId;
				CargarComprobantesDelProveedor(ctaId, _cuentaServicio);
				model.LstComptePendiente = ComboComptesPendientes();
				model.cm_compte = string.Empty;
				return PartialView("_listaComptesPendientes", model);
			}
			catch (Exception ex)
			{
				return PartialView("_empty_view");
			}

		}

		////Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
		//[HttpPost]
		//public JsonResult CargarComprobantesDelProveedorSeleccionado(string ctaId)
		//{
		//	try
		//	{
		//		CtaIdSelected = ctaId;
		//		CargarComprobantesDelProveedor(ctaId, _cuentaServicio);
		//		return Json(new { error = false, warn = false, msg = string.Empty });
		//	}
		//	catch (Exception ex)
		//	{
		//		return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
		//	}

		//}

		//[HttpPost]
		//public JsonResult BuscarComprobantes(string prefix)
		//{
		//	if ((ComprobantesPendientesDeValorizarLista == null || ComprobantesPendientesDeValorizarLista.Count <= 0) && (!string.IsNullOrEmpty(CtaIdSelected)))
		//	{
		//		CargarComprobantesDelProveedorSeleccionado(CtaIdSelected);
		//	}
		//	var rub = ComprobantesPendientesDeValorizarLista.Where(x => x.pg_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
		//	var rubros = rub.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_lista });
		//	return Json(rubros);
		//}

		#region Métodos privados
		protected SelectList ComboComptesPendientes()
		{
			var lista = ComprobantesPendientesDeValorizarLista.Select(x => new ComboGenDto { Id = x.cm_compte.ToString(), Descripcion = $"{x.tco_desc} ({x.tco_id}) {x.cm_compte} {x.cm_fecha.ToShortDateString()} {x.cm_total}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(bool actualizar)
		{

		}
		protected void CargarComprobantesDelProveedor(string ctaId, ICuentaServicio _cuentaServicio)
		{
			var adms = _cuentaServicio.ObtenerComprobantesPendientesDeValorizar(ctaId, TokenCookie);
			ComprobantesPendientesDeValorizarLista = adms;
		}
		#endregion
	}
}
