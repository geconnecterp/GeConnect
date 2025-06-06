﻿using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class NDeCYPIController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		public NDeCYPIController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio,
								 IAdministracionServicio administracionServicio, ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
		{
			_appSettings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_productoServicio = productoServicio;
			_administracionServicio = administracionServicio;
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

				ViewData["Titulo"] = "NECESIDADES DE PEDIDOS INTERNOS";
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
				//model = ObtenerGridCoreSmart<ProductoNCPIDto>(productos);
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
			var model = new GridCoreSmart<ProductoNCPIDto>();
			MetadataGrid metadata;
			GridCoreSmart<ProductoNCPIDto> grillaDatos;
			try
			{
				request.Registros = _appSettings.NroRegistrosPagina;
				request.Adm_Id = AdministracionId;
				request.Usu_Id = UserName;
				var productos = _productoServicio.NCPICargarListaDeProductosPag2(request, TokenCookie).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				//grillaDatos = GenerarGrillaSmart(ListaDeUsuarios, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				grillaDatos = GenerarGrillaSmart(productos.Item1, request.Sort ?? "p_desc", _appSettings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir ?? "ASC");
				//model = ObtenerGridCoreSmart<ProductoNCPIDto>(productos);
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

		private static void ObtenerColor(ref List<ProductoNCPIDto> listaProd)
		{
			foreach (var item in listaProd)
			{
				if (item.p_activo == "D") //Discontinuo
					item.Row_color = "#fc4641";
			}
		}

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

		public async Task<JsonResult> CargaPedidoOCPI(string tipo, string pId, string tipoCarga, int bultos)
		{
			try
			{
				var request = new NCPICargaPedidoRequest() { admId = AdministracionId, usuId = UserName, tipo = tipo, pId = pId, tipoCarga = tipoCarga, bultos = bultos };
				var response = await _productoServicio.NCPICargaPedido(request, TokenCookie);
				//var response = new List<NCPICargaPedidoResponse> //mocked response
				//{
				//	new() { bultos = 10, cantidad = 15, pallet = 20, p_pcosto = 12, resultado = 0, resultado_msj = "", unidad_pres = 4 }
				//};
				if (response == null)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.Count == 0)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.First().resultado != 0)
					return Json(new { error = false, warn = true, msg = response.First().resultado_msj });
				var item = response.First();
				return Json(new { error = false, warn = false, msg = string.Empty, unidadPres = item.unidad_pres, pCosto = item.p_pcosto, bulto = item.bultos, cantidad = item.cantidad, pallet = item.pallet, pCostoTotal = item.p_pcosto * item.cantidad });
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al intentar setear el estado del remito.");
				TempData["error"] = "Hubo algun problema al intentar setear el estado del remito. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, msg = "Error al intentar setear el estado del remito." });
			}
		}

		public  IActionResult ObtenerProveedoresFamilia(string ctaId)
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

		//Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
		[HttpPost]
		public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
		{
			try
			{
				CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception )
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

		#region Métodos privados
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
