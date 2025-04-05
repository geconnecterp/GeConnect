using AspNetCoreGeneratedDocument;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class ComprobanteDeCompraController : ComprobanteDeCompraControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoOpeIvaServicio _tipoOpeServicio;
		private readonly ICondicionAfipServicio _condicionAfipServicio;
		private readonly ITipoProveedorServicio _tipoProveedorServicio;
		private readonly ITipoMonedaServicio _tipoMonedaServicio;
		private readonly ITipoComprobanteServicio _tipoComprobanteServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		private readonly IProducto2Servicio _producto2Servicio;
		private const string PESOS = "PES";
		public ComprobanteDeCompraController(ICuentaServicio cuentaServicio, ITipoOpeIvaServicio tipoOpeIvaServicio, ICondicionAfipServicio condicionAfipServicio,
											 ITipoProveedorServicio tipoProveedorServicio, ITipoMonedaServicio tipoMonedaServicio, ITipoComprobanteServicio tipoComprobanteServicio,
											 IProducto2Servicio producto2Servicio,
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
			_producto2Servicio = producto2Servicio;
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
					return PartialView("_tabCompte", model);

				var response = _cuentaServicio.GetCompteDatosProv(cta_id, TokenCookie).First();
				if (response == null)
					return PartialView("_tabCompte", model);

				model.Moneda = ComboMoneda();
				model.TipoOpe = ComboTipoOpe();
				model.CondAfip = ComboAfip();
				model.TipoCompte = ComboTipoCompte(response.afip_id);
				model.CtaDirecta = ComboTipoGasto();
				model.Cuotas = HelperMvc<ComboGenDto>.ListaGenerica([.. Enumerable.Range(1, 120).Select(x => new ComboGenDto { Id = x.ToString(), Descripcion = x.ToString() })]);
				model.Opciones = ObtenerOpciones(response);
				model.Comprobante = response;
				response.cuotas = 1;
				if (string.IsNullOrEmpty(response.mon_id) || response.mon_id.Equals(string.Empty))
					response.mon_id = PESOS;

				return PartialView("_tabCompte", model);
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
		public IActionResult CargarConceptosFacturados()
		{
			var model = new GridCore<ConceptoFacturadoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (ListaConceptoFacturado != null && ListaConceptoFacturado.Count >= 0)
					model = ObtenerGridCore<ConceptoFacturadoDto>(ListaConceptoFacturado);
				else
					model = ObtenerGridCore<ConceptoFacturadoDto>([]);
				return PartialView("_tabCompte_ConFactu", model);
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
		public IActionResult CargarOtrosTributos()
		{
			var model = new GridCore<OtroTributoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (ListaOtroTributo != null && ListaOtroTributo.Count >= 0)
					model = ObtenerGridCore<OtroTributoDto>(ListaOtroTributo);
				else
					model = ObtenerGridCore<OtroTributoDto>([]);
				return PartialView("_tabCompte_OtrosTrib", model);
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
		public IActionResult CargarGrillaTotales()
		{
			var model = new GridCore<OrdenDeCompraConceptoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (ListaTotales != null && ListaTotales.Count >= 0)
					model = ObtenerGridCore<OrdenDeCompraConceptoDto>(ListaTotales);
				else
					model = ObtenerGridCore<OrdenDeCompraConceptoDto>([]);
				return PartialView("_tabCompte_Totales", model);
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
		public IActionResult ActualizarListaOpciones(string tco_id, string ope_iva)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(tco_id) || string.IsNullOrWhiteSpace(ope_iva))
					return PartialView("_tabCompte_lst_opciones", ComboListaOpciones(tco_id, ope_iva));
				return PartialView("_tabCompte_lst_opciones", ComboListaOpciones(tco_id, ope_iva));
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda de Opciones";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda de Opciones");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}



		[HttpPost]
		public IActionResult ObtenerGrillaDesdeOpcionSeleccionada(string cta_id, int id_selected)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (id_selected > 0)
					id_selected--;
				else
					return PartialView("_empty_view");
				EnOpciones? en = (EnOpciones?)Enum.GetValues(typeof(EnOpciones)).GetValue(id_selected);
				if (en == null)
				{
					return PartialView("_empty_view");
				}
				switch (en)
				{
					case EnOpciones.RPR_ASOCIADA:
						var model = new GridCore<RprAsociadosDto>();
						if (string.IsNullOrEmpty(cta_id))
							return PartialView("_tabCompte_RprAsoc", model);
						var lista = _cuentaServicio.GetCompteCargaRprAsoc(cta_id, TokenCookie);
						model = ObtenerGridCore<RprAsociadosDto>(lista);
						return PartialView("_tabCompte_RprAsoc", model);
					case EnOpciones.NOTAS_A_CUENTA_ASOCIADAS:
						return CargarCompteCargaCtaAsoc(cta_id);
					case EnOpciones.FLETE:
						return PartialView("_empty_view");
					case EnOpciones.PAGO_ANTICIPADO:
						return PartialView("_empty_view");
					default:
						return PartialView("_empty_view");
				}
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
		public IActionResult CargarCompteCargaRprAsoc(string cta_id)
		{
			var model = new GridCore<RprAsociadosDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(cta_id))
					return PartialView("_tabCompte_RprAsoc", model);
				var lista = _cuentaServicio.GetCompteCargaRprAsoc(cta_id, TokenCookie);
				model = ObtenerGridCore<RprAsociadosDto>(lista);
				return PartialView("_tabCompte_RprAsoc", model);
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
		public IActionResult CargarCompteCargaCtaAsoc(string cta_id)
		{
			var model = new GridCore<NotasACuenta>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(cta_id))
					return PartialView("_tabCompte_ACtaAsoc", model);
				var lista = _cuentaServicio.GetCompteCargaCtaAsoc(cta_id, TokenCookie);
				model = ObtenerGridCore<NotasACuenta>(lista);
				return PartialView("_tabCompte_ACtaAsoc", model);
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
		public IActionResult ObtenerDatosModalConceptoFacturado(string tco_id)
		{
			var model = new ModalIvaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(tco_id))
					return PartialView("_empty_view");

				var tco = TiposComprobante.Where(x => x.tco_id.Equals(tco_id)).First();
				if (tco == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"No se ha encontrado el tipo de comprobante {tco_id}"
					};
					return PartialView("_gridMensaje", response);
				}
				if (tco.tco_iva_discriminado == "S")
				{
					model.EsGravado = true;
					model.IvaSituacionLista = ComboIvaSituacionLista();
					model.IvaAlicuotaLista = ComboIvaAlicuotaLista();
				}
				else
				{
					var lista = new List<ComboGenDto>
					{
						new() { Id = "0", Descripcion = "<Sin opciones>" }
					};

					model.EsGravado = false;
					model.IvaSituacionLista = HelperMvc<ComboGenDto>.ListaGenerica(lista);
					model.IvaAlicuotaLista = HelperMvc<ComboGenDto>.ListaGenerica(lista);
				}
				return PartialView("_tabCompte_modal_iva", model);
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

		public JsonResult AgregarConceptoFacturado(string concepto, string sit, decimal ali, decimal subt, decimal iva, decimal tot)
		{
			try
			{
				ListaConceptoFacturado ??= [];
				if (ListaConceptoFacturado.Exists(x=>x.concepto.Equals(concepto)))
					return Json(new { error = true, warn = true, msg = $"El concepto '{concepto}' ya se encuentra en la lista." });
				var newItem = new ConceptoFacturadoDto() { concepto = concepto, cantidad = 1, iva = iva, iva_alicuota = ali, iva_situacion = sit, subtotal = subt, total = tot };
				var listaTemp = new List<ConceptoFacturadoDto>();
				listaTemp = ListaConceptoFacturado;
				listaTemp.Add(newItem);
				ListaConceptoFacturado = listaTemp;

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		#region Métodos Privados
		protected SelectList ComboListaOpciones(string tco_id, string ope_iva)
		{
			var lista = new List<ComboGenDto>();
			var tcoTipo = "";
			if (TiposComprobante != null && TiposComprobante.Count > 0)
				tcoTipo = TiposComprobante.Where(x => x.tco_id.Equals(tco_id)).First().tco_tipo;
			if (ope_iva.Equals("BI") && tcoTipo.Equals("FT"))
			{
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.RPR_ASOCIADA).ToString(), Descripcion = GetDescription(EnOpciones.RPR_ASOCIADA) });
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.FLETE).ToString(), Descripcion = GetDescription(EnOpciones.FLETE) });
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.PAGO_ANTICIPADO).ToString(), Descripcion = GetDescription(EnOpciones.PAGO_ANTICIPADO) });
			}
			else if (ope_iva.Equals("BI") && tcoTipo.Equals("NC"))
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.NOTAS_A_CUENTA_ASOCIADAS).ToString(), Descripcion = GetDescription(EnOpciones.NOTAS_A_CUENTA_ASOCIADAS) });
			else
				lista.Add(new ComboGenDto { Id = "0", Descripcion = "<Sin opciones>" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerOpciones(ComprobanteDeCompraDto response)
		{
			var lista = new List<ComboGenDto>();
			if (response.mostrarGrillaRPR)
			{
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.RPR_ASOCIADA).ToString(), Descripcion = GetDescription(EnOpciones.RPR_ASOCIADA) });
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.FLETE).ToString(), Descripcion = GetDescription(EnOpciones.FLETE) });
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.PAGO_ANTICIPADO).ToString(), Descripcion = GetDescription(EnOpciones.PAGO_ANTICIPADO) });
			}
			if (response.mostrarGrillaNotasACuentaAsociadas)
				lista.Add(new ComboGenDto { Id = ((int)EnOpciones.NOTAS_A_CUENTA_ASOCIADAS).ToString(), Descripcion = GetDescription(EnOpciones.NOTAS_A_CUENTA_ASOCIADAS) });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

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

			if (IvaSituacionLista.Count == 0 || actualizar)
				ObtenerIvaSituacionLista(_producto2Servicio);

			if (IvaAlicuotasLista.Count == 0 || actualizar)
				ObtenerIvaAlicuotasLista(_producto2Servicio);
		}

		protected SelectList ComboTipoCompte(string afip_id)
		{
			var listaTemp = _tipoComprobanteServicio.BuscarTipoComprobanteListaPorTipoAfip(afip_id, Token).Result;
			TiposComprobante = listaTemp;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private enum EnOpciones
		{
			[Description("RPR Asociada")]
			RPR_ASOCIADA = 1,
			[Description("Flete")]
			FLETE,
			[Description("Pago Anticipado")]
			PAGO_ANTICIPADO,
			[Description("Notas a Cuenta Asociadas")]
			NOTAS_A_CUENTA_ASOCIADAS
		}

		private static string GetDescription(Enum value)
		{
			return
				value
					.GetType()
					.GetMember(value.ToString())
					.FirstOrDefault()
					?.GetCustomAttribute<DescriptionAttribute>()
					?.Description
				?? value.ToString();
		}
		#endregion
	}
}
