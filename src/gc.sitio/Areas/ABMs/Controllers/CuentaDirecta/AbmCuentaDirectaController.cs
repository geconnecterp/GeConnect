using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.Areas.ABMs.Models.CuentaDirecta;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.CuentaDirecta
{
	[Area("ABMs")]
	public class AbmCuentaDirectaController : CuentaDirectaControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ILogger<AbmCuentaDirectaController> _logger;
		private readonly ITipoCuentaGastoServicio _tipoCuentaGastoServicio;
		private readonly IABMCuentaDirectaServicio _aBMCuentaDirectaServicio;
		private readonly ICuentaGastoServicio _cuentaGastoServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly IAbmServicio _abmServicio;
		public AbmCuentaDirectaController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmCuentaDirectaController> logger,
										  ITipoCuentaGastoServicio tipoCuentaGastoServicio, IABMCuentaDirectaServicio aBMCuentaDirectaServicio, ICuentaGastoServicio cuentaGastoServicio,
										  IFinancieroServicio financieroServicio, IAbmServicio abmServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_logger = logger;
			_tipoCuentaGastoServicio = tipoCuentaGastoServicio;
			_aBMCuentaDirectaServicio = aBMCuentaDirectaServicio;
			_cuentaGastoServicio = cuentaGastoServicio;
			_financieroServicio = financieroServicio;
			_abmServicio = abmServicio;
		}

		[HttpGet]
		public async Task<IActionResult> Index(bool actualizar = false)
        {
			MetadataGrid metadata;

			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			CargarDatosIniciales(actualizar);

			var listR01 = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

			var listR02 = new List<ComboGenDto>();
			ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

			return View();
        }

		[HttpPost]
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "ctag_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMCuentaDirectaSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMCuentaDirectaSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaBanco == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = CuentaDirectaBuscadas.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					CuentaDirectaBuscadas = lista;
				}
				//else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				else
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _aBMCuentaDirectaServicio.BuscarCuentasDirectas(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					CuentaDirectaBuscadas = lista;
				}
				metadata = MetadataCuentaDirecta;

				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(CuentaDirectaBuscadas, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

				return View("_gridAbmCuentaDirecta", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Cuentas Directas";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda Cuentas Directas");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarCuentaDirecta(string ctagId, string tcgId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctagId))
					return PartialView("_tabDatosCuentaDirecta", new CuentaDirectaAbmModel());

				var res = _cuentaGastoServicio.GetCuentaDirectaParaABM(ctagId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosCuentaDirecta", new CuentaDirectaAbmModel());

				var cuentaDirectaModel = new CuentaDirectaAbmModel()
				{
					CuentaDirecta = res.Where(x=>x.Tcg_Id.Equals(tcgId)).First(),
					TipoCuenta = ComboTipoCuentaGastoLista(),
					CuentaContable = ComboCuentaPlanContableLista(),
				};
				return PartialView("_tabDatosCuentaDirecta", cuentaDirectaModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Obtener Cuenta Directa";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Obtener Cuenta Directa");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> NuevaCuentaDirecta()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var cuentaDirectaModel = new CuentaDirectaAbmModel()
				{
					CuentaDirecta = new CuentaGastoDto(),
					TipoCuenta = ComboTipoCuentaGastoLista(),
					CuentaContable = ComboCuentaPlanContableLista(),
				};
				return PartialView("_tabDatosCuentaDirecta", cuentaDirectaModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Alta de Cuenta Directa - Nueva entidad";
				_logger.LogError(ex, "Error en la invocación de la API - Alta de Cuenta Directa - Nueva entidad");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult BuscarCuentaDirectaCargada(string ctagId)
		{
			if (string.IsNullOrWhiteSpace(ctagId))
				return Json(new { error = true, warn = false, msg = "", data = "" });

			var res = _cuentaGastoServicio.GetCuentaDirectaParaABM(ctagId, TokenCookie);
			if (res == null)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			if (res.Count == 0)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			return Json(new { error = false, warn = false, msg = "", data = res.First().Ctag_Denominacion });
		}

		[HttpPost]
		public JsonResult DataOpsCuentaDirecta(CuentaDirectaAbmValidationModel cd, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(cd, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					cd = HelperGen.PasarAMayusculas(cd);
					var jsonstring = JsonConvert.SerializeObject(cd, new JsonSerializerSettings());
					var respuesta = _abmServicio.AbmConfirmar(ObtenerRequestParaABM(tipoDeOperacion, destinoDeOperacion, jsonstring, AdministracionId, UserName), TokenCookie).Result;
					return AnalizarRespuesta(respuesta);
				}
				else
					return Json(new { error = true, warn = false, msg = respuestaDeValidacion, codigo = 1, setFocus = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, msg = "Ha ocurrido un error al intentar actualizar la información." });
			}
		}

		#region Métodos Privados
		private string ValidarJsonAntesDeGuardar(CuentaDirectaAbmValidationModel banco, char abm)
		{
			return string.Empty;
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoCuentaGastoLista.Count == 0 || actualizar)
				ObtenerTipoCuentaGasto(_tipoCuentaGastoServicio);
			if (CuentaPlanContableLista.Count == 0 || actualizar)
				ObtenerCuentaPlanContableLista(_financieroServicio);
		}
		#endregion

		#region Carga De Listas en sección filtros Base
		[HttpPost]
		public JsonResult BuscarR01(string prefix)
		{
			var tipoNeg = TipoCuentaGastoLista.Where(x => x.tcg_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipoNegs = tipoNeg.Select(x => new ComboGenDto { Id = x.tcg_id, Descripcion = x.tcg_lista });
			return Json(tipoNegs);
		}
		#endregion
	}
}
