using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using X.PagedList;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class CompraController : ControladorBase
	{
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoComprobanteServicio _tiposComprobantesServicio;
		private readonly AppSettings _appSettings;
		private readonly ILogger<CompraController> _logger;
		private readonly IProductoServicio _productoServicio;
		private readonly IDepositoServicio _depositoServicio;
		private const char TipoCuenta = 'B';

		public CompraController(ILogger<CompraController> logger, IOptions<AppSettings> options, IProductoServicio productoServicio, ICuentaServicio cuentaServicio,
			ITipoComprobanteServicio tipoComprobanteServicio, IDepositoServicio depositoServicio, IHttpContextAccessor context) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_productoServicio = productoServicio;
			_cuentaServicio = cuentaServicio;
			_tiposComprobantesServicio = tipoComprobanteServicio;
			_depositoServicio = depositoServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		#region RECEPCION DE PROVEEDORES - Métodos públicos
		public async Task<IActionResult> RPRAutorizacionesLista()
		{
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}
			ElementoEditado = false;
			JsonDeRP = null;
			RPRComprobanteDeRPSeleccionado = null;
			RPRAutorizacionSeleccionada = null;
			RPRComptesDeRPRegs = null;
			List<AutoComptesPendientesDto> pendientes;
			GridCore<AutoComptesPendientesDto> grid;
			try
			{
				pendientes = await _productoServicio.RPRObtenerComptesPendiente(AdministracionId, TokenCookie);
				RPRAutorizacionesPendientesEnRP = ArmarDataRow(pendientes);
				grid = ObtenerAutorizacionPendienteGrid(RPRAutorizacionesPendientesEnRP);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
				TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
				grid = new();
			}
			return View(grid);
		}

		private List<AutoComptesPendientesDto> ArmarDataRow(List<AutoComptesPendientesDto> lista)
		{
			var returnedList = new List<AutoComptesPendientesDto>();
			lista = [.. lista.OrderBy(x => x.Rp)];
			foreach (var item in lista)
			{
				if (returnedList.Count == 0)
				{
					item.Rp_hidden = item.Rp;
					returnedList.Add(item);
					continue;
				}
				if (returnedList.Exists(x => x.Rp == item.Rp))
				{
					returnedList.Add(ObtenerRPSimplificado(item));
					continue;
				}
				else
				{
					item.Rp_hidden = item.Rp;
					returnedList.Add(item);
				}
			}
			return returnedList;
		}

		private AutoComptesPendientesDto ObtenerRPSimplificado(AutoComptesPendientesDto item)
		{
			return new AutoComptesPendientesDto()
			{
				Cm_compte = item.Cm_compte,
				Cm_fecha = item.Cm_fecha,
				Cm_importe = item.Cm_importe,
				Cta_denominacion = string.Empty,
				Cta_id = item.Cta_id,
				Fecha = null,
				Nota = string.Empty,
				Rp = string.Empty,
				Rpe_desc = item.Rpe_desc,
				Rpe_id = item.Rpe_id,
				Tco_desc = item.Tco_id,
				Tco_id = item.Tco_id,
				Usu_id = item.Usu_id,
				Rp_hidden = item.Rp,
			};
		}

		public async Task<IActionResult> VerAut(string rp)
		{
			try
			{
				var lista = new List<RPRItemVerCompteDto>();
				var model = new RPRVerAutoDto();
				JsonDeRPVerCompte = ObtenerComprobantesDesdeJson(rp).Result;
				var adms = _tiposComprobantesServicio.BuscarTiposComptesPorCuenta(JsonDeRPVerCompte.encabezado.First().Cta_id, TokenCookie).GetAwaiter().GetResult();
				TiposComprobantePorCuenta = adms;
				RPRComptesDeRPRegs = CargarComprobantesDeRPDesdeJson(JsonDeRPVerCompte.encabezado);
				var comptes = RPRComptesDeRPRegs;
				model.Comprobantes = comptes;
				model.ComboDeposito = ComboDepositos();
				model.Rp = rp;
				var objeto = JsonDeRPVerCompte.encabezado.FirstOrDefault();
				var cuenta = await _cuentaServicio.ObtenerListaCuentaComercial(objeto?.Cta_id, TipoCuenta, TokenCookie);
				var detalleVerCompte = await _productoServicio.RPRObtenerItemVerCompte(rp, TokenCookie);
				if (detalleVerCompte != null)
				{
					RPRItemVerCompteLista = detalleVerCompte;
				}
				var detalleVerConteos = await _productoServicio.RPRObtenerItemVerConteos(rp, TokenCookie);
				if (detalleVerConteos != null)
				{

					foreach (var item in detalleVerConteos)
					{
						item.Row_color = ObtenerColor(item.No_recibido);
					}
					RPRItemVerConteoLista = detalleVerConteos;
				}

				model.Depo_id = objeto?.Depo_id;
				model.Leyenda = $"Autorización RP {rp} Cuenta: {cuenta.FirstOrDefault().Cta_Denominacion} ({objeto.Cta_id}) Turno: {FormateoDeFecha(objeto.Turno, FechaTipoFormato.PARAUSUARIO)}";
				//Cargar conteos x UL
				var conteosxul = await _productoServicio.RPRxUL(rp, TokenCookie);
				model.ConteosxUL = conteosxul;
				return PartialView("RPRVerAutorizacion", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar abrir pantalla Ver Autorización.");
				TempData["error"] = "Hubo algun problema al intentar abrir pantalla Ver Autorización. Si el problema persiste informe al Administrador";
				return null;
			}

		}

		public async Task<IActionResult> BuscarDetalleULxRPR(string ul_id)
		{
			GridCore<RPRxULDetalleDto> datosIP = new();
			try
			{
				var detalle = await _productoServicio.RPRxULDetalle(ul_id, TokenCookie);
				datosIP = ObtenerGridCore<RPRxULDetalleDto>(detalle);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener el detalle del comprobante UL.");
				TempData["error"] = "Hubo algun problema al intentar obtener el detalle del comprobante UL. Si el problema persiste informe al Administrador";
				return PartialView("_rprULxRPRDetalle", datosIP);
			}
			return PartialView("_rprULxRPRDetalle", datosIP);
		}

		public async Task<IActionResult> ObtenerDetalleVerCompte(string rp, string tco_id, string cc_compte)
		{
			GridCore<RPRItemVerCompteDto> datosIP = new();
			var lista = new List<RPRItemVerCompteDto>();
			var detalleVerCompte = new List<RPRItemVerCompteDto>();
			try
			{
				if (RPRItemVerCompteLista != null)
				{
					lista = RPRItemVerCompteLista;
					foreach (var item in lista)
					{
						item.Row_color = ObtenerColor(item.No_recibido);
					}
					RPRItemVerCompteLista = lista;
					datosIP = ObtenerGridCore(RPRItemVerCompteLista.Where(x => x.Tco_id == tco_id && x.Cm_compte == cc_compte).ToList());
				}
				else
				{
					detalleVerCompte = await _productoServicio.RPRObtenerItemVerCompte(rp, TokenCookie);
					if (detalleVerCompte != null)
					{
						lista = detalleVerCompte.Where(x => x.Tco_id == tco_id && x.Cm_compte == cc_compte).ToList();
						lista.ForEach(x => x.Row_color = ObtenerColor(x.No_recibido));
						datosIP = ObtenerGridCore(lista);
					}
					RPRItemVerCompteLista = detalleVerCompte;
				}
				return PartialView("_rprDetalleVerCompte", datosIP);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener el detalle del comprobante RPR.");
				TempData["error"] = "Hubo algun problema al intentar obtener el detalle del comprobante RPR. Si el problema persiste informe al Administrador";
				return PartialView("_rprDetalleVerCompte", datosIP);
			}
		}



		public async Task<IActionResult> BuscarDetalleVerConteoSeleccionado(string p_id)
		{
			GridCore<RPRItemVerCompteDto> datosIP = new();
			var lista = new List<RPRItemVerCompteDto>();
			try
			{
				lista = RPRItemVerCompteLista.Where(x => x.P_id == p_id).ToList();
				datosIP = ObtenerGridCore(lista);
				return PartialView("_rprDetalleVerConteoSeleccionado", datosIP);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener el detalle conteo para el compte seleccionado.");
				TempData["error"] = "Hubo algun problema al intentar obtener el detalle conteo para el compte seleccionado. Si el problema persiste informe al Administrador";
				return PartialView("_rprDetalleVerConteoSeleccionado", datosIP);
			}
		}

		public async Task<IActionResult> ObtenerDetalleVerConteos()
		{
			GridCore<RPRVerConteoDto> datosIP = new();
			if (RPRItemVerConteoLista == null)
			{
				RPRItemVerConteoLista = [];
			}
			datosIP = ObtenerGridCore(RPRItemVerConteoLista);
			return PartialView("_rprDetalleVerConteos", datosIP);
		}

		public async Task<IActionResult> ObtenerVerComptesDeRP(string rp)
		{
			var model = new List<RPRComptesDeRPDto>();
			JsonDeRPVerCompte = ObtenerComprobantesDesdeJson(rp).Result;
			RPRComptesDeRPRegs = CargarComprobantesDeRPDesdeJson(JsonDeRPVerCompte.encabezado);
			var comptes = RPRComptesDeRPRegs;
			model = comptes;
			return PartialView("_rprVerComptesDeRP", model);
		}

		private async Task<JsonDeRPDto> ObtenerComprobantesDesdeJson(string rp)
		{
			//List<JsonDto> json_string = [];

			try
			{
				JsonDeRPDto resObj = new();
				var res = await _productoServicio.RPObtenerJsonDesdeRP(rp, TokenCookie);
				if (res != null && res.FirstOrDefault() != null)
				{
					resObj = JsonConvert.DeserializeObject<JsonDeRPDto>(res?.FirstOrDefault()?.Json);
					if (resObj != null)
					{
						foreach (var item in resObj.encabezado)
						{
							item.Ope = TipoAltaRP.MODIFICA.ToString();
						}
					}
				}
				return resObj;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener el json desde el comprobante RP.");
				TempData["error"] = "Hubo algun problema al intentar obtener el json desde el comprobante RP. Si el problema persiste informe al Administrador";
				return new JsonDeRPDto();
			}
		}

		/// <summary>
		/// Metodo para cargar y generar el JSON correspondiente al RPR en el que se esta trabajando.
		/// </summary>
		/// <returns></returns>
		public async Task<JsonResult> RPRCargarNuevoComprobante()
		{
			List<RespuestaDto> respuesta = [];
			try
			{
				var json_string = GenerarJsonDesdeJsonEncabezadoDeRPLista();
				respuesta = await _productoServicio.RPRCargarCompte(json_string, TokenCookie);
				if (respuesta == null)
				{
					return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar cargar el comprobante, intente nuevamente mas tarde." });
				}
				else if (respuesta.Count == 0)
				{
					return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar cargar el comprobante, intente nuevamente mas tarde." });
				}
				else
				{
					return Json(new { error = true, warn = false, codigo = respuesta.FirstOrDefault()?.resultado, msg = $"{respuesta.FirstOrDefault()?.resultado_msj} Código ({respuesta.FirstOrDefault()?.resultado})" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar generar y cargar JSON de nuevo comprobante RPR.");
				TempData["error"] = "Hubo algun problema al intentar generar y cargar JSON de nuevo comprobante RPR. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar cargar el comprobante, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> RPREliminarComprobante(string rp)
		{
			List<RespuestaDto> respuesta = [];
			try
			{
				respuesta = await _productoServicio.RPREliminarCompte(rp, TokenCookie);
				if (respuesta == null)
				{
					return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar eliminar el comprobante, intente nuevamente mas tarde." });
				}
				else if (respuesta.Count == 0)
				{
					return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar eliminar el comprobante, intente nuevamente mas tarde." });
				}
				else
				{
					return Json(new { error = false, warn = false, codigo = respuesta.FirstOrDefault()?.resultado, msg = $"{respuesta.FirstOrDefault()?.resultado_msj} Código ({respuesta.FirstOrDefault()?.resultado})" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar eliminar el comprobante RPR.");
				TempData["error"] = "Hubo algun problema al intentar eliminar el comprobante comprobante RPR. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, codigo = 9999, msg = $"Error al intentar eliminar el comprobante, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> NuevaAut(string rp)
		{
			try
			{
				var auth = EstaAutenticado;
				var jsonAux = new JsonDeRPDto();
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				var rpSelected = RPRAutorizacionesPendientesEnRP.Where(x => x.Rp == rp).FirstOrDefault();
				if (rpSelected != default(AutoComptesPendientesDto))
				{
					RPRAutorizacionSeleccionada = rpSelected;
				}
				var model = new BuscarCuentaDto() { ComboDeposito = ComboDepositos(), rp = rp };
				//Precargo el comprobante desde el json
				if (!string.IsNullOrWhiteSpace(rp))
				{
					jsonAux = ObtenerComprobantesDesdeJson(rp).Result;
					//RPRComptesDeRPRegs = [];
					var adms = _tiposComprobantesServicio.BuscarTiposComptesPorCuenta(rpSelected.Cta_id, TokenCookie).GetAwaiter().GetResult();
					TiposComprobantePorCuenta = adms;
				}
				if (RPRComprobanteDeRPSeleccionado != null && string.IsNullOrEmpty(rp))
				{
					model.Cuenta = RPRComprobanteDeRPSeleccionado.cta_id;
					model.Nota = RPRComprobanteDeRPSeleccionado.Nota;
					model.FechaTurno = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado.FechaTurno) ? Convert.ToDateTime(RPRComprobanteDeRPSeleccionado.FechaTurno).ToString("yyyy-MM-dd") : DateTime.Now.ToString("yyyy-MM-dd");
					model.Depo_id = RPRComprobanteDeRPSeleccionado.Depo_id;
					model.CantidadUL = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado.Ul_cantidad) ? Convert.ToInt32(RPRComprobanteDeRPSeleccionado.Ul_cantidad) : 0;
				}
				else if (RPRAutorizacionSeleccionada != null)
				{
					model.Compte = new RPRComptesDeRPDto()
					{
						Fecha = RPRAutorizacionSeleccionada.Fecha?.ToString("yyyy-MM-dd"),
						Importe = RPRAutorizacionSeleccionada.Cm_importe.ToString(),
						NroComprobante = RPRAutorizacionSeleccionada.Cm_compte,
						Tipo = RPRAutorizacionSeleccionada.Tco_id,
						TipoDescripcion = RPRAutorizacionSeleccionada.Tco_desc
					};
					if (jsonAux != null && jsonAux != default(JsonDeRPDto))
					{
						model.Cuenta = jsonAux.encabezado.First().Cta_id;
						model.Nota = jsonAux.encabezado.First().Nota;
						if (int.TryParse(jsonAux.encabezado.First().Ul_cantidad, out int ulCantidad))
							model.CantidadUL = ulCantidad;
						else
							model.CantidadUL = 0;
						model.Depo_id = jsonAux.encabezado.First().Depo_id;
						if (DateTime.TryParse(jsonAux.encabezado.First().Turno, out DateTime fecha))
							model.FechaTurno = fecha.ToString("yyyy-MM-dd");
						else
							model.FechaTurno = DateTime.Now.ToString("yyyy-MM-dd");
						model.FechaTurno = jsonAux.encabezado.First().Turno;
					}
					else
					{
						model.Cuenta = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado?.cta_id) ? RPRComprobanteDeRPSeleccionado?.cta_id : RPRAutorizacionSeleccionada.Cta_id;
						model.Nota = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado?.Nota) ? RPRComprobanteDeRPSeleccionado?.Nota : RPRAutorizacionSeleccionada.Nota;
						model.FechaTurno = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado?.FechaTurno) ? Convert.ToDateTime(RPRComprobanteDeRPSeleccionado.FechaTurno).ToString("yyyy-MM-dd") : RPRAutorizacionSeleccionada.Fecha?.ToString("yyyy-MM-dd");
						model.Depo_id = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado?.Depo_id) ? RPRComprobanteDeRPSeleccionado?.Depo_id : "0";
						model.CantidadUL = !string.IsNullOrWhiteSpace(RPRComprobanteDeRPSeleccionado?.Ul_cantidad) ? Convert.ToInt32(RPRComprobanteDeRPSeleccionado?.Ul_cantidad) : 0;
					}
				}
				if (rp == null)
				{
					model.TituloVista = "Nueva RPR";
				}
				else
				{
					model.TituloVista = $"Autorización RPR N° {rp}";
				}
				return PartialView("RPRNuevaAutorizacion", model);
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

		private void ReCargarRPRComptesDeRPRegs(string rp)
		{
			JsonDeRPVerCompte = ObtenerComprobantesDesdeJson(rp).Result;
			RPRComptesDeRPRegs = CargarComprobantesDeRPDesdeJson(JsonDeRPVerCompte.encabezado);
		}

		public async Task<IActionResult> VerDetalleDeComprobanteDeRP(string idTipoCompte, string nroCompte, string depoSelec, string notaAuto, string turno, string ponerEnCurso, string ulCantidad, string rp = "", string ctaId = "", char tipoCuenta = char.MinValue, string fechaCompte = "", string monto = "", string descTipoCompte = "")
		{
			var compte = new RPRComptesDeRPDto();
			if (CuentaComercialSeleccionada == null)
			{
				CuentaComercialSeleccionada = BuscarCuentaComercial(ctaId, tipoCuenta).Result.FirstOrDefault();
			}
			var model = new RPRDetalleComprobanteDeRP
			{
				Leyenda = $"Carga de Detalle de Comprobante RP Proveedor ({CuentaComercialSeleccionada.Cta_Id}) {CuentaComercialSeleccionada.Cta_Denominacion}"
			};
			if (RPRComptesDeRPRegs == null && !string.IsNullOrWhiteSpace(rp))
			{
				ReCargarRPRComptesDeRPRegs(rp);
			}
			//Aca modifique 23/09
			if (RPRComptesDeRPRegs != null && RPRComptesDeRPRegs.Exists(x => x.Tipo == idTipoCompte && x.NroComprobante == nroCompte && x.Rp == rp))
			{
				compte = RPRComptesDeRPRegs.Where(x => x.Tipo == idTipoCompte && x.NroComprobante == nroCompte && x.Rp == rp).FirstOrDefault();
			}
			if (compte != null && compte != default(RPRComptesDeRPDto))
			{
				model.CompteSeleccionado = compte;
			}
			else
			{
				model.CompteSeleccionado.Fecha = UnixTimeStampToDateTime(fechaCompte).ToString("dd-MM-yyyy"); ;
				model.CompteSeleccionado.Importe = monto;
				model.CompteSeleccionado.NroComprobante = nroCompte;
				model.CompteSeleccionado.TipoDescripcion = descTipoCompte;
			}
			model.cta_id = CuentaComercialSeleccionada.Cta_Id;
			model.ponerEnCurso = bool.Parse(ponerEnCurso);
			model.Nota = notaAuto;
			model.Depo_id = depoSelec;
			model.FechaTurno = UnixTimeStampToDateTime(turno).ToString("dd-MM-yyyy");
			model.Ul_cantidad = ulCantidad;
			RPRComprobanteDeRPSeleccionado = model;
			return PartialView("RPRCargaDetalleDeCompteRP", model);
		}

		public async Task<IActionResult> CargarOCxCuentaEnRP(string cta_id)
		{
			GridCore<RPROrdenDeCompraDto> datosIP;
			var lista = new List<RPROrdenDeCompraDto>();

			lista = await _cuentaServicio.ObtenerListaOCxCuenta(cta_id, TokenCookie);
			datosIP = ObtenerGridCore(lista);
			return PartialView("_rprOCxCuenta", datosIP);
		}

		public async Task<IActionResult> VerDetalleDeOCRP(string oc_compte)
		{
			GridCore<RPROrdenDeCompraDetalleDto> datosIP;
			var lista = new List<RPROrdenDeCompraDetalleDto>();

			lista = await _cuentaServicio.ObtenerDetalleDeOC(oc_compte, TokenCookie);
			datosIP = ObtenerOCDetalleRPGrid(lista);
			return PartialView("_rprOCDetalle", datosIP);
		}


		public async Task<IActionResult> CargarDetalleDeProductosEnRP(string oc_compte, string id_prod, string up, string bulto, string unidad, int accion, string tco_id, string cm_compte, string up_id = "", string p_desc = "", string prov_id = "", string id_barrado = "", string listaProd = "")
		{
			GridCore<ProductoBusquedaDto> datosIP;
			var lista = new List<ProductoBusquedaDto>();

			if (!string.IsNullOrWhiteSpace(oc_compte))
			{
				//Estoy buscando y cargando los productos de una OC seleccionada, siempre y cuando no hayan sido cargados los items de esa OC
				var detalleDeOC = await _cuentaServicio.ObtenerDetalleDeOC(oc_compte, TokenCookie);
				if (detalleDeOC != null && detalleDeOC.Count > 0 && !RPRDetalleDeProductosEnRP.Exists(x => x.oc_compte == oc_compte))
				{
					ElementoEditado = true;
					lista = RPRDetalleDeProductosEnRP;
					var listaProdSeleccionados = listaProd.Split('#');
					foreach (var item in detalleDeOC)
					{
						if (!listaProdSeleccionados.Contains(item.p_id))
						{
							continue;
						}
						var itemTemp = lista.Where(x => x.P_id == item.p_id).FirstOrDefault();
						var itemNextValue = lista.Max(x => x.Item) + 1;
						//No lo encuentra
						if (default(ProductoBusquedaDto) == itemTemp)
						{
							CargarItemEnGrillaDeProductos(lista, item, accion, null, itemNextValue);
						}
						else
						{
							CargarItemEnGrillaDeProductos(lista, item, accion, itemTemp, itemNextValue);
						}
					}
					RPRDetalleDeProductosEnRP = lista;
				}
			}
			//Traigo los prod del almacenamiento temporal
			else if (!string.IsNullOrWhiteSpace(tco_id) && !string.IsNullOrWhiteSpace(cm_compte))
			{
				if (JsonDeRP == null && RPRAutorizacionSeleccionada != null)
				{
					PreCargarJson(RPRAutorizacionSeleccionada.Rp, tco_id, cm_compte);
				}
				if (RPRAutorizacionSeleccionada != null && JsonDeRP != null && JsonDeRP.encabezado.FirstOrDefault().Rp != RPRAutorizacionSeleccionada.Rp)
				{
					PreCargarJson(RPRAutorizacionSeleccionada.Rp, tco_id, cm_compte);
				}
				if (JsonDeRP != null && JsonDeRP.encabezado != null && JsonDeRP.encabezado.Count > 0)
				{
					var encTemp = JsonDeRP.encabezado.Where(x => x.Tco_id == tco_id && x.Cm_compte == cm_compte).FirstOrDefault();
					if (encTemp != null && encTemp.Comprobantes != null)
					{
						var listaIdsDeProductos = encTemp.Comprobantes.Select(x => x.P_id).ToList();
						var listaString = string.Join("@", listaIdsDeProductos);
						if (!string.IsNullOrWhiteSpace(listaString))
						{
							var listaDeProductosAux = ObtenerDatosDeProductos(listaString);
							foreach (var item in encTemp.Comprobantes)
							{
								if (item.Producto == null)
								{
									item.Producto = listaDeProductosAux.Where(x => x.P_id == item.P_id).FirstOrDefault();
									//item.Producto.Cantidad = Convert.ToDecimal(item.Cantidad);
									item.Producto.Cantidad = Convert.ToDecimal((Convert.ToInt32(item.Bulto) * Convert.ToInt32(item.Bulto_up)) + Convert.ToDecimal(item.Uni_suelta));
									item.Producto.Unidad = Convert.ToDecimal(item.Uni_suelta);
									item.Producto.Bulto = Convert.ToInt32(item.Bulto);
									item.Producto.P_unidad_pres = item.Bulto_up;
									item.Producto.Item = item.Item;
									//Cantidad = (item.ocd_unidad_pres * item.ocd_bultos) + item.ocd_unidad_x_bulto,
									lista.Add(item.Producto);
								}
								else
								{
									lista.Add(item.Producto);
								}
							}
						}
					}
				}
				if (!string.IsNullOrWhiteSpace(id_prod)) //Estoy agregando un producto de forma manual
				{
					ElementoEditado = true;
					lista = RPRDetalleDeProductosEnRP;
					//Busco el producto en la lista
					var existeProd = lista.Where(x => x.P_id == id_prod).FirstOrDefault();
					if (existeProd == null || existeProd == default(ProductoBusquedaDto)) //No existe
					{
						lista.Add(new ProductoBusquedaDto()
						{
							P_id = id_prod,
							P_desc = p_desc,
							P_id_prov = prov_id,
							Up_id = up_id,
							P_id_barrado = id_barrado,
							P_unidad_pres = up,
							Bulto = Convert.ToInt32(bulto),
							oc_compte = "",
							Unidad = Convert.ToInt32(unidad),
							Cantidad = CalcularCantidadDeProductoParaAgregar(up_id, bulto, up, unidad),
							Item = RPRDetalleDeProductosEnRP.Count > 0 ? RPRDetalleDeProductosEnRP.Max(x => x.Item) + 1 : 1
						});
					}
					else
					{
						if (accion == 1)//Reemplazar
						{
							var ubicacionActual = existeProd.Item;
							lista.Remove(existeProd);
							lista.Add(new ProductoBusquedaDto()
							{
								P_id = id_prod,
								P_desc = p_desc,
								P_id_prov = prov_id,
								Up_id = up_id,
								P_id_barrado = id_barrado,
								P_unidad_pres = up,
								Bulto = Convert.ToInt32(bulto),
								oc_compte = "",
								Unidad = Convert.ToInt32(unidad),
								Cantidad = CalcularCantidadDeProductoParaAgregar(up_id, bulto, up, unidad),
								Item = ubicacionActual
							});
						}
						else //Acumular
						{
							var itemAQuitar = existeProd;
							lista.Remove(itemAQuitar);
							existeProd.Bulto = existeProd.Bulto + Convert.ToInt32(bulto);
							existeProd.P_unidad_pres = (Convert.ToInt32(existeProd.P_unidad_pres) + Convert.ToInt32(up)).ToString();
							existeProd.Unidad = existeProd.Unidad + Convert.ToInt32(unidad);
							existeProd.Cantidad = existeProd.Cantidad + CalcularCantidadDeProductoParaAgregar(up_id, bulto, up, unidad);
							lista.Add(existeProd);
						}
					}
				}
				RPRDetalleDeProductosEnRP = lista ?? [];
			}
			else
			{
				RPRDetalleDeProductosEnRP = [];
			}
			datosIP = ObtenerGridCore(RPRDetalleDeProductosEnRP);
			return PartialView("_rprDetalleDeProductos", datosIP);
		}

		public JsonResult VerificarDetalleCargado(bool desdeDetalle = false)
		{
			try
			{
				if (RPRDetalleDeProductosEnRP == null)
				{
					return Json(new { error = false, warn = true, vacio = true, cantidad = 0, msg = "Error al intentar validar existencia de productos de RP cargados." });
				}
				else if (RPRDetalleDeProductosEnRP != null && RPRDetalleDeProductosEnRP.Count > 0 && ElementoEditado)
				{
					return Json(new { error = false, warn = false, vacio = false, cantidad = RPRDetalleDeProductosEnRP.Count, msg = $"Existen productos agregados al detalle de comprobante RP de proveedor. Desea guardar los cambios antes de salir? Caso contrario se perderán." });
				}
				else if (!desdeDetalle && JsonDeRP != null && JsonDeRP.encabezado != null && JsonDeRP.encabezado.Count > 0 && ElementoEditado)
				{
					return Json(new { error = false, warn = false, vacio = false, cantidad = JsonDeRP.encabezado.Count, msg = $"Existen productos agregados al detalle de comprobante RP de proveedor. Desea guardar los cambios antes de salir? Caso contrario se perderán." });
				}
				else
				{
					return Json(new { error = false, warn = false, vacio = true, cantidad = 0, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message, vacio = false, cantidad = 0 });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al verificar detalle de productos cargados para RP, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> GuardarDetalleDeComprobanteRP(bool guardado, bool generar, string listaProd, bool ponerEnCurso = false, string ulCantidad = "", string fechaTurno = "", string depoId = "", string nota = "")
		{
			try
			{
				if (guardado) //Guardo el detalle de productos del compte en la variable de sesion
				{
					if (JsonDeRP == null)
						JsonDeRP = new();

					//Nuevo RP -> lo agrego de pechardi, porque evalúo generar? Porque si es true no tengo que agregar nada, tengo que guardar
					if (!generar)
					{
						//Actualizo las ubicaciones de los productos
						//listaProd es un string que viene de la forma EJ: 081672@1#030329@2
						//IdDeProducto@Ubicacion
						string[] arr = [];
						Dictionary<string, string> keyValuePairs = [];
						if (!string.IsNullOrWhiteSpace(listaProd))
							arr = listaProd.Split('#');

						if (arr.Length > 0)
						{
							for (int i = 0; i < arr.Length; i++)
							{
								var item = arr[i].Split('@');
								if (item.Length == 2)
									keyValuePairs.Add(item[0], item[1]);
							}
						}
						var listaTemp = new JsonDeRPDto();
						listaTemp = JsonDeRP;

						JsonEncabezadoDeRPDto encabezado = new();
						encabezado = ObtenerObjectoParaAlmacenar();

						if (!listaTemp.encabezado.Exists(x => x.Rp == encabezado.Rp))
						{
							listaTemp.encabezado.Add(encabezado);
						}
						else //Existe RP, verifico si tiene cargado comprobantes y opero sobre ellos
						{
							var encabezadoTemp = listaTemp.encabezado.Where(x => x.Rp == encabezado.Rp && x.Tco_id == encabezado.Tco_id && x.Cm_compte == encabezado.Cm_compte).FirstOrDefault();
							//Por las dudas verifico que exista el encabezado pero que no tenaga detalle, le cargo el detalle
							if (encabezadoTemp != null)
							{
								if (encabezadoTemp.Comprobantes.Count == 0)
								{
									encabezadoTemp.Comprobantes = encabezado.Comprobantes;
								}
								//Tiene comprobantes, me fijo si ya existen items para ese tipo y numero de comprobante, si es así los actualizo
								else
								{
									encabezadoTemp.Comprobantes.RemoveAll(x => x.Tco_id == encabezado.Tco_id && x.Cm_compte == encabezado.Cm_compte);
									foreach (var x in encabezado.Comprobantes)
									{
										if (keyValuePairs.ContainsKey(x.P_id))
										{
											if (int.TryParse(keyValuePairs.GetValueOrDefault(x.P_id), out int order))
												x.Item = order;
										}
									}
									encabezadoTemp.Comprobantes.AddRange(encabezado.Comprobantes);
								}
								encabezadoTemp.Nota = encabezado.Nota;
								encabezadoTemp.Ul_cantidad = encabezado.Ul_cantidad;
								encabezadoTemp.Depo_id = encabezado.Depo_id;
								encabezadoTemp.Turno = encabezado.Turno;
								encabezadoTemp.Ope = encabezado.Ope;
							}
							else
							{
								listaTemp.encabezado.Add(encabezado);
							}
						}
						JsonDeRP = listaTemp;
					}
					if (generar)
					{
						var aux = JsonDeRP;
						foreach (var item in aux.encabezado)
						{
							item.Ul_cantidad = ulCantidad;
							item.Depo_id = depoId;
							item.Nota = nota;
							item.Turno = fechaTurno;
						}
						JsonDeRP = aux;
						var resultado = CargarNuevoComprobante();
						if (resultado != null && resultado.resultado == 0) //Genero correctamente el json, limpio variable de sesion de JSON y Detalle de productos
						{
							JsonDeRP = new();
							RPRDetalleDeProductosEnRP = [];
							RPRComprobanteDeRPSeleccionado = new();
							RPRAutorizacionSeleccionada = new();
							return Json(new { error = false, warn = false, codigo = 0, msg = "" });
						}
						return Json(new { error = false, warn = true, msg = resultado?.resultado_msj, codigo = resultado?.resultado });
					}
				}
				else
				{
					RPRDetalleDeProductosEnRP = [];
				}
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al guardar el detalle de comprobante RP, intente nuevamente mas tarde." });
			}
		}

		public IActionResult QuitarItemDeDetalleDeProd(string p_id)
		{
			try
			{
				var listaTemp = RPRDetalleDeProductosEnRP;
				if (!string.IsNullOrWhiteSpace(p_id))
				{
					var prodSelected = listaTemp.Where(x => x.P_id == p_id).FirstOrDefault();
					if (prodSelected != null)
					{
						ElementoEditado = true;
						listaTemp.Remove(prodSelected);
						RPRDetalleDeProductosEnRP = listaTemp;
					}
				}
				GridCore<ProductoBusquedaDto> datosIP;
				datosIP = ObtenerGridCore(RPRDetalleDeProductosEnRP);
				return PartialView("_rprDetalleDeProductos", datosIP);
			}
			catch (Exception ex)
			{

				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al quitar un producto del detalle, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> BuscarCuentaComercial(string cuenta, char tipo, string vista)
		{
			List<CuentaDto> Lista = new();
			try
			{
				if (string.IsNullOrEmpty(cuenta) || string.IsNullOrWhiteSpace(cuenta))
				{
					throw new NegocioException("Debe enviar codigo de cuenta");
				}
				if (CuentaComercialSeleccionada == null)
				{
					Lista = BuscarCuentaComercial(cuenta, tipo).Result;
				}
				else if (CuentaComercialSeleccionada.Cta_Id != cuenta)
				{
					Lista = BuscarCuentaComercial(cuenta, tipo).Result;
				}
				else
				{
					Lista.Add(CuentaComercialSeleccionada);
				}
				if (Lista.Count == 0)
				{
					throw new NegocioException("No se obtuvierion resultados");
				}
				if (Lista.Count == 1)
				{
					CuentaComercialSeleccionada = Lista[0];
					return Json(new { error = false, warn = false, unico = true, cuenta = Lista[0] });
				}
				return Json(new { error = false, warn = false, unico = false, cuenta = Lista });
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name} cuenta: {cuenta} tipo: {tipo}");
				return Json(new { error = true, msg = "Algo no fue bien al buscar la cuenta comercial, intente nuevamente mas tarde." });
			}
		}

		[HttpPost]
		public ActionResult ComboTiposComptes(string cuenta)
		{
			var adms = _tiposComprobantesServicio.BuscarTiposComptesPorCuenta(cuenta, TokenCookie).GetAwaiter().GetResult();
			TiposComprobantePorCuenta = adms;
			var lista = adms.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			var TiposComptes = HelperMvc<ComboGenDto>.ListaGenerica(lista);
			return PartialView("~/Areas/ControlComun/Views/CuentaComercial/_ctrComboTipoCompte.cshtml", TiposComptes);
		}

		[HttpPost]
		public ActionResult ActualizarComprobantesDeRP(string tipo, string nroComprobante)
		{
			GridCore<RPRComptesDeRPDto> datosIP;
			var lista = new List<RPRComptesDeRPDto>();
			var rp = "";
			if (RPRComprobanteDeRPSeleccionado != null && RPRComprobanteDeRPSeleccionado != default(RPRDetalleComprobanteDeRP))
				rp = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.Rp;
			if (tipo.IsNullOrEmpty())
			{
				datosIP = ObtenerGridCore(RPRComptesDeRPRegs);
			}
			else if (RPRComptesDeRPRegs.Exists(x => x.Tipo == tipo && x.NroComprobante == nroComprobante && x.Rp == rp))
			{
				lista = RPRComptesDeRPRegs.Where(x => x.Tipo != tipo && x.NroComprobante != nroComprobante && x.Rp != rp).ToList();
				RPRComptesDeRPRegs = lista.Where(x => x.Rp == "").ToList();
				datosIP = ObtenerGridCore(RPRComptesDeRPRegs);
			}
			else
			{
				datosIP = ObtenerGridCore(RPRComptesDeRPRegs);
			}
			return PartialView("_rprComprobantesDeRP", datosIP);
		}

		[HttpPost]
		public ActionResult CargarComprobantesDeRP(string tipo, string tipoDescripcion, string nroComprobante, string fecha, string importe, string rp)
		{
			GridCore<RPRComptesDeRPDto> datosIP;
			var lista = new List<RPRComptesDeRPDto>();
			var listaProd = new List<ProductoBusquedaDto>();
			JsonEncabezadoDeRPDto encaTemp;

			if (!string.IsNullOrWhiteSpace(rp) && !string.IsNullOrWhiteSpace(tipo) && !string.IsNullOrWhiteSpace(nroComprobante) && JsonDeRP == null)
			{
				PreCargarJson(rp, tipo, nroComprobante);
			}

			if (string.IsNullOrWhiteSpace(tipo) && string.IsNullOrWhiteSpace(tipoDescripcion) && string.IsNullOrWhiteSpace(nroComprobante) && string.IsNullOrWhiteSpace(fecha) && string.IsNullOrWhiteSpace(importe))
			{
				if (RPRComptesDeRPRegs != null)
					datosIP = ObtenerGridCore(RPRComptesDeRPRegs.Where(x => string.IsNullOrWhiteSpace(x.Rp)).ToList());
				else
					datosIP = ObtenerGridCore(new List<RPRComptesDeRPDto>());
				return PartialView("_rprComprobantesDeRP", datosIP);
			}
			else if (!string.IsNullOrWhiteSpace(rp) && RPRComptesDeRPRegs != null && RPRComptesDeRPRegs.Exists(x => x.Rp == rp))
			{
				if (!RPRComptesDeRPRegs.Exists(x => x.Rp == rp && x.Tipo == tipo && x.NroComprobante == nroComprobante))
				{
					RPRComptesDeRPRegs = CargarComprobanteRP(RPRComptesDeRPRegs, tipo, tipoDescripcion, nroComprobante, fecha, importe, rp);
					ElementoEditado = true;
				}
				datosIP = ObtenerGridCore(RPRComptesDeRPRegs.Where(x => x.Rp == rp).ToList());
			}
			else
			{
				if (JsonDeRP != null)
				{
					RPRComptesDeRPRegs = CargarComprobantesDeRPDesdeJson(JsonDeRP.encabezado);
				}
				if (RPRComptesDeRPRegs != null && !RPRComptesDeRPRegs.Exists(x => x.Rp == rp && x.Tipo == tipo && x.NroComprobante == nroComprobante))
				{
					RPRComptesDeRPRegs = CargarComprobanteRP(RPRComptesDeRPRegs, tipo, tipoDescripcion, nroComprobante, fecha, importe, rp);
					ElementoEditado = true;
				}
				else if (string.IsNullOrWhiteSpace(rp))
				{
					RPRComptesDeRPRegs = CargarComprobanteRP([], tipo, tipoDescripcion, nroComprobante, fecha, importe, rp);
				}
				datosIP = ObtenerGridCore(RPRComptesDeRPRegs);
			}
			return PartialView("_rprComprobantesDeRP", datosIP);
		}

		[HttpPost]
		public async Task<JsonResult> ConfirmarRPR(string rp)
		{
			var respuesta = new List<RespuestaDto>();
			try
			{
				respuesta = await _productoServicio.RPRConfirmarRPR(rp, AdministracionId, TokenCookie);
				var resultado = respuesta.First();
				if (resultado.resultado == 0)
				{
					return Json(new { error = false, warn = false, msg = "RPR Confirmado con éxito.", codigo = resultado.resultado });
				}
				else
				{
					return Json(new { error = false, warn = true, msg = resultado.resultado_msj, codigo = resultado.resultado });
				}
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.Message, codigo = 9999 });
			}

		}
		#endregion

		#region RECEPCION DE PROVEEDORES - Métodos privados
		private static int IsPositiveOrNegativeUsingBuiltInMethod<T>(T number) where T : ISignedNumber<T>
		{
			if (number == T.Zero)
				return 0;
			return T.IsPositive(number) ? 1 : -1;
		}

		private static string ObtenerColor(string no_recibido)
		{
			var leftPart = "";
			if (no_recibido.Length == 0)
				return "#ffffff";
			if (no_recibido == "0")
				return "#ffffff";
			if (no_recibido.Contains(','))
				leftPart = no_recibido.Split(',')[0];
			if (no_recibido.Contains('.'))
				leftPart = no_recibido.Split('.')[0];

			if (int.TryParse(leftPart, out int numeric_no_recibido))
			{
				return IsPositiveOrNegativeUsingBuiltInMethod(numeric_no_recibido) switch
				{
					0 => "#ffffff",
					1 => "#ff4500",
					-1 => "#008000",
					_ => "#ffffff",
				};
			}
			else
			{
				return "#ffffff";
			}
		}
		private JsonEncabezadoDeRPDto ObtenerObjectoParaAlmacenar()
		{
			try
			{
				var itemCount = 1;
				var ctaid = CuentaComercialSeleccionada.Cta_Id;
				JsonEncabezadoDeRPDto encabezado = new()
				{
					Ope = RPRAutorizacionSeleccionada == null ? TipoAltaRP.AGREGA.ToString() : TipoAltaRP.MODIFICA.ToString(),
					Rp = RPRAutorizacionSeleccionada == null ? "" : RPRAutorizacionSeleccionada.Rp,
					Cta_id = ctaid,
					Usu_id = UserName,
					Adm_id = AdministracionId,
					Rpe_id = "P",
					Rpe_desc = "Pendiente",
					Depo_id = RPRComprobanteDeRPSeleccionado.Depo_id,
					Nota = RPRComprobanteDeRPSeleccionado.Nota,
					Ul_cantidad = RPRComprobanteDeRPSeleccionado.Ul_cantidad,
					Turno = FormateoDeFecha(RPRComprobanteDeRPSeleccionado.FechaTurno, FechaTipoFormato.PARAJSON),
					Tco_id = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.Tipo,
					Cm_compte = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.NroComprobante,
					Comprobantes = RPRDetalleDeProductosEnRP.Select(x => new JsonComprobanteDeRPDto()
					{
						Tco_id = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.Tipo,
						Tco_desc = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.TipoDescripcion,
						Cm_compte = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.NroComprobante,
						Cm_fecha = FormateoDeFecha(RPRComprobanteDeRPSeleccionado.CompteSeleccionado.Fecha, FechaTipoFormato.PARAJSON),
						Cm_importe = RPRComprobanteDeRPSeleccionado.CompteSeleccionado.Importe,
						P_id = x.P_id,
						P_id_prov = x.P_id_prov,
						P_id_barrado = x.P_id_barrado,
						P_desc = x.P_desc,
						Bulto_up = x.P_unidad_pres,
						Bulto = x.Bulto.ToString(),
						Uni_suelta = x.Unidad.ToString(),
						Cantidad = x.Cantidad.ToString(),
						Oc_compte = x.oc_compte,
						Item = itemCount++,
						Producto = x
					}).ToList()
				};
				return encabezado;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name} intentando genera objeto para almacenar");
				return null;
			}
		}

		private async Task<List<CuentaDto>> BuscarCuentaComercial(string cuenta, char tipo)
		{
			List<CuentaDto> Lista = new();
			Lista = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
			if (Lista.Count > 0)
			{
				foreach (var item in Lista)
				{
					RegexOptions options = RegexOptions.None;
					Regex regex = new("[ ]{2,}", options);
					item.Cta_Denominacion = regex.Replace(item.Cta_Denominacion, " ");
				}
			}
			return Lista;
		}
		private List<RPRComptesDeRPDto> CargarComprobantesDeRPDesdeJson(List<JsonEncabezadoDeRPDto> encabezados)
		{
			var lista = new List<RPRComptesDeRPDto>();
			foreach (var item in encabezados)
			{
				var comprobante = item.Comprobantes.FirstOrDefault();
				if (comprobante != null)
				{
					lista.Add(new RPRComptesDeRPDto()
					{
						//Convert.ToDateTime(comprobante.Cm_fecha).ToString("dd/MM/yyyy")
						//Fecha = comprobante.Cm_fecha,
						Fecha = Convert.ToDateTime(comprobante.Cm_fecha).ToString("dd/MM/yyyy"),
						Importe = comprobante.Cm_importe,
						NroComprobante = comprobante.Cm_compte,
						Rp = item.Rp,
						Tipo = comprobante.Tco_id,
						TipoDescripcion = string.IsNullOrEmpty(comprobante.Tco_desc) ? TiposComprobantePorCuenta.Where(x => x.tco_id == comprobante.Tco_id).Select(y => y.tco_desc).First() : comprobante.Tco_desc
					});
				}
			}
			return lista;
		}
		private void PreCargarJson(string rp, string tipo, string nroComprobante)
		{
			JsonEncabezadoDeRPDto encaTemp;
			var listaProd = new List<ProductoBusquedaDto>();
			if (rp != "")
			{
				var objeto = ObtenerComprobantesDesdeJson(rp).Result;
				if (objeto != null)
				{
					JsonDeRP = objeto;
				}
			}
		}
		private decimal CalcularCantidadDeProductoParaAgregar(string up_id, string bulto, string up, string unidad)
		{
			decimal retValue = 0;

			if (up_id == "07")
			{
				return (Convert.ToDecimal(bulto) * Convert.ToDecimal(up)) + Convert.ToDecimal(unidad);
			}
			else
			{
				return Convert.ToDecimal(unidad);
			}
		}
		private string FormateoDeFecha(string formateoDeFecha, FechaTipoFormato tipo)
		{
			try
			{
				if (DateTime.TryParse(formateoDeFecha, out var date))
				{
					return tipo switch
					{
						FechaTipoFormato.PARAJSON => date.ToString("yyyy-MM-dd"),
						FechaTipoFormato.PARAUSUARIO => date.ToString("dd-MM-yyyy"),
						_ => date.ToString("dd-MM-yyyy"),
					};
				}
				else
				{
					return string.Empty;
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name} formateoDeFecha: {formateoDeFecha}");
				return string.Empty;
			}
		}
		private ProductoBusquedaDto ObtenerDatosDeProducto(string p_id)
		{
			ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
			BusquedaBase buscar = new()
			{
				Administracion = AdministracionId,
				Busqueda = p_id,
				DescuentoCli = 0,
				ListaPrecio = "",
				TipoOperacion = ""
			};

			return _productoServicio.BusquedaBaseProductos(buscar, TokenCookie).Result;
		}

		/// <summary>
		/// Metodo que recibe como parametro los id de productos separados por @
		/// </summary>
		/// <param name="p_idLista"></param>
		/// <returns>Lista de productos</returns>
		private List<ProductoBusquedaDto> ObtenerDatosDeProductos(string p_idLista)
		{
			ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
			BusquedaBase buscar = new()
			{
				Administracion = AdministracionId,
				Busqueda = p_idLista,
				DescuentoCli = 0,
				ListaPrecio = "",
				TipoOperacion = ""
			};

			return _productoServicio.BusquedaBaseProductosPorIds(buscar, TokenCookie).Result;
		}

		private RespuestaDto CargarNuevoComprobante()
		{
			try
			{
				List<RespuestaDto> Respuesta = [];
				JsonDeRPDto jsonAux = new();
				foreach (var item in JsonDeRP.encabezado)
				{

					var compAuxOrdered = item.Comprobantes.OrderBy(x => x.Item).ToList();
					item.Comprobantes = compAuxOrdered;
					jsonAux.encabezado.Add(item);
				}
				JsonDeRP = jsonAux;
				var json_string = GenerarJsonDesdeJsonEncabezadoDeRPLista();
				Respuesta = _productoServicio.RPRCargarCompte(json_string, TokenCookie).Result;
				return Respuesta?.FirstOrDefault();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return new RespuestaDto() { resultado = -1, resultado_msj = ex.Message };
			}

		}

		private string GenerarJsonDesdeJsonEncabezadoDeRPLista()
		{
			var jsonstring = JsonConvert.SerializeObject(JsonDeRP, new JsonSerializerSettings() { ContractResolver = new IgnorePropertiesResolver(new[] { "Producto" }) });
			return jsonstring;
		}

		public class IgnorePropertiesResolver : DefaultContractResolver
		{
			private readonly HashSet<string> ignoreProps;
			public IgnorePropertiesResolver(IEnumerable<string> propNamesToIgnore)
			{
				this.ignoreProps = new HashSet<string>(propNamesToIgnore);
			}

			protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
			{
				JsonProperty property = base.CreateProperty(member, memberSerialization);
				if (this.ignoreProps.Contains(property.PropertyName))
				{
					property.ShouldSerialize = _ => false;
				}
				return property;
			}
		}

		private void ObtenerComprobantesDesdeAutorizacionesPendientes()
		{
			var lista = new List<RPRComptesDeRPDto>();
			foreach (var item in RPRAutorizacionesPendientesEnRP)
			{
				var newCompte = new RPRComptesDeRPDto()
				{
					Fecha = item.Fecha?.ToString("dd/MM/yyyy"),
					Importe = item.Cm_importe.ToString(),
					NroComprobante = item.Cm_compte,
					Tipo = item.Tco_id,
					TipoDescripcion = item.Tco_desc,
					Rp = item.Rp,
				};
				lista.Add(newCompte);
			}
			RPRComptesDeRPRegs = lista;
		}

		private List<RPRComptesDeRPDto> CargarComprobanteRP(List<RPRComptesDeRPDto> listaEnSesion, string tipo, string tipoDescripcion, string nroComprobante, string fecha, string importe, string rp)
		{
			var lista = listaEnSesion;
			if (rp == null)
				rp = "";
			var nuevo = new RPRComptesDeRPDto()
			{
				Tipo = tipo,
				TipoDescripcion = tipoDescripcion,
				NroComprobante = nroComprobante,
				Fecha = Convert.ToDateTime(fecha).ToString("dd/MM/yyyy"),
				Importe = importe,
				Rp = rp
			};
			lista.Add(nuevo);
			return lista;
		}
		//private GridCore<T> ObtenerGridCore<T>(List<T> lista) where T : Dto
		//{
		//	var listaDetalle = new StaticPagedList<T>(lista, 1, 999, lista.Count);
		//	return new GridCore<T>() { ListaDatos = listaDetalle, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		//}
		private GridCore<RPROrdenDeCompraDetalleDto> ObtenerOCDetalleRPGrid(List<RPROrdenDeCompraDetalleDto> listaOCDetalle)
		{

			var lista = new StaticPagedList<RPROrdenDeCompraDetalleDto>(listaOCDetalle, 1, 999, listaOCDetalle.Count);

			return new GridCore<RPROrdenDeCompraDetalleDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}

		private GridCore<AutoComptesPendientesDto> ObtenerAutorizacionPendienteGrid(List<AutoComptesPendientesDto> pendientes)
		{

			var lista = new StaticPagedList<AutoComptesPendientesDto>(pendientes, 1, 999, pendientes.Count);

			return new GridCore<AutoComptesPendientesDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
		}


		private SelectList ComboDepositos()
		{
			var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		/// <summary>
		/// Agregar items a la lista de productos, en Carga Detalle de Comprobante RP PRoveedor
		/// </summary>
		/// <param name="lista">Lista de productos en sesión</param>
		/// <param name="item">Elemento a agregar/reemplazar/acumular</param>
		/// <param name="accion">Acción a realizar con el elemento, en relación al segundo parámetro</param>
		private void CargarItemEnGrillaDeProductos(List<ProductoBusquedaDto> lista, RPROrdenDeCompraDetalleDto item, int accion, ProductoBusquedaDto itemAQuitar, int itemNextValue)
		{
			if (accion == 1 && itemAQuitar != null)
			{
				lista.Remove(itemAQuitar);
			}
			else if (accion == 2 && itemAQuitar != null)
			{
				lista.Remove(itemAQuitar);
				item.oc_compte = itemAQuitar.oc_compte;
				item.ocd_bultos += itemAQuitar.Bulto;
				item.ocd_cantidad += itemAQuitar.Cantidad;
			}
			var producto = ObtenerDatosDeProducto(item.p_id);
			lista.Add(new ProductoBusquedaDto()
			{
				P_id = item.p_id,
				P_desc = item.p_desc,
				P_id_prov = item.p_id_prov,
				oc_compte = item.oc_compte,
				P_unidad_pres = item.ocd_unidad_pres.ToString(),
				Bulto = item.ocd_bultos,
				Unidad = item.ocd_unidad_x_bulto,
				Cantidad = (item.ocd_unidad_pres * item.ocd_bultos) + item.ocd_unidad_x_bulto,
				P_id_barrado = producto.P_id_barrado,
				Item = itemNextValue,
			});
		}

		private static DateTime UnixTimeStampToDateTime(string unixTimeStamp)
		{
			if (double.TryParse(unixTimeStamp, out var dt))
			{
				DateTime dateTime = new(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
				dateTime = dateTime.AddSeconds(dt).ToLocalTime();
				return dateTime;
			}
			else
				return default;
		}

		enum TipoAltaRP
		{
			AGREGA,
			MODIFICA,
			ELIMINA
		}

		enum FechaTipoFormato
		{
			PARAJSON,
			PARAUSUARIO
		}
		#endregion
	}
}