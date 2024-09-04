
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

//using System.Data;
using System.Reflection;
using X.PagedList;
using gc.sitio.Areas.Compras.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
				//resguardo lista de autorizaciones pendientes 
				//Veo que en  gc.pocket.site.Areas.PocketPpal.Controllers.RPRController.Index() almacena en una variable de sesión los datos.
				//Consultar si tengo que hacer lo mismo
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

		public async Task<IActionResult> NuevaAut(string rp)
		{
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			//var model = new RPRNuevaAutorizaciónDto() { ComboDeposito = ComboDepositos() };
			var model = new BuscarCuentaDto() { ComboDeposito = ComboDepositos() };
			//return PartialView("RPRNuevaAutorizacion", model);

			return PartialView("RPRNuevaAutorizacion", model);
		}

		public async Task<IActionResult> VerDetalleDeComprobanteDeRP(string idTipoCompte, string nroCompte)
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


		public async Task<IActionResult> CargarDetalleDeProductosEnRP(string oc_compte, string id_prod, string up, string bulto, string unidad, int accion)
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
			else
			{
				//Carga producto manual

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
				else if (RPRDetalleDeProductosEnRP.Count > 0)
				{
					return Json(new { error = false, warn = false, vacio = false, cantidad = RPRDetalleDeProductosEnRP.Count, msg = $"Existen productos ({RPRDetalleDeProductosEnRP.Count}) agregados al detalle de comprobante RP de proveedor. Desea guardar los cambios antes de salir? Caso contrario se perderán." });
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

		public async Task<JsonResult> GuardarDetalleDeComprobanteRP(bool guardado)
		{
			try
			{
				if (guardado) //Guardo el detalle de productos del compte en la variable de sesion
				{
					//TODO -> Generar lista de productos (encabezado/detalle) usando las clases: JsonEncabezadoDeRPDto/JsonComprobanteDeRPDto
				}
				else //Vacío la lista 
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
		public ActionResult CargarComprobantesDeRP(string tipo, string tipoDescripcion, string nroComprobante, string fecha, string importe)
		{
			GridCore<RPRComptesDeRPDto> datosIP;
			var lista = new List<RPRComptesDeRPDto>();
			if (tipo.IsNullOrEmpty())
			{
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			else if (RPRComptesDeRPRegs.Exists(x => x.Tipo == tipo && x.NroComprobante == nroComprobante))
			{
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			else
			{
				lista = RPRComptesDeRPRegs;
				var nuevo = new RPRComptesDeRPDto()
				{
					Tipo = tipo,
					TipoDescripcion = tipoDescripcion,
					NroComprobante = nroComprobante,
					Fecha = Convert.ToDateTime(fecha).ToString("dd/MM/yyyy"),
					Importe = importe
				};
				lista.Add(nuevo);
				RPRComptesDeRPRegs = lista;
				datosIP = ObtenerComprobantesDeRPGrid(RPRComptesDeRPRegs);
			}
			return PartialView("_rprComprobantesDeRP", datosIP);
		}

		#region Métodos privados
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
		private void CargarItemEnGrillaDeProductos(List<ProductoBusquedaDto> lista, RPROrdenDeCompraDetalleDto item, int accion, ProductoBusquedaDto? itemAQuitar)
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
			});
			#endregion
		}
	}
}