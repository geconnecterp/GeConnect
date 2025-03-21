using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDeCompraConsultaController : OrdenDeCompraConsultaControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IAdministracionServicio _adminServicio;
		private readonly IOrdenDeCompraEstadoServicio _ordenDeCompraEstadoServicio;
		private readonly IProductoServicio _productoServicio;
		public OrdenDeCompraConsultaController(ICuentaServicio cuentaServicio, ILogger<OrdenDeCompraController> logger, IAdministracionServicio adminServicio, 
											   IOrdenDeCompraEstadoServicio ordenDeCompraEstadoServicio, IOptions<AppSettings> options, IHttpContextAccessor context, 
											   IProductoServicio productoServicio) : base(options, context, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_adminServicio = adminServicio;
			_ordenDeCompraEstadoServicio = ordenDeCompraEstadoServicio;
			_productoServicio = productoServicio;
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

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				ViewData["Titulo"] = "CONSULTA DE ORDENES DE COMPRA";
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

		//TODO MARCE: Desarrollar vista parcial de grilla de ordenes de compra
		public async Task<IActionResult> BuscarOrdenesDeCompra(BuscarOrdenesDeCompraRequest request) 
		{
			MetadataGrid metadata;
			GridCore<OrdenDeCompraConsultaDto> grillaDatos;
			try
			{
				request.Registros = _settings.NroRegistrosPagina;
				var productos = _productoServicio.CargarOrdenDeCompraConsultaLista(request, TokenCookie).Result;
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				ListaOrdenDeCompraConsulta = productos.Item1;
				grillaDatos = GenerarGrilla(productos.Item1, request.Sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir);
				return PartialView("_grillaProductosOC", grillaDatos);
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
		public JsonResult BuscarSucursales(string prefix)
		{
			if (AdministracionesLista == null || AdministracionesLista.Count <= 0)
			{
				ObtenerAdministracionesLista(_adminServicio);
			}
			var adms = AdministracionesLista.Where(x => x.Adm_nombre.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return Json(lista);
		}

		[HttpPost]
		public JsonResult BuscarEstadosDeOC(string prefix)
		{
			if (OrdenDeCompraEstadoLista == null || OrdenDeCompraEstadoLista.Count <= 0)
			{
				ObtenerOrdenDeCompraEstadoLista(_ordenDeCompraEstadoServicio);
			}
			var ocs = OrdenDeCompraEstadoLista.Where(x => x.oce_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = ocs.Select(x => new ComboGenDto { Id = x.oce_id, Descripcion = x.oce_lista });
			return Json(lista);
		}

		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio);
			}

			if (OrdenDeCompraEstadoLista.Count == 0 || actualizar)
			{
				ObtenerOrdenDeCompraEstadoLista(_ordenDeCompraEstadoServicio);
			}

			if (AdministracionesLista.Count == 0 || actualizar)
			{
				ObtenerAdministracionesLista(_adminServicio);
			}
		}
		#endregion
	}
}
