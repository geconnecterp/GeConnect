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
using gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante.Request;
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
		private readonly IProductoServicio _productoServicio;
		private readonly ITipoDtoValorizaRprServicio _tipoDtoValorizaRprServicio;
		private const string valorizacion_class_blue = "badge bg-label-info-strong me-1";
		private const string valorizacion_class_red = "badge bg-label-danger-strong me-1";

		public ValorizacionDeComprobanteController(ICuentaServicio cuentaServicio, ITipoDtoValorizaRprServicio tipoDtoValorizaRprServicio, IProductoServicio productoServicio,
												   IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ValorizacionDeComprobanteController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_tipoDtoValorizaRprServicio = tipoDtoValorizaRprServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
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
		public IActionResult CargarComprobantesDelProveedorSeleccionado(CargarComprobantesDelProveedorSeleccionadoRequest r)
		{
			var model = new ListaComptePendienteDeValorizarModel();
			try
			{
				CtaIdSelected = r.ctaId;
				CargarComprobantesDelProveedor(r.ctaId, _cuentaServicio);
				model.LstComptePendiente = ComboComptesPendientes();
				model.cm_compte = string.Empty;
				return PartialView("_listaComptesPendientes", model);
			}
			catch (Exception)
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
				RecalcularCostos(responseRpr); //Por pedido de CR pide recalcular los costos por las dudas vengan con errores desde la DB (?)
				CalcularValorizacionDC_DP(responseRpr);
				ComprobantesValorizaDetalleRprListaOriginal = responseRpr; //Resguardo una copia de los valores originales para resguardar
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

				if (responseValorizar != null && responseValorizar.Count > 0)
				{
					model.codigo = responseValorizar.First().resultado.ToString();
					model.mensaje = responseValorizar.First().resultado_msj;

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
					return PartialView("_tabComprobante", model);
				}

				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					EsWarn = false,
					Mensaje = "Ha ocurrido un error al intentar obtener los datos de Valorización."
				};
				return PartialView("_gridMensaje", response);
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

		[HttpPost]
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
						dto_sobre_total_bool = request.dto_sobre_total,
						dto_obs = request.dto_obs
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

		public JsonResult ActualizarOrdenDescFinanc(ActualizarOrdenDescFinancRequest r)
		{
			try
			{
				if (r.listaDesFinanc == null || r.listaDesFinanc.Count <= 0)
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar Lista de Desc. Financieros. Lista vacía." });

				var listaDescFinancTemporal = new List<CompteValorizaDtosListaDto>();
				foreach (var item in r.listaDesFinanc)
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
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar cargar el concepto facturado." });
			}
		}

		public IActionResult ActualizarValorizacion(string cm_compte, bool dif_precio, bool dif_cantidad)
		{
			var model = new GridCoreSmart<CompteValorizaListaDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				model = ObtenerValorizacionActualizada(cm_compte, dif_precio, dif_cantidad);
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
		public JsonResult ActualizarProdEnRprSeccionPrecio(ActualizarProdEnRprSeccionPrecioRequest r)
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
					var producto = productos.FirstOrDefault(x => x.p_id == r.pId);
					if (producto != null)
					{
						if (r.field.Contains("ocd_dto1"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_dto1 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_dto2"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_dto2 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_dto3"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_dto3 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_dto4"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_dto4 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_dto_pa"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_dto_pa = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_plista"))
						{
							r.val = r.val.Replace(",", ".");
							producto.ocd_plista = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("ocd_boni"))
						{
							producto.ocd_boni = r.val;
						}

						producto.ocd_pcosto = Math.Round(CalcularPCosto(producto.ocd_plista, producto.ocd_dto1, producto.ocd_dto2, producto.ocd_dto3, producto.ocd_dto4, producto.ocd_dto_pa, producto.ocd_boni ?? "", 0, producto.rpd_cantidad), 2);
					}
					else
						return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR con identificador {r.pId}" });

					CalcularValorizacionDC_DP(productos);
					ComprobantesValorizaDetalleRprLista = productos; //Actualizo la lista en memoria
					var prodAux = productos.Where(x => x.p_id.Equals(r.pId)).First();
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						costo = producto?.ocd_pcosto.ToString("N2") ?? "0",
						valorizacion_class_dc = prodAux.valorizacion_class_dc,
						valorizacion_class_dp = prodAux.valorizacion_class_dp,
						valorizacion_value_dc = prodAux.valorizacion_value_dc,
						valorizacion_value_dp = prodAux.valorizacion_value_dp,
						valorizacion_mostrar_dc = prodAux.valorizacion_mostrar_dc,
						valorizacion_mostrar_dp = prodAux.valorizacion_mostrar_dp,
						td_dc = $"<span class=\"{prodAux.valorizacion_class_dc}\" style=\"font-size: small;\">{prodAux.valorizacion_value_dc}</span>",
						td_dp = $"<span class=\"{prodAux.valorizacion_class_dp}\" style=\"font-size: small;\">{prodAux.valorizacion_value_dp}</span>"
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {r.pId}" });
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
		public JsonResult ActualizarProdEnRprSeccionFactura(ActualizarProdEnRprSeccionFacturaRequest r)
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
					var producto = productos.FirstOrDefault(x => x.p_id == r.pId);
					if (producto != null)
					{
						if (r.field.Contains("rpd_dto1"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_dto1 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_dto2"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_dto2 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_dto3"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_dto3 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_dto4"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_dto4 = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_dto_pa"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_dto_pa = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_plista"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_plista = Convert.ToDecimal(r.val);
						}
						else if (r.field.Contains("rpd_boni"))
						{
							producto.rpd_boni = r.val;
						}
						else if (r.field.Contains("rpd_cantidad_compte"))
						{
							r.val = r.val.Replace(",", ".");
							producto.rpd_cantidad_compte = Convert.ToDecimal(r.val);
						}

						producto.rpd_pcosto = Math.Round(CalcularPCosto(producto.rpd_plista, producto.rpd_dto1, producto.rpd_dto2, producto.rpd_dto3, producto.rpd_dto4, producto.rpd_dto_pa, producto.rpd_boni ?? "", 0, producto.rpd_cantidad_compte), 2);
					}
					else
						return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR con identificador {r.pId}" });

					CalcularValorizacionDC_DP(productos);
					ComprobantesValorizaDetalleRprLista = productos;
					var prodAux = productos.Where(x => x.p_id.Equals(r.pId)).First();
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						costo = producto?.rpd_pcosto.ToString("N2") ?? "0",
						valorizacion_class_dc = prodAux.valorizacion_class_dc,
						valorizacion_class_dp = prodAux.valorizacion_class_dp,
						valorizacion_value_dc = prodAux.valorizacion_value_dc,
						valorizacion_value_dp = prodAux.valorizacion_value_dp,
						valorizacion_mostrar_dc = prodAux.valorizacion_mostrar_dc,
						valorizacion_mostrar_dp = prodAux.valorizacion_mostrar_dp,
						td_dc = $"<span class=\"{prodAux.valorizacion_class_dc}\" style=\"font-size: small;\">{prodAux.valorizacion_value_dc}</span>",
						td_dp = $"<span class=\"{prodAux.valorizacion_class_dp}\" style=\"font-size: small;\">{prodAux.valorizacion_value_dp}</span>"
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en detalle RPR" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {r.pId}" });
			}
		}

		[HttpPost]
		public JsonResult OCValidar(OCValidarRequest r)
		{
			try
			{
				var res = _productoServicio.OCValidar(r, TokenCookie);
				if (res != null)
				{
					var objRes = res.Result;
					if (objRes.Entidad == null)
						return Json(new { error = true, warn = false, msg = $"Ha ocurrido un error al intentar validar el comprobante." });

					if (objRes.Entidad.resultado != 0)
						return Json(new { error = true, warn = false, msg = objRes.Entidad.resultado_msj });

					return Json(new { error = false, warn = false, msg = string.Empty });
				}
				else
					return Json(new { error = true, warn = false, msg = $"Ha ocurrido un error al intentar validar el comprobante. OC: {r.oc_compte}" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar validar el comprobante. OC: {r.oc_compte}" });
			}
		}

		/// <summary>
		/// Funcion que carga los valores de un producto seleccionado en la Grilla de RPR (Segundo Tab) - Seccion 'Precio OC y Cantidad RP'
		/// y los reemplaza con los valores de la OC seleccionada.
		/// </summary>
		/// <param name="oc_compte"></param>
		/// <param name="idsProds"></param>
		/// <param name="aplica_oc"> Aplica los cambios en los productos del lado de OC</param>
		/// <param name="aplica_fac"> Aplicao los cambios en los productos del lado de Factura</param>
		/// <returns>Retorna la vista parcial con los productos actualizados, en base a la oc_compte proporcionada</returns>
		[HttpPost]

		public IActionResult CargarDetalleRprDesdeOcValidada(string oc_compte, string[] idsProds, bool aplica_oc, bool aplica_fac)
		{
			var model = new TabDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(oc_compte))
					return PartialView("_listaDetalleRpr", model);

				if (idsProds == null || idsProds.Length <= 0)
					return PartialView("_listaDetalleRpr", model);
				var listaProdTemporal = ComprobantesValorizaDetalleRprLista;
				var listaIdsProds = idsProds.ToList();
				var resProdOc = new List<CompteValorizaCostoPorProductoDto>();
				foreach (var item in listaIdsProds)
				{
					var prod = listaProdTemporal.FirstOrDefault(x => x.p_id == item);
					
					if (oc_compte.Equals("relacionada"))
						resProdOc = _cuentaServicio.ObtenerComprobanteValorizaCostoOC(new CompteValorizaCostoOcRequest() { oc_compte = prod?.oc_compte, p_id = item }, TokenCookie);
					else
						resProdOc = _cuentaServicio.ObtenerComprobanteValorizaCostoOC(new CompteValorizaCostoOcRequest() { oc_compte = oc_compte, p_id = item }, TokenCookie);

					if (resProdOc != null && resProdOc.Count > 0)
					{
						if (prod != null)
						{
							var elemento = resProdOc.First();
							if (aplica_oc)
							{
								prod.ocd_plista = elemento.ocd_plista; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_dto1 = elemento.ocd_dto1; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_dto2 = elemento.ocd_dto2; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_dto3 = elemento.ocd_dto3; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_dto4 = elemento.ocd_dto4; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_dto_pa = elemento.ocd_dto_pa; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.ocd_boni = elemento.ocd_boni; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
																   //prod.rpd_pcosto = resProdOc.First().ocd_pcosto;
								prod.rpd_pcosto = CalcularPCosto(elemento.ocd_plista, elemento.ocd_dto1, elemento.ocd_dto2, elemento.ocd_dto3, elemento.ocd_dto4, elemento.ocd_dto_pa, elemento.ocd_boni, 0, prod.rpd_cantidad);
								prod.oc_compte = oc_compte;
							}
							if (aplica_fac)
							{
								prod.rpd_plista = resProdOc.First().ocd_plista; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_dto1 = resProdOc.First().ocd_dto1; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_dto2 = resProdOc.First().ocd_dto2; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_dto3 = resProdOc.First().ocd_dto3; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_dto4 = resProdOc.First().ocd_dto4; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_dto_pa = resProdOc.First().ocd_dto_pa; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
								prod.rpd_boni = resProdOc.First().ocd_boni; //Esperar confirmación sobre si actualizo este valor tambien, o solo el costo
																			//prod.rpd_pcosto = resProdOc.First().ocd_pcosto; 
								prod.rpd_pcosto = CalcularPCosto(elemento.ocd_plista, elemento.ocd_dto1, elemento.ocd_dto2, elemento.ocd_dto3, elemento.ocd_dto4, elemento.ocd_dto_pa, elemento.ocd_boni, 0, prod.rpd_cantidad);
							}
						}
					}
				}
				CalcularValorizacionDC_DP(listaProdTemporal);
				ComprobantesValorizaDetalleRprLista = listaProdTemporal;
				model.GrillaDetalleRpr = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(ComprobantesValorizaDetalleRprLista);
				return PartialView("_listaDetalleRpr", model);
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

		public IActionResult CargarActualizacionPorSeteoMasivo(CompteValorizaSeteoMasivoRequest request)
		{
			var model = new TabDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null || request.idsProductos == null || request.idsProductos.Length <= 0)
					return PartialView("_listaDetalleRpr", model);

				var listaProdTemporal = ComprobantesValorizaDetalleRprLista;
				var listaIdsProds = request.idsProductos.ToList();
				foreach (var item in listaIdsProds)
				{
					var prod = listaProdTemporal.FirstOrDefault(x => x.p_id == item);
					if (prod != null)
					{
						if (request.seccion.Equals(1))  //Precio
						{
							prod.ocd_dto1 = request.dto1;
							prod.ocd_dto2 = request.dto2;
							prod.ocd_dto3 = request.dto3;
							prod.ocd_dto4 = request.dto4;
							prod.ocd_dto_pa = request.dtodpa;
							prod.ocd_boni = request.boni;
							prod.ocd_pcosto = Math.Round(CalcularPCosto(prod.ocd_plista, prod.ocd_dto1, prod.ocd_dto2, prod.ocd_dto3, prod.ocd_dto4, prod.ocd_dto_pa, prod.ocd_boni, 0, prod.rpd_cantidad), 2);
						}
						else //Factura
						{
							prod.rpd_dto1 = request.dto1;
							prod.rpd_dto2 = request.dto2;
							prod.rpd_dto3 = request.dto3;
							prod.rpd_dto4 = request.dto4;
							prod.rpd_dto_pa = request.dtodpa;
							prod.rpd_boni = request.boni;
							prod.rpd_pcosto = Math.Round(CalcularPCosto(prod.rpd_plista, prod.rpd_dto1, prod.rpd_dto2, prod.rpd_dto3, prod.rpd_dto4, prod.rpd_dto_pa, prod.rpd_boni, 0, prod.rpd_cantidad_compte), 2);
						}
					}
				}
				CalcularValorizacionDC_DP(listaProdTemporal);
				ComprobantesValorizaDetalleRprLista = listaProdTemporal;
				model.GrillaDetalleRpr = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(ComprobantesValorizaDetalleRprLista);
				return PartialView("_listaDetalleRpr", model);
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

		public IActionResult CargarDesdeCopiaDeRespaldoListaRpr()
		{
			var model = new TabDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				//ComprobantesValorizaDetalleRprLista = [];
				ComprobantesValorizaDetalleRprLista = [.. ComprobantesValorizaDetalleRprListaOriginal.Select(x => (CompteValorizaDetalleRprListaDto)x.Clone())];
				var grilla = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(ComprobantesValorizaDetalleRprLista);
				model.GrillaDetalleRpr = grilla;
				return PartialView("_listaDetalleRpr", model);
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

		public IActionResult ActualizarProductosSeleccionadosDesdeOcOriginal(string oc_compte, string[] idsProds)
		{
			var model = new TabDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(oc_compte))
					return PartialView("_listaDetalleRpr", model);

				if (idsProds == null || idsProds.Length <= 0)
					return PartialView("_listaDetalleRpr", model);

				var listaProdTemporal = ComprobantesValorizaDetalleRprListaOriginal;
				var listaIdsProds = idsProds.ToList();
				foreach (var item in listaIdsProds)
				{
					var resProdOc = listaProdTemporal.FirstOrDefault(x => x.p_id == item);
					if (resProdOc != null)
					{
						resProdOc.oc_compte = oc_compte;
						resProdOc.rpd_pcosto = resProdOc.ocd_pcosto;
						resProdOc.rpd_plista = resProdOc.ocd_plista;
						resProdOc.rpd_dto1 = resProdOc.ocd_dto1;
						resProdOc.rpd_dto2 = resProdOc.ocd_dto2;
						resProdOc.rpd_dto3 = resProdOc.ocd_dto3;
						resProdOc.rpd_dto4 = resProdOc.ocd_dto4;
						resProdOc.rpd_dto_pa = resProdOc.ocd_dto_pa;
						resProdOc.rpd_boni = resProdOc.ocd_boni;
					}
				}
				CalcularValorizacionDC_DP(listaProdTemporal);
				ComprobantesValorizaDetalleRprLista = listaProdTemporal;
				model.GrillaDetalleRpr = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(ComprobantesValorizaDetalleRprLista);

				return PartialView("_listaDetalleRpr", model);
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

		[HttpPost]
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				CtaIdSelected = string.Empty;
				ComprobantesPendientesDeValorizarLista = [];
				ComprobantesValorizaDescuentosFinancLista = [];
				ComprobantesValorizaDetalleRprLista = [];
				ComprobantesValorizaDetalleRprListaOriginal = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - COMPROBANTEDECOMPRA" });
			}
		}

		/// <summary>
		/// Método que guarda o confirma la valorización de un comprobante.
		/// </summary>
		/// <param name="cmCompte">Identificador del comprobante a valorizar.</param>
		/// <param name="esConfirmacion">Indica si la operación es una confirmación (true) o un guardado (false).</param>
		/// <param name="dif_cantidad">Indica si se debe considerar la diferencia de cantidad.</param>
		/// <param name="dif_precio">Indica si se debe considerar la diferencia de precio.</param>
		/// <returns>Un objeto JSON con el resultado de la operación.</returns>
		[HttpPost]
		public JsonResult GuardarValorizacion(string cmCompte, bool esConfirmacion, bool dif_precio = false, bool dif_cantidad = false)
		{
			try
			{
				// Armado de Request, que es común para ambos servicios
				var compteSeleccionado = ComprobantesPendientesDeValorizarLista
					.Where(x => x.cm_compte.Equals(cmCompte))
					.FirstOrDefault();

				if (compteSeleccionado == null)
				{
					return Json(new { error = true, warn = false, msg = $"Se produjo un error al intentar obtener el comprobante." });
				}

				// Cargar Detalle de Productos RPR
				var jsonResponseRpr = JsonConvert.SerializeObject(ComprobantesValorizaDetalleRprLista, new JsonSerializerSettings());

				// Cargar Detalle de Descuentos Financieros
				var jsonResponseDtos = JsonConvert.SerializeObject(ComprobantesValorizaDescuentosFinancLista, new JsonSerializerSettings());

				// Cargar Datos Valorizados
				var reqValorizados = new CompteValorizaRequest()
				{
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
					json_detalle = jsonResponseRpr,
					json_dtos = jsonResponseDtos,
					usu_id = UserName,
					guarda = true,
					confirma = esConfirmacion,
					dp = dif_precio,
					dc = dif_cantidad,
					adm_id = AdministracionId
				};

				var responseValorizar = _cuentaServicio.ObtenerComprobanteValorizaLista(reqValorizados, TokenCookie);

				if (esConfirmacion)
				{
					return Json(new { error = false, warn = false, msg = "Confirmación correcta." });
				}
				else
				{
					return Json(new { error = false, warn = false, msg = "Guardado correcto." });
				}
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se produjo un error al intentar Guardar/Confirmar la valorización." });
			}
		}

		[HttpPost]
		public JsonResult VerificarErrorEnCalculoDeCostos()
		{
			try
			{
				if (ComprobantesValorizaLista == null)
					return Json(new { error = false, warn = false, msg = "" });
				if (ComprobantesValorizaLista.Count <= 0)
					return Json(new { error = false, warn = false, msg = "" });
				if (ComprobantesValorizaLista.First().resultado == 0)
					return Json(new { error = false, warn = false, msg = "" });

				return Json(new { error = true, warn = false, msg = ComprobantesValorizaLista.First().resultado_msj });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - COMPROBANTEDECOMPRA" });
			}
		}

		#region Métodos privados
		private void RecalcularCostos(List<CompteValorizaDetalleRprListaDto> lista)
		{
			try
			{
				if (lista.Count > 0)
				{
					foreach (var item in lista)
					{
						Console.WriteLine($"ocd_pcosto anterior: {item.ocd_pcosto:N2}");
						item.ocd_pcosto = CalcularPCosto(item.ocd_plista, item.ocd_dto1, item.ocd_dto2, item.ocd_dto3, item.ocd_dto4, item.ocd_dto_pa, item.ocd_boni, 0, item.rpd_cantidad);
						Console.WriteLine($"ocd_pcosto posterior: {item.ocd_pcosto:N2}");
						Console.WriteLine($"///////////////////////////////////////////////////////////////");
						Console.WriteLine($"rpd_pcosto anterior: {item.rpd_pcosto:N2}");
						item.rpd_pcosto = CalcularPCosto(item.rpd_plista, item.rpd_dto1, item.rpd_dto2, item.rpd_dto3, item.rpd_dto4, item.rpd_dto_pa, item.rpd_boni, 0, item.rpd_cantidad_compte);
						Console.WriteLine($"rpd_pcosto posterior: {item.rpd_pcosto:N2}");
						Console.WriteLine($"///////////////////////////////////////////////////////////////");
					}
				}
			}
			catch (Exception)
			{
				throw;
			}

		}

		private void CalcularValorizacionDC_DP(List<CompteValorizaDetalleRprListaDto> lista)
		{
			if (lista.Count > 0)
			{
				foreach (var item in lista)
				{
					var boni = 0.00M;
					if (!string.IsNullOrWhiteSpace(item.ocd_boni))
						boni = CalcularBoni2(item.rpd_boni ?? "", item.rpd_cantidad_compte);
					var result = item.rpd_cantidad - (item.rpd_cantidad_compte + boni);
					if (result == 0)
						item.valorizacion_mostrar_dc = false;
					else
					{
						item.valorizacion_mostrar_dc = true;
						if (result > 0)
						{
							item.valorizacion_class_dc = valorizacion_class_blue;
							item.valorizacion_value_dc = "+";
						}
						else
						{
							item.valorizacion_class_dc = valorizacion_class_red;
							item.valorizacion_value_dc = "-";
						}
					}

					var result2 = Math.Round(item.ocd_pcosto, 2) - Math.Round(item.rpd_pcosto, 2);
					if (result2 == 0)
						item.valorizacion_mostrar_dp = false;
					else
					{
						item.valorizacion_mostrar_dp = true;
						if (result2 > 0)
						{
							item.valorizacion_class_dp = valorizacion_class_blue;
							item.valorizacion_value_dp = "+";
						}
						else
						{
							item.valorizacion_class_dp = valorizacion_class_red;
							item.valorizacion_value_dp = "-";
						}
					}
				}
			}
		}
		private static decimal CalcularPCosto(decimal p_plista, decimal p_d1, decimal p_d2, decimal p_d3, decimal p_d4, decimal p_dpa, string p_boni, decimal flete, decimal cantidad = 0)
		{
			var boni = CalcularBoni2(p_boni, cantidad);
			return p_plista * ((100 - p_d1) / 100) * ((100 - p_d2) / 100) * ((100 - p_d3) / 100) * ((100 - p_d4) / 100) * ((100 - p_dpa) / 100) * boni * ((100 + flete) / 100);
		}

		/// <summary>
		/// Funcion que calcula la bonificacion a agregar a las cantidades
		/// </summary>
		/// <param name="val">valor de tipo string en formato NNN/NNN, Ej: 110/100 -> 110 unidades entregadas por cada 100 compradas, la bonificacion serían los 10</param>
		/// <param name="cant">cantidad de productos comprados</param>
		/// <returns></returns>
		private static int CalcularBoni(string val, decimal cant)
		{
			var boni = 1;
			if (string.IsNullOrWhiteSpace(val))
			{
				return boni;
			}
			var arr = val.Split('/');
			if (!int.TryParse(arr[0], out int num))
			{
				return boni;
			}
			if (!int.TryParse(arr[1], out int den))
			{
				return boni;
			}
			if (num > den)
			{
				return boni;
			}
			var res = den - num; //En la bonificacion viene NNN/MMM donde sería "cada NNN, lleva MMM", siendo MMM mayor a NNN. La diferencia es el valor adicional que se suma al pedido.
			var multiplo = cant / num;
			if (multiplo > 0)
			{
				boni = (res * (int)multiplo);
			}

			return boni;
		}

		private static decimal CalcularBoni2(string val, decimal cant)
		{
			var boni = 1;
			if (string.IsNullOrWhiteSpace(val))
			{
				return boni;
			}
			var arr = val.Split('/');
			if (!int.TryParse(arr[0], out int num))
			{
				return boni;
			}
			if (!int.TryParse(arr[1], out int den))
			{
				return boni;
			}
			if (num > den)
			{
				return boni;
			}
			return Decimal.Divide(den, num);
		}

		private GridCoreSmart<CompteValorizaListaDto> ObtenerValorizacionActualizada(string cm_compte, bool dif_precio = false, bool dif_cantidad = false)
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
					dp = dif_precio,
					dc = dif_cantidad,
					adm_id = AdministracionId
				};
				Console.WriteLine($"////INICIO -> Revalorizar.....////");
				Console.WriteLine($"Json detalle rpr: {jsonResponseRpr}");
				Console.WriteLine($"Json desc financ.: {jsonResponseDtos}");
				var responseValorizar = _cuentaServicio.ObtenerComprobanteValorizaLista(reqValorizados, TokenCookie);
				ComprobantesValorizaLista = responseValorizar;
				Console.WriteLine($"////FIN -> Revalorizar.....////");
				Console.WriteLine($"////RESULTADO -> Revalorizar.....////");
				Console.WriteLine($"Json revalorizacion: {JsonConvert.SerializeObject(responseValorizar, new JsonSerializerSettings())}");
				model = ObtenerGridCoreSmart<CompteValorizaListaDto>(responseValorizar);
				return model;
			}
			catch (Exception)
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
			public string valorizacion_class_dc { get; set; } = string.Empty;
			public string valorizacion_class_dp { get; set; } = string.Empty;
			public string valorizacion_value_dc { get; set; } = string.Empty;
			public string valorizacion_value_dp { get; set; } = string.Empty;
			public bool valorizacion_mostrar_dc { get; set; } = false;
			public bool valorizacion_mostrar_dp { get; set; } = false;
			public string td_dc { get; set; } = string.Empty;
			public string td_dp { get; set; } = string.Empty;
		}
		#endregion
	}
}
