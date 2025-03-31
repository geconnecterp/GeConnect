using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class ComprobanteDeCompraController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoOpeIvaServicio _tipoOpeServicio;
		private readonly ICondicionAfipServicio _condicionAfipServicio;
		private readonly ITipoProveedorServicio _tipoProveedorServicio;
		private readonly ITipoMonedaServicio _tipoMonedaServicio;
		private readonly ITipoComprobanteServicio _tipoComprobanteServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		public ComprobanteDeCompraController(ICuentaServicio cuentaServicio, ITipoOpeIvaServicio tipoOpeIvaServicio, ICondicionAfipServicio condicionAfipServicio,
											 ITipoProveedorServicio tipoProveedorServicio, ITipoMonedaServicio tipoMonedaServicio, ITipoComprobanteServicio tipoComprobanteServicio, 
											 ITipoGastoServicio tipoGastoServicio, IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ComprobanteDeCompraController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_tipoOpeServicio = tipoOpeIvaServicio;
			_condicionAfipServicio = condicionAfipServicio;
			_tipoProveedorServicio = tipoProveedorServicio;
			_tipoMonedaServicio = tipoMonedaServicio;
			_tipoComprobanteServicio = tipoComprobanteServicio;
			_tipoGastoServicio = tipoGastoServicio;
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

				ViewData["Titulo"] = "CARGA DE COMPROBANTE DE COMPRA";
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
		public IActionResult InicializarComprobante(string cta_id) 
		{
			var model = new ComprobanteDeCompraModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(cta_id))
					return PartialView("_tabComprobante", model);

				var response = _cuentaServicio.GetCompteDatosProv(cta_id, TokenCookie).First();
				if (response == null)
					return PartialView("_tabComprobante", model);

				model.Moneda = ComboMoneda();
				model.TipoOpe = ComboTipoOpe();
				model.CondAfip = ComboAfip();
				model.TipoCompte = ComboTipoCompte(response.afip_id);
				model.CtaDirecta = ComboTipoGasto();
				model.Comprobante = response;
				return PartialView("_tabComprobante", model);
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
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
			
			if (TipoOpeIvaLista.Count == 0 || actualizar)
				ObtenerTiposOpeIva(_tipoOpeServicio);

			if (CondicionesAfipLista.Count == 0 || actualizar)
				ObtenerCondicionesAfip(_condicionAfipServicio);

			if (TipoProvLista.Count == 0 || actualizar)
				ObtenerTiposProveedor(_tipoProveedorServicio);

			if (TipoMonedaLista.Count == 0 || actualizar)
				ObtenerTipoMoneda(_tipoMonedaServicio);

			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);
			//if (OrdenDeCompraEstadoLista.Count == 0 || actualizar)
			//{
			//	ObtenerOrdenDeCompraEstadoLista(_ordenDeCompraEstadoServicio);
			//}

			//if (AdministracionesLista.Count == 0 || actualizar)
			//{
			//	ObtenerAdministracionesLista(_adminServicio);
			//}
		}
		protected SelectList ComboTipoCompte(string afip_id)
		{
			var listaTemp = _tipoComprobanteServicio.BuscarTipoComprobanteListaPorTipoAfip(afip_id, Token).Result;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
