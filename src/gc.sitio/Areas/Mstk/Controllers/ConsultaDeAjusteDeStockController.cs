using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.ConsultaDeAjusteDeStock;
using gc.sitio.Areas.Mstk.Models.ConsultaDeRecepcionDeProveedores;
using gc.sitio.Areas.Mstk.Models.ConsultaDevolucionAProveedores;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeAjusteDeStockController : ConsultaDeAjusteDeStockControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IProductoServicio _productoServicio;
		public ConsultaDeAjusteDeStockController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeAjusteDeStockController> logger,
												 IAdministracionServicio administracionServicio, IProductoServicio productoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeAjusteDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE AJUSTE DE STOCK";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(model);

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

		[HttpPost]
		public IActionResult InicializarPantallPrincipal(string sucursalesText, DateTime f_desde, DateTime f_hasta)
		{
			var model = new PrincipalConsultaDeAjusteDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Sucursales = sucursalesText;
				model.Desde = f_desde;
				model.Hasta = f_hasta;
				return PartialView("_pantallaPrincipal", model);
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
		public async Task<IActionResult> CargarAjustesDeStock(string sucIdsList, DateTime f_desde, DateTime f_hasta, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new AjustesModel();
			var lista = new List<AjusteDeStockListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<AjusteDeStockListaDto> grillaDatos;

			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (!buscaNew)
				{
					lista = ListaAjustes.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaAjustes = lista;
				}
				else
				{
					var request = new CargarAjusteDeStockListaRequest
					{
						fecha_d = f_desde,
						fecha_h = f_hasta,
						adm_list = sucIdsList,
						Sort = sort,
						SortDir = sortDir,
						Registros = _setting.NroRegistrosPagina
					};
					//request.Pagina = pag;

					var res = await _productoServicio.ObtenerAjusteDeStockLista(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaAjustes = lista;

				}
				metadata = MetadataListaAjustes;
				grillaDatos = GenerarGrillaSmart(ListaAjustes, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaAjustes = grillaDatos;
				return PartialView("_grillaAjustes", model);

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
		public async Task<IActionResult> ObtenerDetalleAjustes(string as_compte)
		{
			var model = new AjusteDeStockDetalleModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				//List<DevolucionRevertidoDto>
				var lista = await _productoServicio.ObtenerAJREVERTIDO(as_compte, TokenCookie);
				model.GrillaAjusteDetalle = ObtenerGridCoreSmart<AjusteRevertidoDto>(lista);
				model.Leyenda = as_compte;

				return PartialView("_grillaDetalle", model);
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

		#region Metodos privados
		private void CargarDatosIniciales(ConsultaDeAjusteDeStockModel model)
		{
			var hoy = DateTime.Today;
			model.Desde = new DateTime(hoy.Year, hoy.Month, 1);
			model.Hasta = DateTime.Today;

			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}

		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
