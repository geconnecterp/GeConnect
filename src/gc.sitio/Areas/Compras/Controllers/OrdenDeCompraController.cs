using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using gc.sitio.Areas.Compras.Models;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDeCompraController : OrdenDeCompraControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		public OrdenDeCompraController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio, ILogger<OrdenDeCompraController> logger,
									   IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
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

				ViewData["Titulo"] = "ORDEN DE COMPRA";
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

		public async Task<IActionResult> BuscarProductos(NCPICargarListaDeProductos2Request request)
		{
			MetadataGrid metadata;
			GridCore<ProductoNCPIDto> grillaDatos;
			try
			{
				if (request.Rel01 == null || request.Rel01.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Se debe proporcionar una cuenta."
					};
					return PartialView("_gridMensaje", response);
				}
				
				request.Registros = _settings.NroRegistrosPagina;
				request.Adm_Id = AdministracionId;
				request.Usu_Id = UserName;
				var productos = _productoServicio.NCPICargarListaDeProductosPag2(request, TokenCookie).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				grillaDatos = GenerarGrilla(productos.Item1, request.Sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir);
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

		public async Task<IActionResult> BuscarProductosTabOC(string ctaId, string ocCompte)
		{
			ProductoParaOcModel model = new();
			GridCore<ProductoParaOcDto> grillaDatos;
			try
			{
				CtaIdSelected = ctaId;
				CargarProductoParaOcRequest request = new() 
				{
					Adm_Id = AdministracionId,
					Usu_Id = UserName,
					Cta_Id = ctaId,
					Nueva = string.IsNullOrEmpty(ocCompte),
					Oc_Compte = ocCompte
				};
				var productos = _productoServicio.CargarProductosDeOC(request, TokenCookie).Result;
				grillaDatos = ObtenerGridCore<ProductoParaOcDto>(productos);
				model.ListaOC = grillaDatos;
				CalcularTotalesParaOC(model, productos);
				return PartialView("_grillaProductosOC", model);
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
				CtaIdSelected = ctaId;
				CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public JsonResult CargarOCDesdeProveedorSeleccionado(string ctaId)
		{
			try
			{
				CtaIdSelected = ctaId;
				CargarOrdenesDeCompraLista(ctaId, _productoServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de las OC del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public JsonResult BuscarFlias(string prefix)
		{
			if ((ProveedorFamiliaLista == null || ProveedorFamiliaLista.Count <= 0) && (!string.IsNullOrEmpty(CtaIdSelected)))
			{
				BuscarFamiliaDesdeProveedorSeleccionado(CtaIdSelected);
			}
			var rub = ProveedorFamiliaLista.Where(x => x.pg_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var rubros = rub.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_lista });
			return Json(rubros);
		}

		[HttpPost]
		public JsonResult BuscarOCPendientes(string prefix)
		{
			var oc = OrdenDeCompraLista.Where(x => x.oc_compte.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var ocs = oc.Select(x => new ComboGenDto { Id = x.oc_compte, Descripcion = x.oc_compte });
			return Json(ocs);
		}

		#region Métodos privados
		private void CalcularTotalesParaOC(ProductoParaOcModel model, List<ProductoParaOcDto> productos)
		{
			if (productos == null || productos.Count == 0)
			{
				model.Total_Costo = "0.00";
				model.Total_Pallet = "0.00";
			}
			else
			{
				model.Total_Costo = productos.Sum(x => x.P_Pcosto_Total).ToString("0.##");
				model.Total_Pallet = productos.Sum(x => x.Paletizado).ToString("0.##");
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
		protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
		{
			var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
			ProveedorFamiliaLista = adms;
		}
		protected void CargarOrdenesDeCompraLista(string ctaId, IProductoServicio _productoServicio)
		{
			var adms = _productoServicio.CargarOrdenesDeCompraList(ctaId, AdministracionId, UserName, TokenCookie);
			OrdenDeCompraLista = adms;
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio);
			}

			if (RubroLista.Count == 0 || actualizar)
			{
				ObtenerRubros(_rubroServicio);
			}
		}
		#endregion
	}
}
