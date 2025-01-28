using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class AbmSectorController : SectorControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ILogger<AbmSectorController> _logger;
		private readonly IABMSectorServicio _abmsectorServicio;
		private readonly ISectorServicio _sectorServicio;
		public AbmSectorController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmSectorController> logger,
								   IABMSectorServicio abmsectorServicio, ISectorServicio sectorServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_logger = logger;
			_abmsectorServicio = abmsectorServicio;
			_sectorServicio = sectorServicio;
		}

		[HttpGet]
		public IActionResult Index(bool actualizar = false)
		{
			MetadataGrid metadata;

			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			CargarDatosIniciales(actualizar);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "sec_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMSectorSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMSectorSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaSector == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = SectoresBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					SectoresBuscados = lista;
				}
				//else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				else
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _abmsectorServicio.BuscarSectores(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataSector = res.Item2 ?? null;
					metadata = MetadataSector;
					SectoresBuscados = lista;
				}
				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(SectoresBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataSector.TotalCount, MetadataSector.TotalPages, sortDir);

				return View("_gridAbmSector", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Cliente";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda Cliente");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}


		}

		//[HttpPost]
		//public new JsonResult ObtenerDatosPaginacion()
		//{
		//	try
		//	{
		//		return Json(new { error = false, Metadata = MetadataSector });
		//	}
		//	catch (Exception ex)
		//	{
		//		return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
		//	}
		//}

		[HttpPost]
		public async Task<IActionResult> BuscarSector(string secId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(secId))
					return PartialView("_tabDatosSector", new SectorAbmModel());

				var res = _sectorServicio.GetSectorParaABM(secId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosCliente", new CuentaAbmModel());

				var SectorModel= new SectorAbmModel() 
				{ 
					Sector= res.First()
				};
				return PartialView("_tabDatosSector", SectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Sector";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Sector");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
		}
		#endregion
	}
}
