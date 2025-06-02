using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class AbmSectorController : SectorControladorBase
	{
		private readonly AppSettings _settings;
		private readonly IABMSectorServicio _abmsectorServicio;
		private readonly IAbmServicio _abmServicio;
		private readonly ISectorServicio _sectorServicio;
		private readonly IABMProductoServicio _abmProdServ;
		public AbmSectorController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmSectorController> logger,
								   IABMSectorServicio abmsectorServicio, ISectorServicio sectorServicio, IAbmServicio abmServicio,
								   IABMProductoServicio abmProdServ) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_abmsectorServicio = abmsectorServicio;
			_sectorServicio = sectorServicio;
			_abmServicio = abmServicio;
			_abmProdServ = abmProdServ;
		}

		[HttpGet]
		public IActionResult Index(bool actualizar = false)
		{

			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			ViewData["Titulo"] = "ABM SECTORES";
			CargarDatosIniciales(actualizar);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "sec_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMSectorSearchDto> lista;
			MetadataGrid metadata;
			GridCoreSmart<ABMSectorSearchDto> grillaDatos;
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
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					SectoresBuscados = lista;
				}
				metadata = MetadataSector;

				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrillaSmart(SectoresBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

				return View("_gridAbmSector", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Cliente";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda Cliente");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}


		}

		[HttpPost]
		public IActionResult BuscarSector(string secId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(secId))
					return PartialView("_tabDatosSector", new SectorAbmModel());

				var res = _sectorServicio.GetSectorParaABM(secId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosCliente", new SectorAbmModel());

				var subSectores = _sectorServicio.GetSubSectorParaABM(secId, TokenCookie);
				SubSectorLista = subSectores;
				var SectorModel = new SectorAbmModel()
				{
					Sector = res.First()
				};
				return PartialView("_tabDatosSector", SectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Sector";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Sector");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult NuevoSector()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var SectorModel = new SectorAbmModel()
				{
					Sector = new SectorDto()
				};
				return PartialView("_tabDatosSector", SectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Alta de Sector - Nueva entidad";
				_logger?.LogError(ex, "Error en la invocación de la API - Alta de Sector - Nueva entidad");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult DataOpsSector([FromBody]SectorAbmValidationModel sector, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(sector, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					sector = HelperGen.PasarAMayusculas(sector);
					var jsonstring = JsonConvert.SerializeObject(sector, new JsonSerializerSettings());
					var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
					return AnalizarRespuesta(respuesta);
				}
				else
					return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
			}
			catch (Exception )
			{
				return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
			}
		}

		[HttpPost]
		public JsonResult BuscarSectorCargado(string secId)
		{
			if (string.IsNullOrWhiteSpace(secId))
				return Json(new { error = true, warn = false, msg = "", data = "" });

			var res = _sectorServicio.GetSectorParaABM(secId, TokenCookie);
			if (res == null)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			if (res.Count == 0)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			return Json(new { error = false, warn = false, msg = "", data = res.First().Sec_Desc });
		}

		[HttpPost]
		public IActionResult BuscarSubSector(string secId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(secId))
					return PartialView("_tabDatosSubSector", new SectorABMSubSectorModel());

				var res = _sectorServicio.GetSubSectorParaABM(secId, TokenCookie);

				if (res == null)
					return PartialView("_tabDatosSubSector", new SectorABMSubSectorModel());

				var SubSectorModel = new SectorABMSubSectorModel()
				{
					SectorSubSector = ObtenerGridCoreSmart<SubSectorDto>(res),
					SubSector = new SubSectorModel()
				};
				return PartialView("_tabDatosSubSector", SubSectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult BuscarDatosSubSector(string ssId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ssId))
					return PartialView("_tabDatosSubSectorSelected", new SectorABMSubSectorSelectedModel());

				var res = _sectorServicio.GetSubSector(ssId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosSubSectorSelected", new SectorABMSubSectorSelectedModel());

				var SubSectorModel = new SectorABMSubSectorSelectedModel()
				{
					SubSector = ObtenerSubSectorModel(res.First())
				};
				return PartialView("_tabDatosSubSectorSelected", SubSectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector -> Datos";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector -> Datos");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult NuevoSubSector()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var ssm = new SubSectorModel();
				var model = new SectorABMSubSectorSelectedModel()
				{
					SubSector = ssm
				};
				return PartialView("_tabDatosSubSectorSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector -> Sub Sector Selected";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Sub Sector -> Sub Sector Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult DataOpsSubSector(SubSectorAbmValidationModel subSector, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(subSector, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					subSector = HelperGen.PasarAMayusculas(subSector);
					var jsonstring = JsonConvert.SerializeObject(subSector, new JsonSerializerSettings());
					var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
					return AnalizarRespuesta(respuesta);
				}
				else
					return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
			}
			catch (Exception )
			{
				return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
			}
		}

		[HttpPost]
		public IActionResult BuscarRubro(string secId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(secId))
					return PartialView("_tabDatosRubro", new SectorABMRubroModel());

				var res = _sectorServicio.GetRubroParaABM(secId, TokenCookie);

				if (res == null)
					return PartialView("_tabDatosRubro", new SectorABMRubroModel());

				//SubSectorLista = res.GroupBy(c => new { c.Rubg_Id, c.Rubg_Desc, c.Rubg_Lista })
				//					.Select(gc => new SubSectorDto() { Rubg_Id = gc.Key.Rubg_Id, Rubg_Desc = gc.Key.Rubg_Desc, Rubg_Lista = gc.Key.Rubg_Lista })
				//					.ToList();

				var SubSectorModel = new SectorABMRubroModel()
				{
					SectorRubro = ObtenerGridCoreSmart<RubroListaABMDto>(res),
					ComboSubSector = ComboSubSector(),
					Rubro = new RubroListaABMDto()
				};
				return PartialView("_tabDatosRubro", SubSectorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Rubro";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Rubro");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult BuscarDatosRubro(string rubId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(rubId))
					return PartialView("_tabDatosRubroSelected", new SectorABMRubroSelectedModel());

				var res = _sectorServicio.GetRubro(rubId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosRubroSelected", new SectorABMRubroSelectedModel());

				var RubroModel = new SectorABMRubroSelectedModel()
				{
					ComboSubSector = ComboSubSector(),
					Rubro = res.First()
				};

				return PartialView("_tabDatosRubroSelected", RubroModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Rubro -> Datos";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Rubro -> Datos");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
				throw;
			}
		}

		[HttpPost]
		public IActionResult NuevoRubro()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var r = new RubroListaABMDto();
				var model = new SectorABMRubroSelectedModel()
				{
					ComboSubSector = ComboSubSector(),
					Rubro = r
				};
				return PartialView("_tabDatosRubroSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Rubro -> Rubro Selected";
				_logger?.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Rubro -> Rubro Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult DataOpsRubro(RubroAbmValidationModel rubro, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(rubro, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					rubro = HelperGen.PasarAMayusculas(rubro);
					var jsonstring = JsonConvert.SerializeObject(rubro, new JsonSerializerSettings());
					var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
					return AnalizarRespuesta(respuesta);
				}
				else
					return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
			}
		}

		#region Reasignación de familia
		public IActionResult ReasignacionDeRubro()
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

			ViewData["Titulo"] = "REASIGNACIÓN DE RUBROS";
			return View();
		}

		public IActionResult BuscarRubrosPorSector(string secId)
		{
			var model = new ReasignacionRubroModel();

			if (string.IsNullOrEmpty(secId))
				return PartialView("_seccionReasignacion", model);

			var familia = _sectorServicio.GetRubroParaABM(secId, TokenCookie);
			if (familia == null)
				return PartialView("_seccionReasignacion", model);

			model.RubroProductos = ComboFamiliaDeProductos(familia, true);
			familia = _sectorServicio.GetRubroParaABM("%", TokenCookie);
			if (familia == null)
				model.RubroProductosAReasignar = ComboFamiliaDeProductos([], true);
			else
				model.RubroProductosAReasignar = ComboFamiliaDeProductos(familia, true);
			model.ProductosPorRubro = ObtenerGridCoreSmart<InfoProductoRubroDto>([]);
			return PartialView("_seccionReasignacion", model);
		}

		public async Task<IActionResult> BuscarProductosPorRubro(string secId, string rubroSelected)
		{
			var model = new ProdPorRubroModel();

			if (string.IsNullOrEmpty(secId) || string.IsNullOrEmpty(rubroSelected))
				return PartialView("_gridProdPorRubro", model);

			var flia = new ComboGenDto() { Id = secId, Descripcion = rubroSelected };
			var query = new QueryFilters()
			{
				Id = "",
				Buscar = "",
				Rel01 = [],
				Rel02 = [rubroSelected],
				Rel03 = [],
				Registros = 999999,
				Pagina = 1,
				Sort = "p_desc"
			};
			var res = await _abmProdServ.BuscarProducto(query, TokenCookie);
			if (res.Item1 == null)
				return PartialView("_gridProdPorRubro", model);

			model.ProductosPorRubro = ObtenerGridCoreSmart<ProductoListaDto>(res.Item1);
			return PartialView("_gridProdPorRubro", model);
		}

		public JsonResult ReasignarProductos(string secId, string rubroDest, string[] ids)
		{
			if (string.IsNullOrWhiteSpace(secId) || string.IsNullOrWhiteSpace(rubroDest) || ids.Length <= 0)
				return Json(new { error = true, warn = false, msg = "Alguno de los parámetros es erróneo", data = "" });

			var lista = new List<ProdNuevoRubro>();
			for (int i = 0; i < ids.Length; i++)
			{
				lista.Add(new ProdNuevoRubro() { rub_id = rubroDest, p_id = ids[i] });
			}
			var jsonstring = JsonConvert.SerializeObject(lista, new JsonSerializerSettings());
			var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM('A', "rubro_reasigna", jsonstring, AdministracionId, UserName), TokenCookie).Result;
			return AnalizarRespuesta(respuesta);
		}
		#endregion

		#region Métodos privados
		public class ProdNuevoRubro()
		{
			public string p_id { get; set; } = string.Empty;
			public string rub_id { get; set; } = string.Empty;
		}
		protected SelectList ComboFamiliaDeProductos(List<RubroListaABMDto> listaR, bool selectFirst = false)
		{
			var nuevaLista = listaR.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			var combo = HelperMvc<ComboGenDto>.ListaGenerica(nuevaLista);
			if (combo != null && combo.Any() && selectFirst)
			{
				foreach (var item in combo)
				{
					item.Selected = true;
					break;
				}
			}
			return combo ?? new SelectList(Enumerable.Empty<ComboGenDto>()); ;
		}
		private string ValidarJsonAntesDeGuardar(SectorAbmValidationModel sector, char abm)
		{
			return string.Empty;
		}
		private string ValidarJsonAntesDeGuardar(SubSectorAbmValidationModel subSector, char abm)
		{
			return string.Empty;
		}
		private string ValidarJsonAntesDeGuardar(RubroAbmValidationModel rubro, char abm)
		{
			return string.Empty;
		}
		private static SubSectorModel ObtenerSubSectorModel(SubSectorDto ss)
		{
			var mod = new SubSectorModel();
			if (ss == null)
				return mod;
			#region map
			mod.Rubg_Actu = ss.Rubg_Actu;
			mod.Rubg_Desc = ss.Rubg_Desc;
			mod.Rubg_Id = ss.Rubg_Id;
			mod.Rubg_Lista = ss.Rubg_Lista;
			mod.Sec_Desc = ss.Sec_Desc;
			mod.Sec_Id = ss.Sec_Id;
			#endregion
			return mod;
		}

		protected SelectList ComboSubSector()
		{
			var lista = SubSectorLista.Select(x => new ComboGenDto { Id = x.Rubg_Id, Descripcion = x.Rubg_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private void CargarDatosIniciales(bool actualizar)
		{
		}
		#endregion
	}
}
