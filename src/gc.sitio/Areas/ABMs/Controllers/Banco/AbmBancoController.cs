using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Banco
{
	[Area("ABMs")]
	public class AbmBancoController : BancoControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ILogger<AbmBancoController> _logger;
		private readonly IABMBancoServicio _iAbmBancoServicio;
		private readonly IBancoServicio _iBancoServicio;
		private readonly ITipoMonedaServicio _tipoMonedaServicio;
		private readonly ITipoCuentaBcoServicio _tipoCuentaBcoServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly IAbmServicio _abmServicio;

		public AbmBancoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmBancoController> logger,
								  ITipoMonedaServicio tipoMonedaServicio, ITipoCuentaBcoServicio tipoCuentaBcoServicio, ITipoGastoServicio tipoGastoServicio,
								  IFinancieroServicio financieroServicio, IABMBancoServicio iAbmBancoServicio, IBancoServicio iBancoServicio, IAbmServicio abmServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_logger = logger;
			_tipoMonedaServicio = tipoMonedaServicio;
			_tipoCuentaBcoServicio = tipoCuentaBcoServicio;
			_tipoCuentaBcoServicio = tipoCuentaBcoServicio;
			_tipoGastoServicio = tipoGastoServicio;
			_tipoGastoServicio = tipoGastoServicio;
			_financieroServicio = financieroServicio;
			_iAbmBancoServicio = iAbmBancoServicio;
			_iBancoServicio = iBancoServicio;
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
			ViewData["Titulo"] = "CUENTAS BANCOS";
			CargarDatosIniciales(actualizar);

			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "ctaf_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMBancoSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMBancoSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaBanco == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = BancosBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					BancosBuscados = lista;
				}
				//else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				else
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _iAbmBancoServicio.BuscarBancos(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					BancosBuscados = lista;
				}
				metadata = MetadataBanco;

				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(BancosBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

				return View("_gridAbmBanco", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Bancos";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda Bancos");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}


		}

		[HttpPost]
		public async Task<IActionResult> BuscarBanco(string ctafId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctafId))
					return PartialView("_tabDatosBanco", new BancoAbmModel());

				var res = _iBancoServicio.GetBancoParaABM(ctafId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosBanco", new BancoAbmModel());

				var BancoModel = new BancoAbmModel()
				{
					Banco = res.First(),
					Moneda = ComboMoneda(),
					CuentaContable = ComboCuentaPlanContableLista(),
					TipoCuenta = ComboTipoCuentaBco(),
					CuentaGasto = ComboTipoGasto()
				};
				return PartialView("_tabDatosBanco", BancoModel);
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

		[HttpPost]
		public async Task<IActionResult> NuevoBanco()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var BancoModel = new BancoAbmModel()
				{
					Banco = new BancoDto(),
					Moneda = ComboMoneda(),
					CuentaContable = ComboCuentaPlanContableLista(),
					TipoCuenta = ComboTipoCuentaBco(),
					CuentaGasto = ComboTipoGasto()

				};
				return PartialView("_tabDatosBanco", BancoModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Alta de Banco - Nueva entidad";
				_logger.LogError(ex, "Error en la invocación de la API - Alta de Banco - Nueva entidad");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult DataOpsBanco(BancoAbmValidationModel banco, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(banco, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					banco = HelperGen.PasarAMayusculas(banco);
					var jsonstring = JsonConvert.SerializeObject(banco, new JsonSerializerSettings());
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

		[HttpPost]
		public JsonResult BuscarBancoCargado(string ctafId)
		{
			if (string.IsNullOrWhiteSpace(ctafId))
				return Json(new { error = true, warn = false, msg = "", data = "" });

			var res = _iBancoServicio.GetBancoParaABM(ctafId, TokenCookie);
			if (res == null)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			if (res.Count == 0)
				return Json(new { error = true, warn = false, msg = "", data = "" });
			return Json(new { error = false, warn = false, msg = "", data = res.First().Ban_Razon_Social });
		}

		#region Métodos Privados
		private string ValidarJsonAntesDeGuardar(BancoAbmValidationModel banco, char abm)
		{
			return string.Empty;
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoMonedaLista.Count == 0 || actualizar)
				ObtenerTipoMoneda(_tipoMonedaServicio);
			if (TipoCuentaBcoLista.Count == 0 || actualizar)
				ObtenerTiposDeCuentaBco(_tipoCuentaBcoServicio);
			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);
			if (CuentaPlanContableLista.Count == 0 || actualizar)
				ObtenerCuentaPlanContableLista(_financieroServicio);
		}
		#endregion
	}
}
