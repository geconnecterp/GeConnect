using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDePagoDirectaController : OrdenDePagoDirectaControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ITipoOrdenDePagoServicio _tipoOrdenDePagoServicio;
		private readonly ITipoComprobanteServicio _tipoComprobanteServicio;
		private readonly ICondicionAfipServicio _condicionAfipServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		public OrdenDePagoDirectaController(ITipoOrdenDePagoServicio tipoOrdenDePagoServicio, ITipoComprobanteServicio tipoComprobanteServicio, ICondicionAfipServicio condicionAfipServicio,
											IOrdenDePagoServicio ordenDePagoServicio,
											IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDePagoDirectaController> logger) : base(options, contexto, logger)
		{
			_settings = options.Value;
			_tipoOrdenDePagoServicio = tipoOrdenDePagoServicio;
			_tipoComprobanteServicio = tipoComprobanteServicio;
			_condicionAfipServicio = condicionAfipServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				string titulo = "ORDEN DE PAGO DIRECTA";
				ViewData["Titulo"] = titulo;

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

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
		public IActionResult BuscarTiposDeOrdenDePago()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var model = new FiltroTipoOrdenDePagoModel
				{
					listaTiposOrdenDePago = ComboTipoDeOrdenDePago(TipoDeOrdenDePago.Directa),
					optIdSelected = string.Empty
				};

				return PartialView("_listaTipoOrdenDePago", model);
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
		public JsonResult BuscarTiposOPD(string prefix)
		{
			var top = TipoOrdenDePagoLista.Where(x => x.opt_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.opt_id, Descripcion = x.opt_lista });
			return Json(tipos);
		}

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoOrdenDePagoLista.Count == 0 || actualizar)
				ObtenerTiposDeOrdenDePago(_tipoOrdenDePagoServicio);
			
			if (CondicionesAfipLista.Count == 0 || actualizar)
				ObtenerCondicionesAfip(_condicionAfipServicio);
		}

		protected SelectList ComboTipoComprobante(string afip_id, string opt_id)
		{
			var listaTemp = _tipoComprobanteServicio.BuscarTipoComprobanteListaPorTipoAfip(afip_id, opt_id, Token).Result;
			TiposComprobante = listaTemp;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		protected SelectList ComboOPMotivoCtag(string opt_id)
		{
			var listaTemp = _ordenDePagoServicio.CargarOPMotivosCtag(opt_id, Token);
			OPMotivoCtagDtoLista = listaTemp;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctag_motivo, Descripcion = x.ctag_motivo });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
