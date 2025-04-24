using AspNetCoreGeneratedDocument;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
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
		private readonly ITipoTributoServicio _tipoTributoServicio;
		private const string PESOS = "PES";
		private const string GRAVADA = "G";
		private const string NO_GRAVADA = "N";
		private const string EXENTO = "E";
		public ComprobanteDeCompraController(ICuentaServicio cuentaServicio, ITipoOpeIvaServicio tipoOpeIvaServicio, ICondicionAfipServicio condicionAfipServicio,
											 ITipoProveedorServicio tipoProveedorServicio, ITipoMonedaServicio tipoMonedaServicio, ITipoComprobanteServicio tipoComprobanteServicio,
											 IProducto2Servicio producto2Servicio, ITipoTributoServicio tipoTributoServicio,
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
			_tipoTributoServicio = tipoTributoServicio;
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
			var model = new GridCoreSmart<ConceptoFacturadoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (ListaConceptoFacturado != null && ListaConceptoFacturado.Count >= 0)
					model = ObtenerGridCoreSmart<ConceptoFacturadoDto>(ListaConceptoFacturado);
				else
					model = ObtenerGridCoreSmart<ConceptoFacturadoDto>([]);
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
		public IActionResult CargarOtrosTributos(string tco_id)
		{
			var model = new GridCoreSmart<OtroTributoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
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
				var listTempo = new List<OtroTributoDto>();
				if (tco.tco_iva_discriminado == "S")
				{
					//Busco en la lista de Tipos de tributo los que se deben cargar previamente
					var listaTributos = TiposTributoLista.Where(x => x.carga_aut_discriminado).OrderBy(y => y.orden).ToList();
					if (listaTributos != null && listaTributos.Count > 0)
					{
						foreach (var item in listaTributos)
						{
							var otroTributo = new OtroTributoDto
							{
								ins_id = item.ins_id,
								imp = item.ins_desc,
								base_imp = 0.00M,
								alicuota = 0.00M,
								importe = 0.00M
							};
							if (!ListaOtrosTributos.Exists(x => x.ins_id.Equals(item.ins_id)))
								listTempo.Add(otroTributo);
						}
					}
					else
					{
						listTempo = [];
					}
				}
				if (ListaOtrosTributos != null && ListaOtrosTributos.Count >= 0)
				{
					listTempo.AddRange(ListaOtrosTributos);
					ListaOtrosTributos = listTempo;
					model = ObtenerGridCoreSmart<OtroTributoDto>(ListaOtrosTributos);
				}
				else
					model = ObtenerGridCoreSmart<OtroTributoDto>([]);
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

		private void InicializarListaOtrosTributos(string tco_id)
		{
			var tco = TiposComprobante.Where(x => x.tco_id.Equals(tco_id)).First();
			if (tco == null)
			{
			}
			else
			{
				var listTempo = new List<OtroTributoDto>();
				if (tco.tco_iva_discriminado == "S")
				{
					//Busco en la lista de Tipos de tributo los que se deben cargar previamente
					var listaTributos = TiposTributoLista.Where(x => x.carga_aut_discriminado).OrderBy(y => y.orden).ToList();
					if (listaTributos != null && listaTributos.Count > 0)
					{
						foreach (var item in listaTributos)
						{
							var otroTributo = new OtroTributoDto
							{
								ins_id = item.ins_id,
								imp = item.ins_desc,
								base_imp = 0.00M,
								alicuota = 0.00M,
								importe = 0.00M
							};
							if (!ListaOtrosTributos.Exists(x => x.ins_id.Equals(item.ins_id)))
								listTempo.Add(otroTributo);
						}
					}
					else
					{
						listTempo = [];
					}
				}
				if (ListaOtrosTributos != null && ListaOtrosTributos.Count >= 0)
				{
					listTempo.AddRange(ListaOtrosTributos);
					ListaOtrosTributos = listTempo;
				}
			}
		}

		[HttpPost]
		public JsonResult EditarItemEnOtrosConceptos(string id, decimal val, string idOtroTributoSeleccionado, string tco_id = "")
		{
			try
			{
				//ListaOtrosTributos ??= [];
				if (ListaOtrosTributos == null || ListaOtrosTributos.Count <= 0)
					InicializarListaOtrosTributos(tco_id);

				//ListaOtrosTributos ??= [];
				var importe = 0.00M;
				//Si existe, actualizo el valor indicado en el parametro id (campo)
				if (ListaOtrosTributos.Exists(x => x.ins_id.Equals(idOtroTributoSeleccionado)))
				{
					var listaTempo = ListaOtrosTributos;
					if (id.Contains("base_imp"))
					{
						listaTempo.ForEach(x =>
						{
							if (x.ins_id.Equals(idOtroTributoSeleccionado))
							{
								x.base_imp = val;
								x.importe = Math.Round(((x.base_imp * x.alicuota) / 100) + x.base_imp, 2);
							}
						});
					}
					else if (id.Contains("alicuota"))
					{
						listaTempo.ForEach(x =>
						{
							if (x.ins_id.Equals(idOtroTributoSeleccionado))
							{
								x.alicuota = val;
								x.importe = Math.Round(((x.base_imp * x.alicuota) / 100) + x.base_imp, 2);
							}
						});
					}
					else
					{
						listaTempo.ForEach(x =>
						{
							if (x.ins_id.Equals(idOtroTributoSeleccionado))
								x.importe = val;
						});
					}

					ListaOtrosTributos = listaTempo;
					importe = ListaOtrosTributos.Where(x => x.ins_id.Equals(idOtroTributoSeleccionado)).First().importe;

					ActualizarGrillaTotales_OtrosTributos();
				}

				return Json(new { error = false, warn = false, msg = string.Empty, data = new { insId = idOtroTributoSeleccionado, importe } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		[HttpPost]
		public IActionResult CargarGrillaTotales()
		{
			var model = new GridCoreSmart<OrdenDeCompraConceptoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (ListaTotales != null && ListaTotales.Count > 0)
					model = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(ListaTotales);
				else
				{
					InicializarGrillaTotales();
					if (ListaOtrosTributos != null && ListaOtrosTributos.Count > 0)
						ActualizarItemEnGrillaTotales("OtrosTributos", ListaOtrosTributos.Sum(x => x.importe));
					model = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(ListaTotales);
				}
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
						return CargarCompteCargaRprAsoc(cta_id);
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
			var model = new GridCoreSmart<RprAsociadosDto>();
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
				ListaRprAsociados = lista;
				model = ObtenerGridCoreSmart<RprAsociadosDto>(lista);
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
			var model = new GridCoreSmart<NotasACuenta>();
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
				ListaNotasACuenta = lista;
				model = ObtenerGridCoreSmart<NotasACuenta>(lista);
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
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaNotasACuenta = [];
				ListaRprAsociados = [];
				ListaTotales = [];
				ListaOtrosTributos = [];
				ListaConceptoFacturado = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - COMPROBANTEDECOMPRA" });
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
					model.ConceptoFacturado = new ConceptoFacturadoDto();
					model.EsGravado = true;
					model.IvaSituacionLista = ComboIvaSituacionLista();
					model.IvaAlicuotaLista = ComboIvaAlicuotaLista();
					if (model.IvaAlicuotaLista.SelectedValue != null)
					{
						model.ConceptoFacturado.iva_alicuota = ((IVAAlicuotaDto)model.IvaAlicuotaLista.SelectedValue).IVA_Alicuota;
						model.ConceptoFacturado.iva_situacion = "G";
					}
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

				//sit contiene si es gravado, no gravado y exento
				var maxId = 0;
				if (ListaConceptoFacturado.Count != 0)
					maxId = ListaConceptoFacturado.Max(x => x.id);
				maxId++;
				var newItem = new ConceptoFacturadoDto() { id = maxId, concepto = concepto, cantidad = 1, iva = iva, iva_alicuota = ali, iva_situacion = sit, subtotal = subt, total = tot };
				var listaTemp = new List<ConceptoFacturadoDto>();
				listaTemp = ListaConceptoFacturado;
				listaTemp.Add(newItem);
				ListaConceptoFacturado = listaTemp;

				ActualizarGrillaTotales_ConceptosFacturados();

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		public IActionResult QuitarItemEnConceptoFacturado(int id)
		{
			try
			{
				var model = new GridCoreSmart<ConceptoFacturadoDto>();
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (id <= 0)
				{
					if (ListaConceptoFacturado != null && ListaConceptoFacturado.Count >= 0)
						model = ObtenerGridCoreSmart<ConceptoFacturadoDto>(ListaConceptoFacturado);
					else
						model = ObtenerGridCoreSmart<ConceptoFacturadoDto>([]);
					return PartialView("_tabCompte_ConFactu", model);
				}
				var listaTemp = ListaConceptoFacturado;
				listaTemp = [.. listaTemp.Where(x => !x.id.Equals(id))];
				ListaConceptoFacturado = listaTemp;
				model = ObtenerGridCoreSmart<ConceptoFacturadoDto>(ListaConceptoFacturado);

				ActualizarGrillaTotales_ConceptosFacturados();

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

		public IActionResult ObtenerDatosModalOtrosTributos(string tco_id)
		{
			var model = new ModalOtroConceptoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (TiposTributoLista.Count == 0)
					return PartialView("_empty_view");

				if (string.IsNullOrEmpty(tco_id))
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"Se debe especificar un tipo de comprobante"
					};
					return PartialView("_gridMensaje", response);
				}

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
					model.OtrasPercepcionesLista = ComboTiposTributosParaModalOtrosTributos();
				else
					model.OtrasPercepcionesLista = ComboTiposTributosParaModalOtrosTributosNoDiscrimina();

				return PartialView("_tabCompte_modal_otro_concepto", model);
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

		public JsonResult AgregarOtroTributo(string insId, decimal baseImp, decimal alicuota, decimal importe)
		{
			try
			{
				ListaOtrosTributos ??= [];
				if (string.IsNullOrWhiteSpace(insId))
					return Json(new { error = true, warn = true, msg = $"El concepto es obligatorio." });
				if (ListaOtrosTributos.Exists(x => x.ins_id.Equals(insId)))
					return Json(new { error = true, warn = true, msg = $"El concepto '{insId}' ya se encuentra en la lista." });
				var newItem = new OtroTributoDto() { ins_id = insId, base_imp = baseImp, alicuota = alicuota, importe = importe, imp = TiposTributoLista.Where(x => x.ins_id.Equals(insId)).First().ins_desc };
				var listaTemp = new List<OtroTributoDto>();
				listaTemp = ListaOtrosTributos;
				listaTemp.Add(newItem);
				ListaOtrosTributos = listaTemp;

				ActualizarGrillaTotales_OtrosTributos();

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el tributo." });
			}
		}

		public IActionResult QuitarItemEnOtrosTributos(string id)
		{
			try
			{
				var model = new GridCoreSmart<OtroTributoDto>();
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(id))
				{
					if (ListaOtrosTributos != null && ListaOtrosTributos.Count >= 0)
						model = ObtenerGridCoreSmart<OtroTributoDto>(ListaOtrosTributos);
					else
						model = ObtenerGridCoreSmart<OtroTributoDto>([]);
					return PartialView("_tabCompte_OtrosTrib", model);
				}
				var listaTemp = ListaOtrosTributos;
				listaTemp = [.. listaTemp.Where(x => !x.ins_id.Equals(id))];
				ListaOtrosTributos = listaTemp;
				model = ObtenerGridCoreSmart<OtroTributoDto>(ListaOtrosTributos);
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
		public JsonResult ConfirmarComprobanteDeCompra(ConfirmarComprobanteDeCompraRequest request)
		{
			try
			{
				var json_encabezado = "";
				var json_asociaciones = "";
				var json_concepto = "";
				var json_otros = "";
				if (ListaConceptoFacturado == null || ListaConceptoFacturado.Count <= 0)
					return Json(new { error = true, warn = false, msg = $"No existen conceptos facturados cargados en el comprobante de compra." });
				if (string.IsNullOrEmpty(request.cta_id))
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al selecciona la cuenta." });
				if (request.encabezado == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener los datos del encabezado." });

				if (request.asociaciones != null && request.asociaciones.Count > 0)
					json_asociaciones = JsonConvert.SerializeObject(request.asociaciones, new JsonSerializerSettings());
				else
					json_asociaciones = "{}";

					CompletarEncabezadoConTotales(request.encabezado);
				//json_encabezado = JsonConvert.SerializeObject(request.encabezado, new JsonSerializerSettings());
				json_encabezado = JsonConvert.SerializeObject(request.encabezado, new JsonSerializerSettings());
				json_concepto = JsonConvert.SerializeObject(ListaConceptoFacturado, new JsonSerializerSettings());
				if (ListaOtrosTributos == null || ListaOtrosTributos.Count <= 0)
					InicializarListaOtrosTributos(request.encabezado.tco_id);
				json_otros = ObtenerJsonOtrosTributos(ListaOtrosTributos);
				var req = new CompteCargaConfirmaRequest
				{
					cta_id = request.cta_id,
					adm_id = AdministracionId,
					usu_id = UserName,
					json_encabezado = json_encabezado,
					json_concepto = json_concepto,
					json_otro = json_otros,
					json_relacion = json_asociaciones
				};
				Console.WriteLine($"Json encabezado: {json_encabezado}");
				Console.WriteLine($"Json concepto: {json_concepto}");
				Console.WriteLine($"Json otros: {json_otros}");
				Console.WriteLine($"Json asociaciones: {json_asociaciones}");
				var respuesta = _cuentaServicio.CompteCargaConfirma(req, TokenCookie);
				return AnalizarRespuesta(respuesta, "El Comprobante de Compra se Confirmo con Éxito");
				//return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar confirmar los datos del comprobante de compra" });
			}
		}

		#region Métodos Privados
		private class OtroTributoParaJson
		{
			public string imp { get; set; } = string.Empty;
			public string tipo { get; set; } = string.Empty;
			public string ctaf_id { get; set; } = string.Empty;
			[JsonProperty(PropertyName = "base")]
			public decimal base_imp { get; set; } = 0.00M; //Al armar el json esta columna se debe llamar 'base'
			public decimal alicuota { get; set; } = 0.00M;
			public decimal importe { get; set; } = 0.00M;
		}
		private string ObtenerJsonOtrosTributos(List<OtroTributoDto> lista)
		{
			if (lista == null || lista.Count <= 0)
				return string.Empty;
			var listaTemp = new List<OtroTributoParaJson>();
			foreach (var item in lista)
			{
				var aux = TiposTributoLista.Where(x => x.ins_id.Equals(item.ins_id)).First();
				listaTemp.Add(new OtroTributoParaJson() { alicuota = item.alicuota, base_imp = item.base_imp, imp = item.ins_id, importe = item.importe, tipo = aux.ins_tipo, ctaf_id = "" });
			}
			return JsonConvert.SerializeObject(listaTemp, new JsonSerializerSettings());
		}
		private void CompletarEncabezadoConTotales(Encabezado encabezado)
		{
			//cm_exento
			encabezado.cm_exento = ListaTotales.Where(x => x.id.Equals("NetoExento")).First().Importe;
			//cm_no_gravado
			encabezado.cm_no_gravado = ListaTotales.Where(x => x.id.Equals("NetoNoGravado")).First().Importe;
			//cm_gravado
			encabezado.cm_gravado = ListaTotales.Where(x => x.id.Equals("NetoGravado")).First().Importe;
			//cm_iva
			encabezado.cm_iva = ListaConceptoFacturado.Select(x => x.iva).Sum();
			//encabezado.cm_iva = ListaTotales.Where(x => x.id.Contains("IVA")).Sum(x => x.Importe);
			//cm_total
			encabezado.cm_total = ListaTotales.Where(x => x.id.Equals("total")).First().Importe;

			//Para estos, trabajar con TiposTributoLista
			//Si no hay ningun elemento con importe, por default van 0
			if (!ListaOtrosTributos.Exists(x => x.importe > 0))
			{
				encabezado.cm_otros_ng = 0;
				encabezado.cm_ii = 0;
				encabezado.cm_percepciones = 0;
			}
			else
			{
				//cm_otros_ng
				var listaIdsTemp = TiposTributoLista.Where(x => x.ins_tipo.Equals("O")).Select(y => y.ins_id).ToList();
				encabezado.cm_otros_ng = ListaOtrosTributos.Where(x => listaIdsTemp.Contains(x.ins_id)).Sum(y => y.importe);

				//cm_ii
				listaIdsTemp = TiposTributoLista.Where(x => x.ins_tipo.Equals("I")).Select(y => y.ins_id).ToList();
				encabezado.cm_ii = ListaOtrosTributos.Where(x => listaIdsTemp.Contains(x.ins_id)).Sum(y => y.importe);

				//cm_percepciones
				listaIdsTemp = TiposTributoLista.Where(x => x.ins_tipo.Equals("P") || x.ins_tipo.Equals("R")).Select(y => y.ins_id).ToList();
				encabezado.cm_percepciones = ListaOtrosTributos.Where(x => listaIdsTemp.Contains(x.ins_id)).Sum(y => y.importe);
			}
		}

		private void ActualizarGrillaTotales_ConceptosFacturados()
		{
			if (ListaTotales == null || ListaTotales.Count <= 0)
				InicializarGrillaTotales();
			var listaTotalesTemp = ListaTotales;
			var listaConcFactuTemp = ListaConceptoFacturado;
			//Sumar conceptos Gravados
			if (listaConcFactuTemp.Exists(x => x.iva_situacion.Equals("G")))
			{
				var items_Sum = listaConcFactuTemp.Where(x => x.iva_situacion.Equals("G")).ToList().Sum(x => x.subtotal);
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
				itemListaTotalGravado.Importe = items_Sum;
			}
			else
			{
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
				itemListaTotalGravado.Importe = 0;
			}

			//Sumas conceptos No Gravados
			if (listaConcFactuTemp.Exists(x => x.iva_situacion.Equals("N")))
			{
				var items_Sum = listaConcFactuTemp.Where(x => x.iva_situacion.Equals("N")).ToList().Sum(x => x.subtotal);
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoNoGravado")).First(); //Obtengo el item que corresponde a Neto No Gravado
				itemListaTotalGravado.Importe = items_Sum;
			}
			else
			{
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoNoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
				itemListaTotalGravado.Importe = 0;
			}

			//Sumas conceptos Exentos
			if (listaConcFactuTemp.Exists(x => x.iva_situacion.Equals("E")))
			{
				var items_Sum = listaConcFactuTemp.Where(x => x.iva_situacion.Equals("E")).ToList().Sum(x => x.subtotal);
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoExento")).First(); //Obtengo el item que corresponde a Neto No Gravado
				itemListaTotalGravado.Importe = items_Sum;
			}
			else
			{
				var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoExento")).First(); //Obtengo el item que corresponde a Neto Gravado
				itemListaTotalGravado.Importe = 0;
			}

			//Sumar por Alicuota de IVA
			//Con esas sumas actualizar la grilla de totales, la unida diferencia es que hay que discriminar por Alicuotas, es decir, si tengo un concepto por 21% y otro por 27%, van a haber dos registros (uno para cada uno)
			//Si hay dos registros que aplica 21%, agrego un solo registro para esa alicuota pero sumarizado
			var _index = 2;
			var listaTempAgrupados = listaConcFactuTemp.GroupBy(x => x.iva_alicuota).Select(y => new { alicuota = y.Key, sumaImporte = y.Sum(z => z.iva) }).ToList();
			if (listaTempAgrupados != null && listaTempAgrupados.Count > 0)
			{
				foreach (var item in listaTempAgrupados)
				{
					if (listaTotalesTemp.Exists(x => x.id.Contains(item.alicuota.ToString())))
					{
						var itemListaTotal = listaTotalesTemp.Where(x => x.Concepto.Contains(item.alicuota.ToString())).First();
						itemListaTotal.Importe = item.sumaImporte;
					}
					else
					{
						_index++;
						listaTotalesTemp.Add(new OrdenDeCompraConceptoDto()
						{
							Concepto = $"IVA {item.alicuota}%",
							Importe = item.sumaImporte,
							Orden = _index++,
							id = $"IVA{item.alicuota}"
						});
					}
				}
				var item_Total = listaTotalesTemp.Where(x => x.id.Contains("total")).First();
				item_Total.Importe = listaTotalesTemp.Where(y => y.id.Equals("NetoNoGravado") || y.id.Equals("NetoExento") || y.id.Equals("NetoGravado") || y.id.Equals("OtrosTributos") || y.id.Contains("IVA")).Sum(x => x.Importe);
			}
			ListaTotales = [.. listaTotalesTemp.OrderBy(x => x.Orden)];
		}
		private void ActualizarGrillaTotales_OtrosTributos()
		{
			if (ListaTotales == null || ListaTotales.Count <= 0)
				InicializarGrillaTotales();
			var listaTotalesTemp = ListaTotales;
			var importeTotalParaLista = ListaOtrosTributos.Sum(x => x.importe);
			var item = listaTotalesTemp.Where(x => x.id.Equals("OtrosTributos")).First();
			item.Importe = importeTotalParaLista;
			var item_Total = listaTotalesTemp.Where(x => x.id.Contains("total")).First();
			item_Total.Importe = listaTotalesTemp.Where(y => y.id.Equals("NetoNoGravado") || y.id.Equals("NetoExento") || y.id.Equals("NetoGravado") || y.id.Equals("OtrosTributos")).Sum(x => x.Importe);
			ListaTotales = listaTotalesTemp;
		}
		private void ActualizarItemEnGrillaTotales(string id, decimal val)
		{
			var listaTemp = ListaTotales;
			var item = listaTemp.Where(x => x.id.Equals(id)).First();
			item.Importe = val;
			ListaTotales = listaTemp;
		}

		private void InicializarGrillaTotales()
		{
			var listaTemp = new List<OrdenDeCompraConceptoDto>
			{
				new() { Concepto = "Neto No Gravado", id = "NetoNoGravado", Orden = 0, Importe = 0 },
				new() { Concepto = "Neto Exento", id = "NetoExento", Orden = 1, Importe = 0 },
				new() { Concepto = "Neto Gravado", id = "NetoGravado", Orden = 2, Importe = 0 },
				new() { Concepto = "Otros Tributos", id = "OtrosTributos", Orden = 998, Importe = 0 },
				new() { Concepto = "Total", id = "total", Orden = 999, Importe = 0 }
			};
			ListaTotales = [.. listaTemp.OrderBy(x => x.Orden)];
		}

		protected SelectList ComboTiposTributosParaModalOtrosTributos()
		{
			var lista = TiposTributoLista.Where(x => !x.carga_aut_discriminado).Select(x => new ComboGenDto { Id = x.ins_id.ToString(), Descripcion = x.ins_desc.ToString() });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		protected SelectList ComboTiposTributosParaModalOtrosTributosNoDiscrimina()
		{
			var lista = TiposTributoLista.Where(x => x.carga_aut_no_discriminado).Select(x => new ComboGenDto { Id = x.ins_id.ToString(), Descripcion = x.ins_desc.ToString() });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

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

			if (TiposTributoLista.Count == 0 || actualizar)
				ObtenerTiposTributoLista(_tipoTributoServicio);
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
