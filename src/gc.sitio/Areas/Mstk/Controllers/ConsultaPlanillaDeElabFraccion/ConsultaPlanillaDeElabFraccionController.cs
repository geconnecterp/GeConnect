using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.ConsultaDeAjusteDeStock;
using gc.sitio.Areas.Mstk.Models.ConsultaPlanillaDeElabFraccion;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaPlanillaDeElabFraccion
{
	[Area("Mstk")]
	public class ConsultaPlanillaDeElabFraccionController : ConsultaPlanillaDeElabFraccionControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IProductoServicio _prdSrv;
		private readonly IAdministracionServicio _administracionServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.PLANILLA_DE_ELABORACION_Y_FRACCION.ToString();
		private readonly IDocManagerServicio _docMSv;

		public ConsultaPlanillaDeElabFraccionController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaPlanillaDeElabFraccionController> logger,
														IProductoServicio prdSrv, IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
														IAdministracionServicio administracionServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_prdSrv = prdSrv;
			_administracionServicio = administracionServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: CC_NR_NP
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsultaPlanillaDeElabFraccionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE PLANILLA DE ELABORACIÓN Y FRACCIÓN";
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

		public IActionResult InicializarPantallPrincipal(string sucursalesText, DateTime f_desde, DateTime f_hasta)
		{
			var model = new PrincipalConsultaDePlanillaDeElabFraccionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Sucursales = sucursalesText;
				model.Desde = f_desde;
				model.Hasta = f_hasta;
				return PartialView("_pantallaPrincipal");
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
		public async Task<IActionResult> CargarPlanillaElab(string sucIdsList, DateTime f_desde, DateTime f_hasta, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new PlanillaModel();
			var lista = new List<PlanillaElabYFraccionListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<PlanillaElabYFraccionListaDto> grillaDatos;

			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (!buscaNew)
				{
					lista = ListaplanillaElabYFraccion.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaplanillaElabYFraccion = lista;
				}
				else
				{
					var request = new PlanillaElabYFraccionListaRequest
					{
						FechaDesde = f_desde,
						FechaHasta = f_hasta,
						adm_list = sucIdsList,
						Sort = sort,
						SortDir = sortDir,
						Registros = _setting.NroRegistrosPagina
					};
					//request.Pagina = pag;

					var res = await _prdSrv.ObtenerPlanillaElabFraccionLista(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaplanillaElabYFraccion = lista;

				}
				metadata = MetadataListaPlanillaElabYFraccion;
				grillaDatos = GenerarGrillaSmart(ListaplanillaElabYFraccion, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.Grilla = grillaDatos;
				return PartialView("_gridListadoPlanilla", model);

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
		public async Task<IActionResult> ObtenerDetallePlanillaElab(string ep_compte)
		{
			var model = new PlanillaDetalleModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				//List<DevolucionRevertidoDto>
				var lista = await _prdSrv.ObtenerPlanillaDeElabYFraccion(ep_compte, TokenCookie);
				model.Grilla = ObtenerGridCoreSmart<PlanillaElabYFraccionDto>(lista);
				model.Leyenda = ep_compte;

				return PartialView("_gridListadoDetalle", model);
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

		#region Metodos Privados
		private void CargarDatosIniciales(ConsultaPlanillaDeElabFraccionModel model)
		{
			model.Desde = ObtenerPrimerDiaDelMesActual();
			model.Hasta = DateTime.Now.Date;
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", Token);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursales = ObtenerSucursales(sucursales);
				var suc = sucursales.Where(x => x.Adm_id == AdministracionId).FirstOrDefault();
			}
			else
			{
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerSucursales(List<AdministracionDto> administraciones)
		{
			var lista = administraciones.Select(a => new ComboGenDto
			{
				Id = a.Adm_id,
				Descripcion = a.Adm_nombre
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		public static DateTime ObtenerPrimerDiaDelMesActual()
		{
			return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		}
		#endregion
	}
}
