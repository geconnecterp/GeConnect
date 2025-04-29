using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Tr.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class ValorizacionDeComprobanteController : ValorizacionDeComprobanteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoDtoValorizaRprServicio _tipoDtoValorizaRprServicio;

		public ValorizacionDeComprobanteController(ICuentaServicio cuentaServicio, ITipoDtoValorizaRprServicio tipoDtoValorizaRprServicio,
												   IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ValorizacionDeComprobanteController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_tipoDtoValorizaRprServicio = tipoDtoValorizaRprServicio;
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

		[HttpPost]
		public IActionResult CargarDatosParaValorizar(string cm_compte)
		{
			var model = new TabComprobanteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(cm_compte))
					return PartialView("_tabComprobante", model);

				//Armado de Request, que es común para ambos servicios
				var compteSeleccionado = ComprobantesPendientesDeValorizarLista.Where(x => x.cm_compte.Equals(cm_compte)).First();
				if (compteSeleccionado == null)
					return PartialView("_tabComprobante", model);

				var req = new CompteValorizaRprDtosRequest()
				{
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
				};
				//Cargar Detalle de Productos RPR
				var responseRpr = _cuentaServicio.ObtenerComprobantesDetalleRpr(req, TokenCookie);
				ComprobantesValorizaDetalleRprLista = responseRpr;
				var jsonResponseRpr = JsonConvert.SerializeObject(responseRpr, new JsonSerializerSettings());

				//Cargar Detalle de Descuentos Financieros
				var responseDtos = _cuentaServicio.ObtenerComprobantesDtos(req, TokenCookie);
				ComprobantesValorizaDescuentosFinancLista = responseDtos;
				var jsonResponseDtos = JsonConvert.SerializeObject(responseDtos, new JsonSerializerSettings());

				//Cargar Datos Valorizados
				var reqValorizados = new CompteValorizaRequest()
				{
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
					json_detalle = jsonResponseRpr,
					json_dtos = jsonResponseDtos,
					usu_id = UserName,
					guarda = false,
					confirma = false,
					dp = false,
					dc = false,
					adm_id = AdministracionId
				};
				var responseValorizar = _cuentaServicio.ObtenerComprobanteValorizaLista(reqValorizados, TokenCookie);

				model.GrillaValoracion = ObtenerGridCoreSmart<CompteValorizaListaDto>(responseValorizar);
				model.GrillaDescuentosFin = ObtenerGridCoreSmart<CompteValorizaDtosListaDto>(responseDtos);
				model.ConceptoDtoFinanc = ComboConceptoDescuentoFinanc();
				model.DescFinanc = new CompteValorizaDtosListaDto
				{
					dto_sobre_total = 'S',
					dto_sobre_total_bool = true,
					cm_compte = compteSeleccionado.cm_compte,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id
				};
				///TODO MARCE: Seguir aca
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

		public IActionResult CargarListaDetalleRpr()
		{
			var model = new TabDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				var grilla = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(ComprobantesValorizaDetalleRprLista);
				model.GrillaDetalleRpr = grilla;
				return PartialView("_tabDetalleRpr", model);
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

		public IActionResult AgregarDescFinanc(CompteValorizarAgregarDescFinanRequest request)
		{
			var model = new GridCoreSmart<CompteValorizaDtosListaDto>();
			try
			{
				if (request != null)
				{
					var newItem = new CompteValorizaDtosListaDto
					{
						cm_compte = request.cm_compte,
						dia_movi = request.dia_movi,
						tco_id = request.tco_id,
						item = 0,
						dto = request.dto,
						dto_importe = request.dto_importe,
						dto_fijo = request.dto_fijo ? 'S' : 'N',
						dto_sobre_total = request.dto_sobre_total ? 'S' : 'N',
						dtoc_id = request.dtoc_id,
						dtoc_desc = request.dtoc_desc,
						dto_fijo_bool = request.dto_fijo,
						dto_sobre_total_bool = request.dto_sobre_total
					};
					var listaDescuentosFinancTemp = ComprobantesValorizaDescuentosFinancLista;
					var maxIdx = 0;
					if (listaDescuentosFinancTemp.Count > 0)
						maxIdx = listaDescuentosFinancTemp.Max(x => x.item);

					maxIdx++;
					newItem.item = maxIdx;
					listaDescuentosFinancTemp.Add(newItem);
					ComprobantesValorizaDescuentosFinancLista = listaDescuentosFinancTemp;
					model = ObtenerGridCoreSmart<CompteValorizaDtosListaDto>(ComprobantesValorizaDescuentosFinancLista);
					//TODO MARCE: Recalcular Valorizacion con el nuevo json de descuentos financ.
				}

				return PartialView("_listaDescFinanc", model);
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

		public JsonResult ActualizarOrdenDescFinanc(List<CompteValorizarAgregarDescFinanRequest> listaDesFinanc)
		{
			try
			{
				if (listaDesFinanc == null || listaDesFinanc.Count <= 0)
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar Lista de Desc. Financieros. Lista vacía." });

				var listaDescFinancTemporal = new List<CompteValorizaDtosListaDto>();
				foreach (var item in listaDesFinanc)
				{
					var newItem = new CompteValorizaDtosListaDto
					{
						cm_compte = item.cm_compte,
						dia_movi = item.dia_movi,
						dto = item.dto,
						dtoc_desc = item.dtoc_desc,
						dtoc_id = item.dtoc_id,
						dto_fijo = item.dto_fijo ? 'S' : 'N',
						dto_fijo_bool = item.dto_fijo,
						dto_importe = item.dto_importe,
						dto_sobre_total = item.dto_sobre_total ? 'S' : 'N',
						dto_sobre_total_bool = item.dto_sobre_total,
						item = item.item,
						tco_id = item.tco_id
					};
					listaDescFinancTemporal.Add(newItem);
				}
				if (listaDescFinancTemporal.Count > 0)
				{
					ComprobantesValorizaDescuentosFinancLista = listaDescFinancTemporal;
				}
				else
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar Lista de Desc. Financieros. Error al parsear datos." });

				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		public IActionResult ActualizarValorizacion(string cm_compte)
		{
			var model = new GridCoreSmart<CompteValorizaListaDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				model = ObtenerValorizacionActualizada(cm_compte);
				return PartialView("_listaValorizacion", model);
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

		/// <summary>
		/// Funcion que actualiza los valores de un producto seleccionado en la Grilla de RPR (Segundo Tab) - Seccion 'Precio OC y Cantidad RP'
		/// Los valores a actualizar son 'Precio Costo'
		/// </summary>
		/// <param name="pId">ID del producto seleccionado</param>
		/// <param name="field">Campo que se ha editado, los cuales pueden ser: Precio de Lista, Dto1, Dto2, Dto3, Dto4, DtoPa, Boni</param>
		/// <param name="val">Valor correspondiente al campo editado</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult ActualizarProdEnRprSeccionPrecio(string pId, string field, string val)
		{
			List<CompteValorizaDetalleRprListaDto> productos = [];
			try
			{
				if (ComprobantesValorizaDetalleRprLista != null && ComprobantesValorizaDetalleRprLista.Count > 0)
				{
					productos = ComprobantesValorizaDetalleRprLista;
				}
				if (productos.Count > 0)
				{
					var producto = productos.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						if (field.Contains("ocd_dto1"))
						{
							val = val.Replace(",", ".");
							producto.ocd_dto1 = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_dto2"))
						{
							val = val.Replace(",", ".");
							producto.ocd_dto2 = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_dto3"))
						{
							val = val.Replace(",", ".");
							producto.ocd_dto3 = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_dto4"))
						{
							val = val.Replace(",", ".");
							producto.ocd_dto4 = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_dto_pa"))
						{
							val = val.Replace(",", ".");
							producto.ocd_dto_pa = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_plista"))
						{
							val = val.Replace(",", ".");
							producto.ocd_plista = Convert.ToDecimal(val);
						}
						else if (field.Contains("ocd_boni"))
						{
							producto.ocd_boni = val;
						}

						producto.ocd_pcosto = Math.Round(ProductoParaOcDto.CalcularPCosto(producto.ocd_plista, producto.ocd_dto1, producto.ocd_dto2, producto.ocd_dto3, producto.ocd_dto4, producto.ocd_dto_pa, producto.ocd_boni, 0), 2);
					}
					ComprobantesValorizaDetalleRprLista = productos; //Actualizo la lista en memoria
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						costo = producto?.ocd_pcosto.ToString("N2") ?? "0",
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {pId}" });
			}
		}

		/// <summary>
		/// Funcion que actualiza los valores de un producto seleccionado en la Grilla de RPR (Segundo Tab) - Seccion 'Factura'
		/// Los valores a actualizar son 'Precio Costo'
		/// </summary>
		/// <param name="pId">ID del producto seleccionado</param>
		/// <param name="field">Campo que se ha editado, los cuales pueden ser: Precio de Lista, Dto1, Dto2, Dto3, Dto4, DtoPa, Boni</param>
		/// <param name="val">Valor correspondiente al campo editado</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult ActualizarProdEnRprSeccionFactura(string pId, string field, string val)
		{
			List<CompteValorizaDetalleRprListaDto> productos = [];
			try
			{
				if (ComprobantesValorizaDetalleRprLista != null && ComprobantesValorizaDetalleRprLista.Count > 0)
				{
					productos = ComprobantesValorizaDetalleRprLista;
				}
				if (productos.Count > 0)
				{
					var producto = productos.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						if (field.Contains("rpd_dto1"))
						{
							val = val.Replace(",", ".");
							producto.rpd_dto1 = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_dto2"))
						{
							val = val.Replace(",", ".");
							producto.rpd_dto2 = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_dto3"))
						{
							val = val.Replace(",", ".");
							producto.rpd_dto3 = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_dto4"))
						{
							val = val.Replace(",", ".");
							producto.rpd_dto4 = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_dto_pa"))
						{
							val = val.Replace(",", ".");
							producto.rpd_dto_pa = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_plista"))
						{
							val = val.Replace(",", ".");
							producto.rpd_plista = Convert.ToDecimal(val);
						}
						else if (field.Contains("rpd_boni"))
						{
							producto.rpd_boni = val;
						}

						producto.rpd_pcosto = Math.Round(ProductoParaOcDto.CalcularPCosto(producto.rpd_plista, producto.rpd_dto1, producto.rpd_dto2, producto.rpd_dto3, producto.rpd_dto4, producto.rpd_dto_pa, producto.rpd_boni, 0), 2);
					}
					ComprobantesValorizaDetalleRprLista = productos; //Actualizo la lista en memoria
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						costo = producto?.rpd_pcosto.ToString("N2") ?? "0",
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {pId}" });
			}
		}

		public IActionResult QuitarDescFinanc(int item)
		{
			var model = new GridCoreSmart<CompteValorizaDtosListaDto>();
			try
			{
				if (ComprobantesValorizaDescuentosFinancLista != null && ComprobantesValorizaDescuentosFinancLista.Count > 0)
				{
					var idx = 1;
					var listaDescuentosFinancTemp = ComprobantesValorizaDescuentosFinancLista;
					var listaDescuentosFinanc = listaDescuentosFinancTemp.Where(x => !x.item.Equals(item)).ToList();
					listaDescuentosFinanc.ForEach(x => x.item = idx++);
					ComprobantesValorizaDescuentosFinancLista = listaDescuentosFinanc;
					model = ObtenerGridCoreSmart<CompteValorizaDtosListaDto>(ComprobantesValorizaDescuentosFinancLista);
					//TODO MARCE: Recalcular Valorizacion con el nuevo json de descuentos financ.
				}
				return PartialView("_listaDescFinanc", model);
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

		private GridCoreSmart<CompteValorizaListaDto> ObtenerValorizacionActualizada(string cm_compte)
		{
			var model = new GridCoreSmart<CompteValorizaListaDto>();
			try
			{
				//Armado de Request, que es común para ambos servicios
				var compteSeleccionado = ComprobantesPendientesDeValorizarLista.Where(x => x.cm_compte.Equals(cm_compte)).First();
				if (compteSeleccionado == null)
					return model;

				//Cargar Detalle de Productos RPR
				var jsonResponseRpr = JsonConvert.SerializeObject(ComprobantesValorizaDetalleRprLista, new JsonSerializerSettings());

				//Cargar Detalle de Descuentos Financieros
				var jsonResponseDtos = JsonConvert.SerializeObject(ComprobantesValorizaDescuentosFinancLista, new JsonSerializerSettings());

				//Cargar Datos Valorizados
				var reqValorizados = new CompteValorizaRequest()
				{
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
					json_detalle = jsonResponseRpr,
					json_dtos = jsonResponseDtos,
					usu_id = UserName,
					guarda = false,
					confirma = false,
					dp = false,
					dc = false,
					adm_id = AdministracionId
				};
				Console.WriteLine($"Json detalle rpr: {jsonResponseRpr}");
				Console.WriteLine($"Json desc financ.: {jsonResponseDtos}");
				var responseValorizar = _cuentaServicio.ObtenerComprobanteValorizaLista(reqValorizados, TokenCookie);
				model = ObtenerGridCoreSmart<CompteValorizaListaDto>(responseValorizar);
				return model;
			}
			catch (Exception ex)
			{
				return model;
			}
		}


		private void SetearValorAConcepto(List<CompteValorizaDtosListaDto> lista)
		{
			foreach (var item in lista)
			{
				var aux = TipoDescValorizaRprLista.Where(x => x.dtoc_id.Equals(item.dtoc_id)).FirstOrDefault();
				if (aux != null)
				{
					item.dtoc_desc = aux.dtoc_desc;
				}
				else
				{
					item.dtoc_desc = string.Empty;
				}
			}
		}

		protected SelectList ComboComptesPendientes()
		{
			var lista = ComprobantesPendientesDeValorizarLista.Select(x => new ComboGenDto { Id = x.cm_compte.ToString(), Descripcion = $"{x.tco_desc} ({x.tco_id}) {x.cm_compte} {x.cm_fecha.ToShortDateString()} {x.cm_total}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);

			if (TipoDescValorizaRprLista.Count == 0 || actualizar)
				ObtenerTipoDescValorizaRpr(_tipoDtoValorizaRprServicio);

		}
		protected void CargarComprobantesDelProveedor(string ctaId, ICuentaServicio _cuentaServicio)
		{
			var adms = _cuentaServicio.ObtenerComprobantesPendientesDeValorizar(ctaId, TokenCookie);
			ComprobantesPendientesDeValorizarLista = adms;
		}
		#endregion

		#region Clases Locales
		private class msgRes()
		{
			public bool error { get; set; }
			public bool warn { get; set; }
			public string msg { get; set; } = string.Empty;
			public string costo { get; set; } = string.Empty;
		}
		#endregion
	}
}
