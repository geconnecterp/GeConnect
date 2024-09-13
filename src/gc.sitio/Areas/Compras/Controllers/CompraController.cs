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
//using System.Data;
using System.Reflection;
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

		public async Task<IActionResult> RPRAutorizacionesLista()
		{
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}
			List<RPRAutoComptesPendientesDto> pendientes;
			GridCore<RPRAutoComptesPendientesDto> grid;
			try
			{
				pendientes = await _productoServicio.RPRObtenerComptesPendiente(AdministracionId, TokenCookie);
				RPRAutorizacionesPendientesEnRP = pendientes;
				ObtenerComprobantesDesdeAutorizacionesPendientes();
				grid = ObtenerAutorizacionPendienteGrid(pendientes);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener Autorizaciones pendientes");
				TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones Pendientes. Si el problema persiste informe al Administrador";
				grid = new();
			}
			return View(grid);
		}

		public async Task<IActionResult> VerAut(string rp)
		{
			try
			{
				var lista = new List<RPRItemVerCompteDto>();
				var model = new RPRVerAutoDto();
				var comptes = RPRComptesDeRPRegs.Where(x => x.Rp == rp).ToList();
				model.Comprobantes = comptes;
				model.ComboDeposito = ComboDepositos();
				model.Rp = rp;
				JsonDeRPVerCompte = ObtenerComprobantesDesdeJson(rp).Result;
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
					RPRItemVerConteoLista = detalleVerConteos;
				}

				model.Depo_id = objeto?.Depo_id;
				model.Leyenda = $"Autorización RP {rp} Cuenta: {cuenta.FirstOrDefault().Cta_Denominacion} ({objeto.Cta_id}) Turno: {FormateoDeFecha(objeto.Turno, FechaTipoFormato.PARAUSUARIO)}";
				return PartialView("RPRVerAutorizacion", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar abrir pantalla Ver Autorización.");
				TempData["error"] = "Hubo algun problema al intentar abrir pantalla Ver Autorización. Si el problema persiste informe al Administrador";
				return null;
			}

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
					datosIP = ObtenerDetalleItemVerCompteGrid(RPRItemVerCompteLista.Where(x => x.Tco_id == tco_id && x.Cm_compte == cc_compte).ToList());
				}
				else
				{
					detalleVerCompte = await _productoServicio.RPRObtenerItemVerCompte(rp, TokenCookie);
					if (detalleVerCompte != null)
					{
						lista = detalleVerCompte.Where(x => x.Tco_id == tco_id && x.Cm_compte == cc_compte).ToList();
						datosIP = ObtenerDetalleItemVerCompteGrid(lista);
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
				datosIP = ObtenerDetalleItemVerCompteGrid(lista);
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
			datosIP = ObtenerDetalleItemVerConteosGrid(RPRItemVerConteoLista);
			return PartialView("_rprDetalleVerConteos", datosIP);
		}

		private async Task<JsonDeRPDto> ObtenerComprobantesDesdeJson(string rp)
		{
			List<JsonDto> json_string = [];

			try
			{
				JsonDeRPDto resObj = new JsonDeRPDto();
				var res = await _productoServicio.RPObtenerJsonDesdeRP(rp, TokenCookie);
				if (res != null && res.FirstOrDefault() != null)
					resObj = JsonConvert.DeserializeObject<JsonDeRPDto>(res?.FirstOrDefault()?.Json);
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
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}
			var rpSelected = RPRAutorizacionesPendientesEnRP.Where(x => x.Rp == rp).FirstOrDefault();
			if (rpSelected != default(RPRAutoComptesPendientesDto))
			{
				RPRAutorizacionSeleccionada = rpSelected;
			}
			var model = new BuscarCuentaDto() { ComboDeposito = ComboDepositos(), rp = rp };
			if (RPRComprobanteDeRPSeleccionado != null && string.IsNullOrEmpty(rp))
			{
				model.Cuenta = RPRComprobanteDeRPSeleccionado.cta_id;
				model.Nota = RPRComprobanteDeRPSeleccionado.Nota;
				model.FechaTurno = Convert.ToDateTime(RPRComprobanteDeRPSeleccionado.FechaTurno).ToString("yyyy-MM-dd");
				model.Depo_id = RPRComprobanteDeRPSeleccionado.Depo_id;
				//model.rp = rp;
			}
			else if (RPRAutorizacionSeleccionada != null)
			{
				model.Cuenta = RPRAutorizacionSeleccionada.Cta_id;
				model.Nota = RPRAutorizacionSeleccionada.Nota;
				model.FechaTurno = RPRAutorizacionSeleccionada.Fecha.ToString("yyyy-MM-dd");
				model.Depo_id = "0";
				//model.rp = rp;
				model.Compte = new RPRComptesDeRPDto()
				{
					Fecha = RPRAutorizacionSeleccionada.Fecha.ToString("yyyy-MM-dd"),
					Importe = RPRAutorizacionSeleccionada.Cm_importe.ToString(),
					NroComprobante = RPRAutorizacionSeleccionada.Cm_compte,
					Tipo = RPRAutorizacionSeleccionada.Tco_id,
					TipoDescripcion = RPRAutorizacionSeleccionada.Tco_desc
				};
			}
			return PartialView("RPRNuevaAutorizacion", model);
		}

		public async Task<IActionResult> VerDetalleDeComprobanteDeRP(string idTipoCompte, string nroCompte, string depoSelec, string notaAuto, string turno, string ponerEnCurso)
		{
			var compte = new RPRComptesDeRPDto();
			var model = new RPRDetalleComprobanteDeRP
			{
				Leyenda = $"Carga de Detalle de Comprobante RP Proveedor ({CuentaComercialSeleccionada.Cta_Id}) {CuentaComercialSeleccionada.Cta_Denominacion}"
			};

			if (RPRComptesDeRPRegs.Exists(x => x.Tipo == idTipoCompte && x.NroComprobante == nroCompte))
			{
				compte = RPRComptesDeRPRegs.Where(x => x.Tipo == idTipoCompte && x.NroComprobante == nroCompte).FirstOrDefault();
			}
			model.CompteSeleccionado = compte ?? new RPRComptesDeRPDto();
			model.cta_id = CuentaComercialSeleccionada.Cta_Id;
			model.ponerEnCurso = bool.Parse(ponerEnCurso);
			model.Nota = notaAuto;
			model.Depo_id = depoSelec;
			model.FechaTurno = UnixTimeStampToDateTime(turno).ToString("dd-MM-yyyy");
			RPRComprobanteDeRPSeleccionado = model;
			return PartialView("RPRCargaDetalleDeCompteRP", model);
		}

		public async Task<IActionResult> CargarOCxCuentaEnRP(string cta_id)
		{
			GridCore<RPROrdenDeCompraDto> datosIP;
			var lista = new List<RPROrdenDeCompraDto>();

			lista = await _cuentaServicio.ObtenerListaOCxCuenta(cta_id, TokenCookie);
			datosIP = ObtenerOCxCuentaRPGrid(lista);
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


		public async Task<IActionResult> CargarDetalleDeProductosEnRP(string oc_compte, string id_prod, string up, string bulto, string unidad, int accion, string tco_id, string cm_compte)
		{
			GridCore<ProductoBusquedaDto> datosIP;
			var lista = new List<ProductoBusquedaDto>();

			if (!string.IsNullOrWhiteSpace(oc_compte))
			{
				//Estoy buscando y cargando los productos de una OC seleccionada, siempre y cuando no hayan sido cargados los items de esa OC
				var detalleDeOC = await _cuentaServicio.ObtenerDetalleDeOC(oc_compte, TokenCookie);
				if (detalleDeOC != null && detalleDeOC.Count > 0 && !RPRDetalleDeProductosEnRP.Exists(x => x.oc_compte == oc_compte))
				{
					lista = RPRDetalleDeProductosEnRP;
					foreach (var item in detalleDeOC)
					{
						var itemTemp = lista.Where(x => x.P_id == item.p_id).FirstOrDefault();
						//No lo encuentra
						if (default(ProductoBusquedaDto) == itemTemp)
						{
							CargarItemEnGrillaDeProductos(lista, item, accion, null);
						}
						else
						{
							CargarItemEnGrillaDeProductos(lista, item, accion, itemTemp);
						}
					}
					RPRDetalleDeProductosEnRP = lista;
				}
			}
			//Traigo los prod del almacenamiento temporal
			else if (!string.IsNullOrWhiteSpace(tco_id) && !string.IsNullOrWhiteSpace(cm_compte) && JsonDeRP != null && JsonDeRP.encabezado != null && JsonDeRP.encabezado.Count > 0)
			{
				var encTemp = JsonDeRP.encabezado.Where(x => x.Tco_id == tco_id && x.Cm_compte == cm_compte).FirstOrDefault();
				if (encTemp != null)
				{
					var listaComprobantesTemp = encTemp.Comprobantes.Where(x => x.Tco_id == tco_id && x.Cm_compte == cm_compte).ToList();
					foreach (var item in listaComprobantesTemp)
					{
						lista.Add(item.Producto);
					}
				}
				RPRDetalleDeProductosEnRP = lista ?? [];
			}
			else
			{
				RPRDetalleDeProductosEnRP = [];
			}
			datosIP = ObtenerDetalleDeProductosRPGrid(RPRDetalleDeProductosEnRP);
			return PartialView("_rprDetalleDeProductos", datosIP);
		}

		public JsonResult VerificarDetalleCargado()
		{
			try
			{
				if (RPRDetalleDeProductosEnRP == null)
				{
					return Json(new { error = false, warn = true, vacio = true, cantidad = 0, msg = "Error al intentar validar existencia de productos de RP cargados." });
				}
				else if (RPRDetalleDeProductosEnRP != null && RPRDetalleDeProductosEnRP.Count > 0)
				{
					return Json(new { error = false, warn = false, vacio = false, cantidad = RPRDetalleDeProductosEnRP.Count, msg = $"Existen productos agregados al detalle de comprobante RP de proveedor. Desea guardar los cambios antes de salir? Caso contrario se perderán." });
				}
				else if (JsonDeRP != null && JsonDeRP.encabezado != null && JsonDeRP.encabezado.Count > 0)
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

		public async Task<JsonResult> GuardarDetalleDeComprobanteRP(bool guardado, bool generar)
		{
			try
			{
				if (guardado) //Guardo el detalle de productos del compte en la variable de sesion
				{
					if (JsonDeRP == null)
						JsonDeRP = new();

					//TODO : Seguir con el cambio de de agregado de encabezado JsonDeRP
					var listaTemp = new JsonDeRPDto();
					listaTemp = JsonDeRP;

					JsonEncabezadoDeRPDto encabezado = new();
					encabezado = ObtenerObjectoParaAlmacenar();

					//Nuevo RP -> lo agrego de pechardi, porque evalúo generar? Porque si es true no tengo que agregar nada, tengo que guardar
					if (!generar)
					{
						if (!listaTemp.encabezado.Exists(x => x.Rp == encabezado.Rp))
						{
							listaTemp.encabezado.Add(encabezado);
						}
						else //Existe RP, verifico si tiene cargado comprobantes y opero sobre ellos
						{
							var encabezadoTemp = listaTemp.encabezado.Where(x => x.Rp == encabezado.Rp).FirstOrDefault();
							//Por las dudas verifico que exista el encabezado pero que no tenaga detalle, le cargo el detalle
							if (encabezadoTemp != null)
							{
								if (encabezadoTemp.Comprobantes.Count == 0)
								{
									encabezadoTemp.Comprobantes = encabezado.Comprobantes;
								}
								//Tiene comprobantes, me fijo si ya existen items para ese tipo y numero de comprobante, si es así los actualizo
								else if (encabezadoTemp.Comprobantes.Exists(x => x.Tco_id == encabezado.Tco_id && x.Cm_compte == encabezado.Cm_compte))
								{
									encabezadoTemp.Comprobantes.RemoveAll(x => x.Tco_id == encabezado.Tco_id && x.Cm_compte == encabezado.Cm_compte);
									encabezadoTemp.Comprobantes.AddRange(encabezado.Comprobantes);
								}
								//No existen items para ese tipo y numero de comprobante, los agrego
								else
								{
									//encabezadoTemp.Comprobantes.AddRange(encabezado.Comprobantes);
									listaTemp.encabezado.Add(encabezado);
								}
								//listaTemp.RemoveAll(x => x.Rp == encabezado.Rp);
								//listaTemp.Add(encabezadoTemp);
							}
						}
						JsonDeRP = listaTemp;
					}
					if (generar)
					{
						var resultado = CargarNuevoComprobante();
						if (resultado != null && resultado.resultado == "0") //Genero correctamente el json, limpio variable de sesion de JSON y Detalle de productos
						{
							JsonDeRP = new();
							RPRDetalleDeProductosEnRP = [];
							return Json(new { error = false, warn = false, codigo = 0, msg = "" });
						}
						return Json(new { error = false, warn = true, msg = resultado?.resultado_msj, codigo = resultado?.resultado });
					}
				}
				//RPRDetalleDeProductosEnRP = [];
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
						listaTemp.Remove(prodSelected);
						RPRDetalleDeProductosEnRP = listaTemp;
					}
				}
				GridCore<ProductoBusquedaDto> datosIP;
				datosIP = ObtenerDetalleDeProductosRPGrid(RPRDetalleDeProductosEnRP);
				return PartialView("_rprDetalleDeProductos", datosIP);
			}
			catch (Exception ex)
			{

				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al quitar un producto del detalle, intente nuevamente mas tarde." });
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
					Lista = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
				}
				else if (CuentaComercialSeleccionada.Cta_Id != cuenta)
				{
					Lista = await _cuentaServicio.ObtenerListaCuentaComercial(cuenta, tipo, TokenCookie);
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
			var lista = adms.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			var TiposComptes = HelperMvc<ComboGenDto>.ListaGenerica(lista);
			return PartialView("~/Areas/ControlComun/Views/CuentaComercial/_ctrComboTipoCompte.cshtml", TiposComptes);
		}

		[HttpPost]
		public ActionResult ActualizarComprobantesDeRP(string tipo, string nroComprobante)
		{
			GridCore<RPRComptesDeRPDto> datosIP;
			var lista = new List<RPRComptesDeRPDto>();
			if (tipo.IsNullOrEmpty())
			{
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			else if (RPRComptesDeRPRegs.Exists(x => x.Tipo == tipo && x.NroComprobante == nroComprobante))
			{
				lista = RPRComptesDeRPRegs.Where(x => x.Tipo != tipo && x.NroComprobante != nroComprobante).ToList();
				RPRComptesDeRPRegs = lista;
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			else
			{
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			return PartialView("_rprComprobantesDeRP", datosIP);
		}

		[HttpPost]
		public ActionResult CargarComprobantesDeRP(string tipo, string tipoDescripcion, string nroComprobante, string fecha, string importe, string rp)
		{
			GridCore<RPRComptesDeRPDto> datosIP;
			var lista = new List<RPRComptesDeRPDto>();

			if (rp != "")
			{
				var objeto = ObtenerComprobantesDesdeJson(rp);
			}

			if (string.IsNullOrWhiteSpace(tipo) && string.IsNullOrWhiteSpace(tipoDescripcion) && string.IsNullOrWhiteSpace(nroComprobante) && string.IsNullOrWhiteSpace(fecha) && string.IsNullOrWhiteSpace(importe))
			{
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs.Where(x => string.IsNullOrWhiteSpace(x.Rp)).ToList());
				return PartialView("_rprComprobantesDeRP", datosIP);
			}
			else if (!string.IsNullOrWhiteSpace(rp) && RPRComptesDeRPRegs.Exists(x => x.Rp == rp))
			{
				if (!RPRComptesDeRPRegs.Exists(x => x.Rp == rp && x.Tipo == tipo && x.NroComprobante == nroComprobante))
				{
					RPRComptesDeRPRegs = CargarComprobanteRP(RPRComptesDeRPRegs, tipo, tipoDescripcion, nroComprobante, fecha, importe, rp);
				}
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs.Where(x => x.Rp == rp).ToList());
			}
			else
			{
				RPRComptesDeRPRegs = CargarComprobanteRP(RPRComptesDeRPRegs, tipo, tipoDescripcion, nroComprobante, fecha, importe, rp);
				if (string.IsNullOrWhiteSpace(rp))
					datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs.Where(x => string.IsNullOrWhiteSpace(x.Rp)).ToList());
				else
					datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs.Where(x => x.Rp == rp).ToList());
			}
			return PartialView("_rprComprobantesDeRP", datosIP);
		}

		#region Métodos privados
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

		private RespuestaDto CargarNuevoComprobante()
		{
			try
			{
				List<RespuestaDto> Respuesta = [];
				var json_string = GenerarJsonDesdeJsonEncabezadoDeRPLista();
				Respuesta = _productoServicio.RPRCargarCompte(json_string, TokenCookie).Result;
				return Respuesta?.FirstOrDefault();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return new RespuestaDto() { resultado = "9999", resultado_msj = ex.Message };
			}

		}

		private string GenerarJsonDesdeJsonEncabezadoDeRPLista()
		{
			var jsonstring = JsonConvert.SerializeObject(JsonDeRP, new JsonSerializerSettings() { ContractResolver = new IgnorePropertiesResolver(new[] { "Producto" }) });
			//var jsonstring = JsonConvert.SerializeObject(JsonEncabezadoDeRPLista, Formatting.Indented);
			return jsonstring;
		}

		//TODO: agregar spinner de espera cuando estoy agregando productos (demora por la busqueda de los datos de los productos)

		//short helper class to ignore some properties from serialization
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
					Fecha = item.Fecha.ToString("dd/MM/yyyy"),
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
		private GridCore<RPRVerConteoDto> ObtenerDetalleItemVerConteosGrid(List<RPRVerConteoDto> lista)
		{

			var listaDetalle = new StaticPagedList<RPRVerConteoDto>(lista, 1, 999, lista.Count);

			return new GridCore<RPRVerConteoDto>() { ListaDatos = listaDetalle, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		private GridCore<RPRItemVerCompteDto> ObtenerDetalleItemVerCompteGrid(List<RPRItemVerCompteDto> lista)
		{

			var listaDetalle = new StaticPagedList<RPRItemVerCompteDto>(lista, 1, 999, lista.Count);

			return new GridCore<RPRItemVerCompteDto>() { ListaDatos = listaDetalle, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		private GridCore<RPROrdenDeCompraDetalleDto> ObtenerOCDetalleRPGrid(List<RPROrdenDeCompraDetalleDto> listaOCDetalle)
		{

			var lista = new StaticPagedList<RPROrdenDeCompraDetalleDto>(listaOCDetalle, 1, 999, listaOCDetalle.Count);

			return new GridCore<RPROrdenDeCompraDetalleDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		private GridCore<RPROrdenDeCompraDto> ObtenerOCxCuentaRPGrid(List<RPROrdenDeCompraDto> listaOCxCuentaRP)
		{

			var lista = new StaticPagedList<RPROrdenDeCompraDto>(listaOCxCuentaRP, 1, 999, listaOCxCuentaRP.Count);

			return new GridCore<RPROrdenDeCompraDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		private GridCore<ProductoBusquedaDto> ObtenerDetalleDeProductosRPGrid(List<ProductoBusquedaDto> listaProdDeRP)
		{

			var lista = new StaticPagedList<ProductoBusquedaDto>(listaProdDeRP, 1, 999, listaProdDeRP.Count);

			return new GridCore<ProductoBusquedaDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}
		private GridCore<RPRComptesDeRPDto> ObtenerComprobantesDeRPGrid(List<RPRComptesDeRPDto> listaComptesDeRP)
		{

			var lista = new StaticPagedList<RPRComptesDeRPDto>(listaComptesDeRP, 1, 999, listaComptesDeRP.Count);

			return new GridCore<RPRComptesDeRPDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Item", SortDir = "ASC" };
		}

		private GridCore<RPRAutoComptesPendientesDto> ObtenerAutorizacionPendienteGrid(List<RPRAutoComptesPendientesDto> pendientes)
		{

			var lista = new StaticPagedList<RPRAutoComptesPendientesDto>(pendientes, 1, 999, pendientes.Count);

			return new GridCore<RPRAutoComptesPendientesDto>() { ListaDatos = lista, CantidadReg = 999, PaginaActual = 1, CantidadPaginas = 1, Sort = "Cta_denominacion", SortDir = "ASC" };
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
		private void CargarItemEnGrillaDeProductos(List<ProductoBusquedaDto> lista, RPROrdenDeCompraDetalleDto item, int accion, ProductoBusquedaDto itemAQuitar)
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
				Cantidad = item.ocd_cantidad,
				P_id_barrado = producto.P_id_barrado,
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