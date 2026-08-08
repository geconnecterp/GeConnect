using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Response;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class NDeCYPIController : NDeCYPIControladorBase
	{
		private const string PedidoInterno = "PI";
		private const string NecesidadesCompra = "NC";
		private readonly AppSettings _appSettings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IOfertaServicio _ofertaServicio;
		private readonly IDepositoServicio _depositoServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo;
		private string APP_MODULO = AppModulos.PEDIDO_INTERNO.ToString();
		private readonly IDocManagerServicio _docMSv;
		public NDeCYPIController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio,
								 IAdministracionServicio administracionServicio, ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context,
								 IOfertaServicio ofertaServicio, IHttpContextAccessor accessor, IDepositoServicio depositoServicio,
								 IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, accessor, logger)
		{
			_appSettings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_productoServicio = productoServicio;
			_administracionServicio = administracionServicio;
			_ofertaServicio = ofertaServicio;
			_depositoServicio = depositoServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult NecesidadesDeCompraBack()
		{
			NDeCYPI.NecesidadesDeCompraDto model = new();
			List<ProveedorFamiliaListaDto> proveedoresFamilias = [];
			try
			{
				model.ComboProveedores = ComboProveedores();
				model.ComboProveedoresFamilia = HelperMvc<ComboGenDto>.ListaGenerica(proveedoresFamilias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc }));
				model.ComboRubros = ComboRubros();
				model.Productos = ObtenerGridCoreSmart<ProductoNCPIDto>([]);
				model.ComboSucursales = ComboSucursales();
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al inicializar vista Necesidades de Compra");
				TempData["error"] = "Hubo algun problema al inicializar vista Necesidades de Compra. Si el problema persiste informe al Administrador";
				model = new();
			}
			return View(model);
		}

		public IActionResult NecesidadesDeCompra()
		{
			NDeCYPI.NecesidadesDeCompraDto model = new();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				ViewData["Titulo"] = "NECESIDADES DE STOCK DE COMPRA";
				model.ComboSucursales = ComboSucursales();
				CargarDatosIniciales(true);
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

		public IActionResult PedidosInternos()
		{
			NDeCYPI.PedidosInternosDto model = new();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				var titulo = "NECESIDADES DE PEDIDOS INTERNOS";
				ViewData["Titulo"] = titulo;
				model.ComboSucursales = ComboSucursales();
				model.ListaSucursales = ComboSucursales(AdministracionId);
				CargarDatosIniciales(true);

				#region Gestor Impresion - Inicializacion de variables
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);
				#endregion

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

		public IActionResult PedidosInternosBack()
		{
			NDeCYPI.PedidosInternosDto model = new();
			List<ProveedorFamiliaListaDto> proveedoresFamilias = [];
			try
			{
				model.ComboProveedores = ComboProveedores();
				model.ComboProveedoresFamilia = HelperMvc<ComboGenDto>.ListaGenerica(proveedoresFamilias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc }));
				model.ComboRubros = ComboRubros();
				model.Productos = ObtenerGridCoreSmart<ProductoNCPIDto>([]);
				ViewData["Titulo"] = "NECESIDADES DE PEDIDOS INTERNOS";
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al obtener intentar obtener la vista de Pedidos Internos");
				TempData["error"] = "Hubo algun problema al intentar obtener la vista de Pedidos Internos. Si el problema persiste informe al Administrador";
				model = new();
			}
			return View(model);
		}

		public IActionResult BuscarProductosOCPI(string filtro, string id, string tipo, string sort = "p_m_desc", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new GridCoreSmart<ProductoNCPIDto>();
			MetadataGrid metadata;
			GridCoreSmart<ProductoNCPIDto> grillaDatos;
			try
			{
				var Sort = sort;
				var SortDir = sortDir;
				var Registros = _appSettings.NroRegistrosPagina;
				var Pagina = pag;
				var productos = _productoServicio.NCPICargarListaDeProductosPag(tipo, AdministracionId, filtro, id, TokenCookie, Sort, SortDir, Registros, Pagina).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				//grillaDatos = GenerarGrillaSmart(ListaDeUsuarios, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				grillaDatos = GenerarGrillaSmart(productos.Item1, sort, _appSettings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, sortDir);
				ListaProductoNCPI = productos.Item1;
				return PartialView("_grillaProductos", grillaDatos);
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

		public IActionResult BuscarProductosOCPI2(NCPICargarListaDeProductos2Request request)
		{
			//var model = new GridCoreSmart<ProductoNCPIDto>();
			MetadataGrid metadata;
			GridCoreSmart<ProductoNCPIDto> grillaDatos;
			var model = new BuscarProductosOCPI2Model();
			try
			{
				request.Registros = _appSettings.NroRegistrosPagina;
				request.Adm_id = AdministracionId;
				request.Usu_id = UserName;
				var productos = _productoServicio.NCPICargarListaDeProductosPag2(request, TokenCookie).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				//grillaDatos = GenerarGrillaSmart(ListaDeUsuarios, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				grillaDatos = GenerarGrillaSmart(productos.Item1, request.Sort ?? "p_desc", _appSettings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir ?? "ASC");
				productos.Item1.Where(x => x.p_orden_pg == null).ToList().ForEach(x => x.p_orden_pg = 0);
				ListaProductoNCPI = productos.Item1;
				model.ListaDatosProductos = grillaDatos;
				model.Tipo = request.Tipo;
				return PartialView("_grillaProductos", model);
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

		#region Buscar InfoProd

		public async Task<IActionResult> BuscarInfoProdIExMeses(string pId, string admId, int meses)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProdIExMesDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProdIExMes(admId, pId, meses, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExMesDto>(info);
				return PartialView("_infoProdIExMeses", model);
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

		public async Task<IActionResult> BuscarInfoProdIExSemanas(string pId, string admId, int semanas)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProdIExSemanaDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProdIExSemana(admId, pId, semanas, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExSemanaDto>(info);
				return PartialView("_infoProdIExSemanas", model);
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

		public async Task<IActionResult> BuscarInfoProdStkDeposito(string pId, string admId)
		{
			var model = new GridCoreSmart<InfoProdStkD>();
			try
			{
				var info = await _productoServicio.InfoProductoStkD(pId, AdministracionId, TokenCookie);
				model = ObtenerGridCoreSmart<InfoProdStkD>(info);
				return PartialView("_infoProdPorDeposito", model);
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

		public async Task<IActionResult> BuscarInfoProdStkSucursal(string pId, string admId)
		{
			var model = new GridCoreSmart<InfoProdStkA>();
			try
			{
				var info = await _productoServicio.InfoProductoStkA(pId, AdministracionId, TokenCookie);
				model = ObtenerGridCoreSmart<InfoProdStkA>(info);
				return PartialView("_infoProdPorSucursal", model);
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

		public async Task<IActionResult> BuscarInfoProdSustituto(string pId, string tipo, bool soloProv)
		{
			var model = new GridCoreSmart<ProductoNCPISustitutoDto>();
			try
			{
				var info = await _productoServicio.InfoProdSustituto(pId, tipo, AdministracionId, soloProv, TokenCookie);
				model = ObtenerGridCoreSmart<ProductoNCPISustitutoDto>(info);
				return PartialView("_infoProdSustituto", model);
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

		public async Task<IActionResult> BuscarInfoProd(string pId)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProductoDto>();
			try
			{
				var info = await _productoServicio.InfoProd(pId, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProductoDto>(info);
				return PartialView("_infoProducto", model);
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

		#endregion

		public JsonResult CargaPedidoOCPI(string tipo, string pId, string tipoCarga, int bultos)
		{
			try
			{
				var request = new NCPICargaPedidoRequest() { adm_id = AdministracionId, usu_id = UserName, tipo = tipo, pId = pId, tipoCarga = tipoCarga, bultos = bultos };
				var response = CargaPedidoOCPI(request);

				if (response == null)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.Count == 0)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.First().resultado != 0)
					return Json(new { error = false, warn = true, msg = response.First().resultado_msj });

				var item = response.First();

				var listaTemp = ListaProductoNCPI;
				var prod = listaTemp.Where(x => x.p_id == pId).First();
				prod.cantidad = item.cantidad;
				prod.costo = item.p_pcosto;
				prod.costo_total = item.p_pcosto * item.cantidad;
				prod.pedido = bultos;
				prod.paletizado = item.pallet;
				prod.pedido_tipo = "M"; //Manual
				ListaProductoNCPI = listaTemp;

				return Json(new
				{
					error = false,
					warn = false,
					msg = string.Empty,
					unidadPres = item.unidad_pres,
					pCosto = item.p_pcosto,
					bulto = item.bultos,
					cantidad = item.cantidad,
					pallet = item.pallet,
					pCostoTotal = item.p_pcosto * item.cantidad,
					pedidoTipo = "M"
				});
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al intentar setear el estado del remito.");
				TempData["error"] = "Hubo algun problema al intentar setear el estado del remito. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, msg = "Error al intentar setear el estado del remito." });
			}
		}

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaProductoNCPI = [];
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public IActionResult ObtenerProveedoresFamilia(string ctaId)
		{
			var model = new NDeCYPI.ProveedoresFamiliaDto();
			try
			{
				model.ComboProveedoresFamilia = ComboProveedoresFamilia(ctaId, _cuentaServicio);
				return PartialView("_listaProveedoresFamilia", model);
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

		public IActionResult ObtenerRubros()
		{
			var model = new ListaRubroModel();
			try
			{
				model.ListaRubros = ComboRubros();
				return PartialView("_listaRubros", model);
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

		//Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
		[HttpPost]
		public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
		{
			try
			{
				CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public JsonResult BuscarFlias(string prefix)
		{
			//var nombres = await _provSv.BuscarAsync(new QueryFilters { Search = prefix }, TokenCookie);
			//var lista = nombres.Item1.Select(c => new EmpleadoVM { Nombre = c.NombreCompleto, Id = c.Id, Cuil = c.CUIT });
			var rub = ProveedorFamiliaLista.Where(x => x.pg_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var rubros = rub.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_lista });
			return Json(rubros);
		}

		public IActionResult RecargarGrilla(string tipo)
		{
			var model = new BuscarProductosOCPI2Model();
			try
			{
				model.Tipo = tipo;
				model.ListaDatosProductos = ObtenerGridCoreSmart<ProductoNCPIDto>(ListaProductoNCPI);
				return PartialView("_grillaProductos", model);
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

		#region Modal Pedido Auto
		[HttpPost]
		public IActionResult AbrirModalAuto(string abrirComo)
		{
			var model = new FiltroCompraAutoModel();
			try
			{
				if (abrirComo == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "No se han especificado datos obligatiorios."
					};
					return PartialView("_gridMensaje", response);
				}
				if (abrirComo == NecesidadesCompra)
				{
					model.EsOC = true;
					model.Titulo = "Determinación Automática de Compra";
					model.MostrarExcluirOCPendientes = true;
				}
				else
				{
					model.EsOC = false;
					model.Titulo = "Determinación Automática de Compra";
					model.MostrarExcluirOCPendientes = false;
				}
				model.DiasAprov = 30;
				model.VentaDiariaDesde = DateTime.Now.AddDays(-30);
				model.VentaDiariaHasta = DateTime.Now;
				model.LimitarPedidoACompletar = false;
				model.LimitarPedidoParaCumplir = false;
				model.TomarUltimoPedido = false;

				var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
				if (depositos != null && depositos.Count > 0)
				{
					ListaDepositos = depositos;
					model.ListaDepositos = ObtenerListaDepositos(depositos);
				}
				else
				{
					ListaDepositos = [];
					model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
				}

				var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
				if (sucursales != null && sucursales.Count > 0)
				{
					ListaSucursales = sucursales;
					model.ListaSucursales = ObtenerLista(sucursales);
				}
				else
				{
					ListaSucursales = [];
					model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
				}

				var SucursalesList = new List<ComboGenDto>();
				ViewBag.SucursalesListModal = HelperMvc<ComboGenDto>.ListaGenerica(SucursalesList);
				var DepositosList = new List<ComboGenDto>();
				ViewBag.DepositosListModal = HelperMvc<ComboGenDto>.ListaGenerica(DepositosList);
				return PartialView("_modalFiltrosCompraAuto", model);
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

		public JsonResult ValidarPertenenciaDeDepositoEnSucursal(string depoId, List<string> sucuId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(depoId))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un depósito.", puedoAgregar = false });
				if (sucuId == null || sucuId.Count <= 0)
					return Json(new { error = false, warn = false, msg = "", puedoAgregar = true });
				var deposito = ListaDepositos.Where(x => x.Depo_Id == depoId).FirstOrDefault();
				if (deposito == null)
					return Json(new { error = true, warn = false, msg = "El depósito seleccionado no es válido.", puedoAgregar = false });
				if (sucuId.Contains(deposito.Adm_Id))
					return Json(new { error = false, warn = false, msg = "", puedoAgregar = true });
				else
					return Json(new { error = true, warn = false, msg = "El deposito seleccionado no pertence a la sucursal que ha incluido.", puedoAgregar = false });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = "Se ha producido un error al intentar validar el depósito seleccionado." });
			}
		}

		public JsonResult ConfirmarCambiosPedidoAuto(NCPIConfirmarCambiosPedidoAutoRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				if (ListaProductoNCPI == null || ListaProductoNCPI.Count == 0)
					return Json(new { error = true, warn = false, msg = $"No hay productos." });

				var listaProdPedAuto = new List<ProductoNCPI_AutoDto_>();
				ListaProductoNCPI.ForEach(x => listaProdPedAuto.Add(new ProductoNCPI_AutoDto_() { p_id = x.p_id }));

				request.json_p = JsonConvert.SerializeObject(listaProdPedAuto, new JsonSerializerSettings());
				
				if (request.tipo == PedidoInterno)
					request.adm_list.Add(AdministracionId);
				
				PrintProperties(request);
				var respuesta = _productoServicio.NecesidadesStockAuto(request, TokenCookie);

				if (respuesta != null && respuesta.Count > 0)
				{
					var listaTemp = ListaProductoNCPI;
					foreach (var item in respuesta)
					{
						ActualizarProductos(item, listaTemp, request.tipo, "A");
					}
					ListaProductoNCPI = listaTemp;
					return Json(new { error = false, warn = false, msg = "" });
				}
				else
					return Json(new { error = true, warn = false, msg = "Se ha producido un error interno al realizar la determinación automática de compra." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener precios auto." });
			}
		}
		#endregion

		#region Pedidos Internos
		public IActionResult AbrirPantallaPasarAPI()
		{
			var model = new PasarAPIModel();
			try
			{
				var productos = _productoServicio.PIPendienteDetalle(AdministracionId, UserName, TokenCookie).Result;
				productos.ToList().ForEach(x => x.selected = true);
				model.ListaSucursales = ComboSucursales(AdministracionId);
				model.ListaProductos = ObtenerGridCoreSmart<PedidoInternoPendienteDetalleDto>(productos);
				return PartialView("_pasar_a_PI", model);
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
		public async Task<JsonResult> ConfirmarPedidoInterno(ConfirmarPedidoInternoRequest request)
		{
			try
			{
				// Verificar autenticación - consistente con otros métodos
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { ok = false, mensaje = "No autorizado" });

				if (request == null)
					return Json(new { ok = false, mensaje = "Los datos de confirmación no fueron recepcionados. Verifique." });
				if (string.IsNullOrEmpty(request.adm_id_entrega))
					return Json(new { ok = false, mensaje = "Debe especificar una sucursal válida. Verifique." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;

				if (string.IsNullOrEmpty(request.json))
					return Json(new { ok = false, mensaje = "Al menos un producto es necesario." });

				// Llamada al servicio
				var respuesta = await _productoServicio.ConfirmarPedidoInterno(request, TokenCookie);

				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Se ha genetado el pedido interno exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en la confirmación del pedido: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						msg = respuesta.Mensaje ?? "Error al procesar el pedido"
					});
				}
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, "Error inesperado al confirmar pedido");
				return Json(new
				{
					ok = false,
					error = true,
					msg = "Error interno al procesar la solicitud"
				});
			}
		}
		#endregion

		#region Métodos privados
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		/// <summary>
		/// Actualizar el producto en la lista temporal con los datos obtenidos del pedido automático o manual
		/// </summary>
		/// <param name="prod">Producto a actualizar</param>
		/// <param name="listaT">Lista de productos obtenidos en base a los filtros de busquedas, reservados en sesión</param>
		/// <param name="pedidoTipo">Tipo de actualización: M -> Manual; A -> Automático</param>
		private void ActualizarProductos(ProductoNCPI_AutoDto prod, List<ProductoNCPIDto> listaT, string tipo, string pedidoTipo = "M")
		{
			if (prod == null)
				return;
			var req = new NCPICargaPedidoRequest() { adm_id = AdministracionId, usu_id = UserName, tipo = tipo, pId = prod.p_id, tipoCarga = "A", bultos = prod.auto_bulto };
			var respuesta = CargaPedidoOCPI(req);
			if (respuesta == null)
				return;
			if (respuesta.Count == 0)
				return;
			if (respuesta.First().resultado != 0)
				return;
			var item = respuesta.First();
			var ItemEnListaT = listaT.Where(x => x.p_id == prod.p_id).First();
			ItemEnListaT.cantidad = item.cantidad;
			ItemEnListaT.costo = item.p_pcosto;
			ItemEnListaT.costo_total = item.p_pcosto * item.cantidad;
			ItemEnListaT.pedido = prod.auto_bulto;
			ItemEnListaT.paletizado = item.pallet;
			ItemEnListaT.pedido_tipo = pedidoTipo;
		}

		private static void ObtenerColor(ref List<ProductoNCPIDto> listaProd)
		{
			foreach (var item in listaProd)
			{
				if (item.p_activo == "D") //Discontinuo
					item.Row_color = "#fc4641";
			}
		}

		private List<NCPICargaPedidoResponse> CargaPedidoOCPI(NCPICargaPedidoRequest request)
		{
			try
			{
				return _productoServicio.NCPICargaPedido(request, TokenCookie).Result;
			}
			catch (Exception)
			{
				return [];
			}
		}

		protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
		{
			var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
			ProveedorFamiliaLista = adms;
		}

		private SelectList ComboProveedores()
		{
			var adms = _cuentaServicio.ObtenerListaProveedores("BI", TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ComboRubros()
		{
			var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		//private SelectList ComboProveedoresFamilia(string ctaId)
		//{
		//	var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
		//	var lista = adms.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_lista });
		//	return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		//}
		private SelectList ComboSucursales()
		{
			var adms = _administracionServicio.GetAdministracionLogin();
			var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboSucursales(string exclude)
		{
			var adms = _administracionServicio.GetAdministracionLogin();
			if (adms != null && adms.Count > 0)
			{
				adms = adms.Where(x => x.Id != exclude).ToList();
			}
			else
			{
				return HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
			var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio, "BI");
			}

			if (RubroLista.Count == 0 || actualizar)
			{
				ObtenerRubros(_rubroServicio);
			}
		}
		#endregion
	}
}
