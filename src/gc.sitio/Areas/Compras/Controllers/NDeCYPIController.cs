using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Response;
using gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class NDeCYPIController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly ILogger<CompraController> _logger;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		public NDeCYPIController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio, ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> NecesidadesDeCompra()
		{
			NecesidadesDeCompraDto model = new();
			List<ProveedorFamiliaListaDto> proveedoresFamilias = [];
			try
			{
				model.ComboProveedores = ComboProveedores();
				model.ComboProveedoresFamilia = HelperMvc<ComboGenDto>.ListaGenerica(proveedoresFamilias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc }));
				model.ComboRubros = ComboRubros();
				model.Productos = ObtenerGridCore<ProductoNCPIDto>([]);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al inicializar vista Necesidades de Compra");
				TempData["error"] = "Hubo algun problema al inicializar vista Necesidades de Compra. Si el problema persiste informe al Administrador";
				model = new();
			}
			return View(model);
		}

		public async Task<IActionResult> PedidosInternos()
		{
			PedidosInternosDto model = new();
			List<ProveedorFamiliaListaDto> proveedoresFamilias = [];
			try
			{
				model.ComboProveedores = ComboProveedores();
				model.ComboProveedoresFamilia = HelperMvc<ComboGenDto>.ListaGenerica(proveedoresFamilias.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc }));
				model.ComboRubros = ComboRubros();
				model.Productos = ObtenerGridCore<ProductoNCPIDto>([]);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener intentar obtener la vista de Pedidos Internos");
				TempData["error"] = "Hubo algun problema al intentar obtener la vista de Pedidos Internos. Si el problema persiste informe al Administrador";
				model = new();
			}
			return View(model);
		}

		public async Task<IActionResult> BuscarProductosOCPI(string filtro, string id, string tipo)
		{
			var model = new GridCore<ProductoNCPIDto>();
			try
			{
				var productos = _productoServicio.NCPICargarListaDeProductos(tipo, AdministracionId, filtro, id, TokenCookie).Result;
				model = ObtenerGridCore<ProductoNCPIDto>(productos);
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

		public async Task<JsonResult> CargaPedidoOCPI(string tipo, string pId, string tipoCarga, int bultos)
		{
			try
			{
				var request = new NCPICargaPedidoRequest() { admId = AdministracionId, usuId = UserName, tipo = tipo, pId = pId, tipoCarga = tipoCarga, bultos = bultos };
				//var response = await _productoServicio.NCPICargaPedido(request, TokenCookie);
				var response = new List<NCPICargaPedidoResponse> //mocked response
				{
					new() { bultos = 10, cantidad = 15, pallet = 20, p_pcosto = 12, resultado = 0, resultado_msj = "", unidad_pres = 4 }
				};
				if (response == null)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.Count == 0)
					return Json(new { error = true, warn = false, msg = "Error al intentar cargar el pedido." });
				if (response.First().resultado != 0)
					return Json(new { error = false, warn = true, msg = response.First().resultado_msj });
				var item = response.First();
				return Json(new { error = false, warn = false, msg = string.Empty, unidadPres = item.unidad_pres, pCosto = item.p_pcosto, bulto = item.bultos, cantidad = item.cantidad, pallet = item.pallet });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar setear el estado del remito.");
				TempData["error"] = "Hubo algun problema al intentar setear el estado del remito. Si el problema persiste informe al Administrador";
				return Json(new { error = true, warn = false, msg = "Error al intentar setear el estado del remito." });
			}
		}

		public async Task<IActionResult> ObtenerProveedoresFamilia(string ctaId)
		{
			var model = new ProveedoresFamiliaDto();
			try
			{
				model.ComboProveedoresFamilia = ComboProveedoresFamilia(ctaId);
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
				return PartialView("_listaProveedoresFamilia", response);
			}
		}

		#region Métodos privados
		private SelectList ComboProveedores()
		{
			var adms = _cuentaServicio.ObtenerListaProveedores(TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Denominacion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboRubros()
		{
			var adms = _rubroServicio.ObtenerListaRubros(TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboProveedoresFamilia(string ctaId)
		{
			var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
