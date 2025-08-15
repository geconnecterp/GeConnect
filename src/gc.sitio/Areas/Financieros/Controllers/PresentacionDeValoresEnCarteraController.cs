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
	public class PresentacionDeValoresEnCarteraController : PresentacionDeValoresEnCarteraControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private const string param_tipo_medio_pago = "TEND";
		public PresentacionDeValoresEnCarteraController(ITipoCuentaFinServicio tipoCuentaFinServicio, IFinancieroServicio financieroServicio,
														IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<PresentacionDeValoresEnCarteraController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			var model = new PresDeValEnCartera_Paso1Model();
			try
			{
				var titulo = "TRANSFERENCIAS DE VALORES EN CARTERA";
				ViewData["Titulo"] = titulo;

				var lista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores(param_tipo_medio_pago, TokenCookie);
				model.ListaTipoMedioDePago = ComboTipoMediosDePago(lista);
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

		public IActionResult SeleccionCuentaFin(string tcf_id)
		{
			var model = new SeleccionCtaFinModel();
			try
			{
				var lista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tcf_id, AdministracionId, TokenCookie);
				model.GrillaCtaFin = ObtenerGridCoreSmart<FinancieroDesdeSeleccionDeTipoDto>(lista);
				return PartialView("_seleccionCtaFin", model);
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

		public IActionResult Paso1()
		{
			var model = new PresDeValEnCartera_Paso1Model();
			try
			{
				var lista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores(param_tipo_medio_pago, TokenCookie);
				model.ListaTipoMedioDePago = ComboTipoMediosDePago(lista);
				return PartialView("_paso1", model);
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

		public IActionResult SeleccionValoresAPresentar(string ctaf_id, string ctaf_desc)
		{
			var model = new SeleccionValoresAPresentarModel();
			try
			{
				var lista = _financieroServicio.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, TokenCookie);
				if (lista != null && lista.Count > 0)
				{
					model.GrillaValoresAPresentar = ObtenerGridCoreSmart<FinancieroCarteraDto>(lista);
					var item = lista.First();
					model.ctaf_id = ctaf_id;
					model.ctaf_desc = ctaf_desc;
					model.titulo_col_1 = item.ins_dato1_desc ?? string.Empty;
					model.titulo_col_2 = item.ins_dato2_desc ?? string.Empty;
					model.titulo_col_3 = item.ins_dato3_desc ?? string.Empty;
				}
				return PartialView("_seleccionValoresAPresentar", model);
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
		protected SelectList ComboTipoMediosDePago(List<TipoCuentaFinDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tcf_id, Descripcion = x.tcf_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
