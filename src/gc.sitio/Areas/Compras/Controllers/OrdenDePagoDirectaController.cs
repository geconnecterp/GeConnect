using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.OrdenDePagoDirecta;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using static gc.sitio.Areas.Compras.Controllers.OrdenDePagoAProveedorController;

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
		private readonly IProducto2Servicio _producto2Servicio;
		private readonly ITipoTributoServicio _tipoTributoServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		public OrdenDePagoDirectaController(ITipoOrdenDePagoServicio tipoOrdenDePagoServicio, ITipoComprobanteServicio tipoComprobanteServicio, ICondicionAfipServicio condicionAfipServicio,
											IOrdenDePagoServicio ordenDePagoServicio, IProducto2Servicio producto2Servicio, ITipoTributoServicio tipoTributoServicio,
											ITipoGastoServicio tipoGastoServicio,
											IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDePagoDirectaController> logger) : base(options, contexto, logger)
		{
			_settings = options.Value;
			_tipoOrdenDePagoServicio = tipoOrdenDePagoServicio;
			_tipoComprobanteServicio = tipoComprobanteServicio;
			_condicionAfipServicio = condicionAfipServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
			_producto2Servicio = producto2Servicio;
			_tipoTributoServicio = tipoTributoServicio;
			_tipoGastoServicio = tipoGastoServicio;
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
		public IActionResult AceptarDesdeSeleccionarTipoDeOP(string tipoOP)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(tipoOP))
					return PartialView("_empty_view");

				TipoOPSelected = tipoOP;
				var listaTemp = _ordenDePagoServicio.CargarOPMotivosCtag(tipoOP, TokenCookie);
				OPMotivoCtagDtoLista = listaTemp;
				var model = new OPDPaso1Model
				{
					listaCondAfip = ComboAfip(),
					listaTiposComptes = ComboTipoComprobante(string.Empty, tipoOP),
					listaCuentaDirecta = ComboTipoGasto(),
					itemOPD = new AgregarOPDRequest()
					{
						afip_id = string.Empty,
						ctag_id = string.Empty,
						cm_cuit = string.Empty,
						cm_fecha = DateTime.Now,
						cm_compte = string.Empty,
						cm_nombre = string.Empty,
						tco_id = string.Empty,
						ctag_motivo = string.Empty,
					},
					GrillaConceptosFacturados = ObtenerGridCoreSmart<ConceptoFacturadoDto>([]),
					GrillaOtrosTributos = ObtenerGridCoreSmart<OtroTributoDto>([]),
					GrillaConcpetos = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>([]),
					GrillaObligaciones = ObtenerGridCoreSmart<OPDirectaObligacionesDto>([])
				};
				return PartialView("_vistaOPD_paso1", model);
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

				if (TiposComprobante == null || TiposComprobante.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"Debe seleccionar un tipo de comprobante"
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
				var newItem = new ConceptoFacturadoEnOPDDto()
				{
					id = maxId,
					concepto = concepto,
					cantidad = 1,
					iva = Math.Round(iva, 2),
					iva_alicuota = Math.Round(ali, 2),
					iva_situacion = sit,
					subtotal = Math.Round(subt, 2),
					total = Math.Round(tot, 2)
				};
				var listaTemp = new List<ConceptoFacturadoEnOPDDto>();
				listaTemp = ListaConceptoFacturado;
				listaTemp.Add(newItem);
				ListaConceptoFacturado = listaTemp;

				ActualizarGrillaTotales_ConceptosFacturados(null);

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		[HttpPost]
		public IActionResult CargarConceptosFacturados(bool reinicia)
		{
			var model = new GridCoreSmart<ConceptoFacturadoEnOPDDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (reinicia)
					ListaConceptoFacturado = [];

				if (ListaConceptoFacturado != null && ListaConceptoFacturado.Count >= 0)
					model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>(ListaConceptoFacturado);
				else
					model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>([]);
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
		public IActionResult CargarConceptosFacturadosDesdeSeleccion(CargarComprobanteOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null || string.IsNullOrEmpty(request.tco_id) || string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte))
					return PartialView("_empty_view");

				ActualizarConceptosFacturadosDesdeSeleccion(request);
				var model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>(ListaConceptoFacturado);
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
					model = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(ListaTotales ?? []);
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
		public IActionResult CargarGrillaTotalesDesdeSeleccion(CargarComprobanteOPDRequest request)
		{
			var model = new GridCoreSmart<OrdenDeCompraConceptoDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				ActualizarGrillaTotales_OtrosTributos(request, true);
				ActualizarGrillaTotales_ConceptosFacturados(request, true);

				if (ListaTotales != null && ListaTotales.Count > 0)
					model = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(ListaTotales);
				else
					model = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>([]);
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

		public IActionResult QuitarItemEnConceptoFacturado(int id)
		{
			try
			{
				var model = new GridCoreSmart<ConceptoFacturadoEnOPDDto>();
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (id <= 0)
				{
					if (ListaConceptoFacturado != null && ListaConceptoFacturado.Count >= 0)
						model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>(ListaConceptoFacturado);
					else
						model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>([]);
					return PartialView("_tabCompte_ConFactu", model);
				}
				var listaTemp = ListaConceptoFacturado;
				var itemTemp = listaTemp.Where(x => x.id.Equals(id)).First(); //Lo mantengo para buscarlo y quitarlo de la lista de totales
				listaTemp = [.. listaTemp.Where(x => !x.id.Equals(id))];
				ListaConceptoFacturado = listaTemp;
				model = ObtenerGridCoreSmart<ConceptoFacturadoEnOPDDto>(ListaConceptoFacturado);

				//Busco el item de la lista de totales para quitarlo
				if (ListaTotales != null && ListaTotales.Count > 0)
				{
					if (ListaTotales.Any(x => x.Concepto.Contains(itemTemp.iva_alicuota.ToString())))
					{
						var lista22 = ListaTotales.Where(x => !x.Concepto.Contains(itemTemp.iva_alicuota.ToString())).ToList();
						ListaTotales = lista22;
					}

				}

				ActualizarGrillaTotales_ConceptosFacturados(null);

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

		public IActionResult QuitarItemEnOtrosTributos(string id)
		{
			try
			{
				var model = new GridCoreSmart<OtroTributoEnOPDDto>();
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(id))
				{
					if (ListaOtrosTributos != null && ListaOtrosTributos.Count >= 0)
					{
						ActualizarGrillaTotales_OtrosTributos(null);
						model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>(ListaOtrosTributos);
					}
					else
						model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>([]);
					return PartialView("_tabCompte_OtrosTrib", model);
				}
				var listaTemp = ListaOtrosTributos;
				listaTemp = [.. listaTemp.Where(x => !x.ins_id.Equals(id))];
				ListaOtrosTributos = listaTemp;
				ActualizarGrillaTotales_OtrosTributos(null);
				model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>(ListaOtrosTributos);
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

		public JsonResult AgregarOtroTributo(string insId, decimal baseImp, decimal alicuota, decimal importe)
		{
			try
			{
				ListaOtrosTributos ??= [];
				if (string.IsNullOrWhiteSpace(insId))
					return Json(new { error = true, warn = true, msg = $"El concepto es obligatorio." });
				if (ListaOtrosTributos.Exists(x => x.ins_id.Equals(insId)))
					return Json(new { error = true, warn = true, msg = $"El concepto '{insId}' ya se encuentra en la lista." });
				var newItem = new OtroTributoEnOPDDto() { ins_id = insId, base_imp = baseImp, alicuota = alicuota, importe = importe, imp = TiposTributoLista.Where(x => x.ins_id.Equals(insId)).First().ins_desc };
				var listaTemp = new List<OtroTributoEnOPDDto>();
				listaTemp = ListaOtrosTributos;
				listaTemp.Add(newItem);
				ListaOtrosTributos = listaTemp;

				ActualizarGrillaTotales_OtrosTributos(null);

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el tributo." });
			}
		}

		[HttpPost]
		public IActionResult CargarOtrosTributos(string tco_id)
		{
			var model = new GridCoreSmart<OtroTributoEnOPDDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(tco_id)) //Inicializo la grilla cuando viene vacío el parámetro
					return PartialView("_tabCompte_OtrosTrib", ObtenerGridCoreSmart<OtroTributoEnOPDDto>([]));

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
				var listTempo = new List<OtroTributoEnOPDDto>();
				if (tco.tco_iva_discriminado == "S")
				{
					//Busco en la lista de Tipos de tributo los que se deben cargar previamente
					var listaTributos = TiposTributoLista.Where(x => x.carga_aut_discriminado).OrderBy(y => y.orden).ToList();
					if (listaTributos != null && listaTributos.Count > 0)
					{
						foreach (var item in listaTributos)
						{
							var otroTributo = new OtroTributoEnOPDDto
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
					model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>(ListaOtrosTributos);
				}
				else
					model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>([]);
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
		public IActionResult CargarOtrosTributosDesdeSeleccion(CargarComprobanteOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null || string.IsNullOrEmpty(request.tco_id) || string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte))
					return PartialView("_empty_view");

				ActualizarOtrosTributosDesdeSeleccion(request);
				var model = ObtenerGridCoreSmart<OtroTributoEnOPDDto>(ListaOtrosTributos);

				if (ListaOtrosTributos == null || ListaOtrosTributos.Count <= 0)
					return PartialView("_tabCompte_OtrosTrib", ObtenerGridCoreSmart<OtroTributoEnOPDDto>([]));

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
		public JsonResult EditarItemEnOtrosConceptos(string id, decimal val, string idOtroTributoSeleccionado, string tco_id = "")
		{
			try
			{
				if (ListaOtrosTributos == null || ListaOtrosTributos.Count <= 0)
					InicializarListaOtrosTributos(tco_id);

				var importe = 0.00M;
				//Si existe, actualizo el valor indicado en el parametro id (campo)
				if (ListaOtrosTributos != null && ListaOtrosTributos.Exists(x => x.ins_id.Equals(idOtroTributoSeleccionado)))
				{
					var listaTempo = ListaOtrosTributos;
					if (id.Contains("base_imp"))
					{
						listaTempo.ForEach(x =>
						{
							if (x.ins_id.Equals(idOtroTributoSeleccionado))
							{
								x.base_imp = val;
								x.importe = Math.Round(((x.base_imp * x.alicuota) / 100), 2);
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
								x.importe = Math.Round(((x.base_imp * x.alicuota) / 100), 2);
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

					ActualizarGrillaTotales_OtrosTributos(null);
				}

				return Json(new { error = false, warn = false, msg = string.Empty, data = new { insId = idOtroTributoSeleccionado, importe } });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		[HttpPost]
		public JsonResult AgregarItemEnOpdPaso1(AgregarOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = $"La sesión ha finalizado, debe reingresar al sistema." });

				if (request == null)
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				if (string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte) || string.IsNullOrEmpty(request.tco_id))
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				var listaAux = ListaOrdenDePagoDirecta.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).ToList();
				if (listaAux != null && listaAux.Count > 0)
					return Json(new { error = true, warn = true, msg = $"Ya existe un comprobante con los datos ingresados." });

				var nuevoComprobante = new ComprobanteDto
				{
					afip_id = request.afip_id,
					ctag_id = request.ctag_id,
					cm_cuit = request.cm_cuit,
					cm_fecha = request.cm_fecha,
					cm_compte = request.cm_compte,
					cm_nombre = request.cm_nombre,
					tco_id = request.tco_id,
					tco_desc = request.tco_desc,
					ctag_motivo = request.ctag_motivo,
					ctag_desc = request.ctag_desc,
					cm_domicilio = request.cm_domicilio,
					cm_total = ListaConceptoFacturado.Select(x => x.total).Sum() + ListaOtrosTributos.Select(x => x.importe).Sum(),
				};
				ListaConceptoFacturado.ForEach(x => { x.afip_id = request.afip_id; x.cm_cuit = request.cm_cuit; x.tco_id = request.tco_id; x.cm_compte = request.cm_compte; });
				//************
				//Si existen item con 0 en monto, los quitos (ya que han sido precargados cuando se seleccionó el tipo de comprobante)
				var listaAuxOtrosTrib = ListaOtrosTributos.Where(x => x.importe != 0).ToList();
				ListaOtrosTributos = listaAuxOtrosTrib;
				//************
				ListaOtrosTributos.ForEach(x => { x.afip_id = request.afip_id; x.cm_cuit = request.cm_cuit; x.tco_id = request.tco_id; x.cm_compte = request.cm_compte; });
				var nuevaOPD = new OrdenDePagoDirectaDto
				{
					opd = nuevoComprobante,
					listaConceptoFacturado = ListaConceptoFacturado ?? [],
					listaOtrosTributos = ListaOtrosTributos ?? [],
				};

				var listaTemp = ListaOrdenDePagoDirecta;
				listaTemp.Add(nuevaOPD);
				ListaOrdenDePagoDirecta = listaTemp;

				//Limpiamos listas en sesión luego de cargar el item a la lista principal, menos la de valores
				InicializarDatosEnSession(false);

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el agregar el ítem en Obligaciones." });
			}
		}

		[HttpPost]
		public JsonResult EditarItemEnOpdPaso1(AgregarOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = $"La sesión ha finalizado, debe reingresar al sistema." });

				if (request == null)
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				if (string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte) || string.IsNullOrEmpty(request.tco_id))
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				var listaAux = ListaOrdenDePagoDirecta.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).ToList();
				if (listaAux == null || listaAux.Count <= 0)
					return Json(new { error = true, warn = true, msg = $"No existe el comprobante con los datos ingresados." });

				var listaTemp = ListaOrdenDePagoDirecta;
				var itemTemp = listaTemp.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).First();
				itemTemp.opd.ctag_id = request.ctag_id;
				itemTemp.opd.cm_fecha = request.cm_fecha;
				itemTemp.opd.cm_nombre = request.cm_nombre;
				itemTemp.opd.cm_domicilio = request.cm_domicilio;
				itemTemp.opd.ctag_motivo = request.ctag_motivo;
				itemTemp.opd.ctag_desc = request.ctag_desc;
				itemTemp.opd.cm_total = ListaConceptoFacturado.Select(x => x.total).Sum() + ListaOtrosTributos.Select(x => x.importe).Sum();
				itemTemp.listaConceptoFacturado = ListaConceptoFacturado ?? [];
				itemTemp.listaOtrosTributos = ListaOtrosTributos ?? [];
				ListaOrdenDePagoDirecta = listaTemp;

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar editar la obligación seleccionada." });
			}
		}

		[HttpPost]
		public JsonResult EliminarItemEnOpdPaso1(AgregarOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = $"La sesión ha finalizado, debe reingresar al sistema." });

				if (request == null)
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				if (string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte) || string.IsNullOrEmpty(request.tco_id))
					return Json(new { error = true, warn = true, msg = $"Debe completar los campos obligatorios." });

				var listaAux = ListaOrdenDePagoDirecta.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).ToList();
				if (listaAux == null || listaAux.Count <= 0)
					return Json(new { error = true, warn = true, msg = $"No existe el comprobante con los datos ingresados." });

				var listaTemp = ListaOrdenDePagoDirecta.Where(x => !x.opd.afip_id.Equals(request.afip_id) && !x.opd.cm_cuit.Equals(request.cm_cuit) && !x.opd.cm_compte.Equals(request.cm_compte) && !x.opd.tco_id.Equals(request.tco_id)).ToList();
				ListaOrdenDePagoDirecta = listaTemp;

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar eliminar la obligación seleccionada." });
			}
		}

		[HttpPost]
		public IActionResult CargarGrillaObligaciones()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = $"La sesión ha finalizado, debe reingresar al sistema." });

				var listaTemp = new List<OPDirectaObligacionesDto>();
				var listaTem2 = new List<ComprobanteDto>();
				listaTem2.AddRange([.. ListaOrdenDePagoDirecta.Select(x => x.opd)]);
				foreach (var item in listaTem2)
				{
					var opd = new OPDirectaObligacionesDto
					{
						concepto = $"{item.tco_desc} ({item.tco_id}) {item.cm_compte}",
						fecha_vencimiento = item.cm_fecha,
						gasto = item.ctag_desc,
						motivo = item.ctag_motivo,
						imputado = item.cm_total,
						afip_id = item.afip_id,
						cm_cuit = item.cm_cuit,
						cm_compte = item.cm_compte,
						tco_id = item.tco_id,
					};
					listaTemp.Add(opd);
				}
				var model = ObtenerGridCoreSmart<OPDirectaObligacionesDto>(listaTemp);
				return PartialView("_grillaObligaciones", model);
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

		public JsonResult ActualizarTotalesSuperiores()
		{
			try
			{
				//Obligaciones a cancelar
				var tot_ObligacionesCancelar = (decimal)0.00;
				if (ListaOrdenDePagoDirecta != null && ListaOrdenDePagoDirecta.Count > 0)
				{
					var listaTem2 = new List<ComprobanteDto>();
					listaTem2.AddRange([.. ListaOrdenDePagoDirecta.Select(x => x.opd)]);
					tot_ObligacionesCancelar = listaTem2.Sum(x => x.cm_total);
				}

				//Créditos
				var tot_CredYValImputados = (decimal)0.00;
				if (ListaValores != null && ListaValores.Count > 0)
					tot_CredYValImputados = ListaValores.Sum(x => x.op_importe);

				//Diferencia
				var tot_Diferencia = tot_ObligacionesCancelar - tot_CredYValImputados;
				return Json(new { error = false, warn = false, msg = string.Empty, data = new TotalesActualizados() { ObligacionesCancelar = tot_ObligacionesCancelar, CredYValImputados = tot_CredYValImputados, Diferencia = tot_Diferencia } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar calcular los totales. {ex}" });
			}
		}

		[HttpPost]
		public IActionResult CargarDatosDeComprobanteSeleccionado(CargarComprobanteOPDRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null || string.IsNullOrEmpty(request.tco_id) || string.IsNullOrEmpty(request.afip_id) || string.IsNullOrEmpty(request.cm_cuit) || string.IsNullOrEmpty(request.cm_compte))
					return PartialView("_empty_view");

				var item = ListaOrdenDePagoDirecta.Where(x => x.opd.tco_id.Equals(request.tco_id) && x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte)).First();
				if (item == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"No se ha encontrado el comprobante seleccionado."
					};
					return PartialView("_gridMensaje", response);
				}
				var ptoVta = item.opd.cm_compte.Split('-')[0];
				var ptoNro = item.opd.cm_compte.Split('-')[1];
				var model = new ComprobanteModel
				{
					listaCondAfip = ComboAfip(),
					listaTiposComptes = ComboTipoComprobante(item.opd.afip_id, TipoOPSelected),
					listaCuentaDirecta = ComboTipoGasto(),
					itemOPD = new AgregarOPDRequest()
					{
						afip_id = item.opd.afip_id,
						ctag_id = item.opd.ctag_id,
						cm_cuit = item.opd.cm_cuit,
						cm_fecha = item.opd.cm_fecha,
						cm_compte = item.opd.cm_compte,
						cm_nombre = item.opd.cm_nombre,
						tco_id = item.opd.tco_id,
						ctag_motivo = item.opd.ctag_motivo,
						cm_compte_pto_vta = ptoVta,
						cm_compte_pto_nro = ptoNro,
						cm_domicilio = item.opd.cm_domicilio,

					},
				};

				return PartialView("_datosComprobante", model);
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
		public IActionResult InicializarComprobante()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var model = new ComprobanteModel
				{
					listaCondAfip = ComboAfip(),
					listaTiposComptes = ComboTipoComprobante(string.Empty, TipoOPSelected),
					listaCuentaDirecta = ComboTipoGasto(),
					itemOPD = new AgregarOPDRequest()
					{
						afip_id = string.Empty,
						ctag_id = string.Empty,
						cm_cuit = string.Empty,
						cm_fecha = DateTime.Now,
						cm_compte = string.Empty,
						cm_nombre = string.Empty,
						tco_id = string.Empty,
						ctag_motivo = string.Empty,
						cm_compte_pto_vta = string.Empty,
						cm_compte_pto_nro = string.Empty,
						cm_domicilio = string.Empty,
					},
				};
				return PartialView("_datosComprobante", model);
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
		public IActionResult BuscarTiposComptes(string condAfip)
		{
			var lista = ComboTipoComprobante(condAfip, TipoOPSelected);
			var model = new SelectTipoComprobanteModel()
			{
				listaTiposComptes = lista
			};
			return PartialView("_listaTipoComprobante", model);
		}

		[HttpPost]
		public JsonResult BuscarTiposOPD(string prefix)
		{
			var top = TipoOrdenDePagoLista.Where(x => x.opt_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.opt_id, Descripcion = x.opt_lista });
			return Json(tipos);
		}

		[HttpPost]
		public JsonResult BuscarMotivos(string prefix)
		{
			var top = OPMotivoCtagDtoLista.Where(x => x.ctag_motivo.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.ctag_motivo, Descripcion = x.ctag_motivo });
			return Json(tipos);
		}

		/// <summary>
		/// Limpiar las variables de sesion (listas)
		/// </summary>
		/// <param name="limpiaValores">Si es 'true' limpia la lista de Valores a agregar</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult LimpiarVariablesDeSesion(bool limpiaValores = false)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = $"La sesión ha finalizado, debe reingresar al sistema." });
				InicializarDatosEnSession(limpiaValores);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.Message });
			}
		}

		#region Métodos privados
		private void InicializarDatosEnSession(bool limpiaValores = false)
		{
			ListaConceptoFacturado = [];
			ListaOtrosTributos = [];
			ListaTotales = [];
			if (limpiaValores)
				ListaValores = [];
		}

		private void InicializarListaOtrosTributos(string tco_id)
		{
			var tco = TiposComprobante.Where(x => x.tco_id.Equals(tco_id)).First();
			if (tco == null)
			{
			}
			else
			{
				var listTempo = new List<OtroTributoEnOPDDto>();
				if (tco.tco_iva_discriminado == "S")
				{
					//Busco en la lista de Tipos de tributo los que se deben cargar previamente
					var listaTributos = TiposTributoLista.Where(x => x.carga_aut_discriminado).OrderBy(y => y.orden).ToList();
					if (listaTributos != null && listaTributos.Count > 0)
					{
						foreach (var item in listaTributos)
						{
							var otroTributo = new OtroTributoEnOPDDto
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

		private void ActualizarOtrosTributosDesdeSeleccion(CargarComprobanteOPDRequest request)
		{
			var listaAux = ListaOrdenDePagoDirecta.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).First();
			ListaOtrosTributos = listaAux.listaOtrosTributos;
		}

		private void ActualizarConceptosFacturadosDesdeSeleccion(CargarComprobanteOPDRequest request)
		{
			var listaAux = ListaOrdenDePagoDirecta.Where(x => x.opd.afip_id.Equals(request.afip_id) && x.opd.cm_cuit.Equals(request.cm_cuit) && x.opd.cm_compte.Equals(request.cm_compte) && x.opd.tco_id.Equals(request.tco_id)).First();
			ListaConceptoFacturado = listaAux.listaConceptoFacturado;
		}

		private void ActualizarGrillaTotales_OtrosTributos(CargarComprobanteOPDRequest request, bool actualizaDesdeSeleccion = false)
		{
			if (ListaTotales == null || ListaTotales.Count <= 0)
				InicializarGrillaTotales();

			if (actualizaDesdeSeleccion)
				ActualizarOtrosTributosDesdeSeleccion(request);

			var listaTotalesTemp = ListaTotales;
			listaTotalesTemp ??= [];
			var importeTotalParaLista = ListaOtrosTributos.Sum(x => x.importe);
			var item = listaTotalesTemp.Where(x => x.id.Equals("OtrosTributos")).First();
			item.Importe = importeTotalParaLista;
			var item_Total = listaTotalesTemp.Where(x => x.id.Contains("total")).First();
			item_Total.Importe = listaTotalesTemp.Where(y => y.id != null && !y.id.Equals("total")).Sum(x => x.Importe);
			ListaTotales = listaTotalesTemp;
		}

		private void ActualizarGrillaTotales_ConceptosFacturados(CargarComprobanteOPDRequest request, bool actualizaDesdeSeleccion = false)
		{
			if (ListaTotales == null || ListaTotales.Count <= 0)
				InicializarGrillaTotales();

			if (actualizaDesdeSeleccion)
				ActualizarConceptosFacturadosDesdeSeleccion(request);

			var listaTotalesTemp = ListaTotales;
			var listaConcFactuTemp = ListaConceptoFacturado;
			//Sumar conceptos Gravados
			if (listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Exists(x => x.iva_situacion.Equals("G")))
			{
				if (listaTotalesTemp != null)
				{
					var items_Sum = listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Where(x => x.iva_situacion.Equals("G")).ToList().Sum(x => x.subtotal);
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
					itemListaTotalGravado.Importe = items_Sum;
				}
			}
			else
			{
				if (listaTotalesTemp != null)
				{
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
					itemListaTotalGravado.Importe = 0;
				}
			}

			//Sumas conceptos No Gravados
			if (listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Exists(x => x.iva_situacion.Equals("N")))
			{
				if (listaTotalesTemp != null)
				{
					var items_Sum = listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Where(x => x.iva_situacion.Equals("N")).ToList().Sum(x => x.subtotal);
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoNoGravado")).First(); //Obtengo el item que corresponde a Neto No Gravado
					itemListaTotalGravado.Importe = items_Sum;
				}
			}
			else
			{
				if (listaTotalesTemp != null)
				{
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoNoGravado")).First(); //Obtengo el item que corresponde a Neto Gravado
					itemListaTotalGravado.Importe = 0;
				}
			}

			//Sumas conceptos Exentos
			if (listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Exists(x => x.iva_situacion.Equals("E")))
			{
				if (listaTotalesTemp != null)
				{
					var items_Sum = listaConcFactuTemp.Where(y => y.iva_situacion != null).ToList().Where(x => x.iva_situacion.Equals("E")).ToList().Sum(x => x.subtotal);
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoExento")).First(); //Obtengo el item que corresponde a Neto No Gravado
					itemListaTotalGravado.Importe = items_Sum;
				}
			}
			else
			{
				if (listaTotalesTemp != null)
				{
					var itemListaTotalGravado = listaTotalesTemp.Where(x => x.id.Equals("NetoExento")).First(); //Obtengo el item que corresponde a Neto Gravado
					itemListaTotalGravado.Importe = 0;
				}
			}

			if (listaConcFactuTemp.Exists(x => x.iva_situacion == null))
			{
				if (listaTotalesTemp != null)
				{
					var items_Sum = listaConcFactuTemp.Where(x => x.iva_situacion == null).ToList().Sum(y => y.subtotal);
					var itemLista = listaTotalesTemp.Where(x => x.id.Equals("NetoNoGravado")).First(); //Obtengo el item que corresponde a Neto No Gravado
					itemLista.Importe += items_Sum;
				}
			}
			listaTotalesTemp ??= [];
			//Sumar por Alicuota de IVA
			//Con esas sumas actualizar la grilla de totales, la unida diferencia es que hay que discriminar por Alicuotas, es decir, si tengo un concepto por 21% y otro por 27%, van a haber dos registros (uno para cada uno)
			//Si hay dos registros que aplica 21%, agrego un solo registro para esa alicuota pero sumarizado
			//Excluyo de los totales el iva 0.0
			var _index = 2;
			var listaTempAgrupados = listaConcFactuTemp.Where(x => x.iva_alicuota != 0.0M).GroupBy(x => x.iva_alicuota).Select(y => new { alicuota = y.Key, sumaImporte = y.Sum(z => z.iva) }).ToList();
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
			else
			{
				var item_Total = listaTotalesTemp.Where(x => x.id.Contains("total")).First();
				item_Total.Importe = listaTotalesTemp.Where(y => y.id.Equals("NetoNoGravado") || y.id.Equals("NetoExento") || y.id.Equals("NetoGravado") || y.id.Equals("OtrosTributos") || y.id.Contains("IVA")).Sum(x => x.Importe);
			}
			ListaTotales = [.. listaTotalesTemp.OrderBy(x => x.Orden)];
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

		private void ActualizarItemEnGrillaTotales(string id, decimal val)
		{
			var listaTemp = ListaTotales;
			var item = listaTemp.Where(x => x.id.Equals(id)).First();
			item.Importe = val;
			ListaTotales = listaTemp;
		}

		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoOrdenDePagoLista.Count == 0 || actualizar)
				ObtenerTiposDeOrdenDePago(_tipoOrdenDePagoServicio);

			if (CondicionesAfipLista.Count == 0 || actualizar)
				ObtenerCondicionesAfip(_condicionAfipServicio);

			if (IvaAlicuotasLista.Count == 0 || actualizar)
				ObtenerIvaAlicuotasLista(_producto2Servicio);

			if (IvaSituacionLista.Count == 0 || actualizar)
				ObtenerIvaSituacionLista(_producto2Servicio);

			if (TiposTributoLista.Count == 0 || actualizar)
				ObtenerTiposTributoLista(_tipoTributoServicio);

			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);
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
		#endregion
	}
}
