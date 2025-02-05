using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace gc.sitio.Areas.ABMs.Controllers.MedioDePago
{
	[Area("ABMs")]
	public class AbmMedioDePagoController : MedioDePagoControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ILogger<AbmMedioDePagoController> _logger;
		private readonly IABMMedioDePagoServicio _abmMedioDePago;
		private readonly IMedioDePagoServicio _medioDePagoServicio;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly ITipoMonedaServicio _tipoMonedaServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;

		public AbmMedioDePagoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmMedioDePagoController> logger,
										IABMMedioDePagoServicio abmMedioDePago, ITipoCuentaFinServicio tipoCuentaFinServicio,
										IMedioDePagoServicio medioDePagoServicio, ITipoMonedaServicio tipoMonedaServicio,
										IFinancieroServicio financieroServicio, IAdministracionServicio administracionServicio,
										ITipoGastoServicio tipoGastoServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_logger = logger;
			_abmMedioDePago = abmMedioDePago;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_medioDePagoServicio = medioDePagoServicio;
			_tipoMonedaServicio = tipoMonedaServicio;
			_financieroServicio = financieroServicio;
			_administracionServicio = administracionServicio;
			_tipoGastoServicio = tipoGastoServicio;
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
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "ins_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMMedioDePagoSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMMedioDePagoSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaProd == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = MediosDePagoBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					MediosDePagoBuscados = lista;
				}
				//else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				else
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _abmMedioDePago.BuscarMediosDePago(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					MediosDePagoBuscados = lista;
				}
				metadata = MetadataGeneral;
				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(MediosDePagoBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

				//string volver = Url.Action("index", "home", new { area = "" });
				//ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

				return View("_gridAbmMedioDePago", grillaDatos);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda Medios de Pago";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda Medios de Pago");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}


		}

		[HttpPost]
		public async Task<IActionResult> BuscarMedioDePago(string insId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(insId))
					return PartialView("_tabDatosMedioDePago", new MedioDePagoAbmModel());

				var res = _medioDePagoServicio.GetMedioDePagoParaABM(insId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosMedioDePago", new MedioDePagoAbmModel());

				var medioDePagoModel = new MedioDePagoAbmModel()
				{
					MedioDePago = res.First(),
					ComboMoneda = ComboMoneda(),
					ComboFinanciero = ComboFinancieroRela(res.First().Tcf_Id),
					//TODO: Armar html con los datos
				};
				return PartialView("_tabDatosMedioDePago", medioDePagoModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Proveedor";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Proveedor");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarOpcionesCuotas(string insId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(insId))
					return PartialView("_tabDatosOpcionesCuotas", new MedioDePagoAbmOpcCuotaModel());

				var opcCuota = _medioDePagoServicio.GetOpcionesCuota(insId, TokenCookie);
				var model = new MedioDePagoAbmOpcCuotaModel()
				{
					OpcionCuota = new OpcionCuotaModel(),
					ListaOpcionesCuota = ObtenerGridCore<OpcionCuotaDto>(opcCuota)
				};
				return PartialView("_tabDatosOpcionesCuotas", model);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Opciones Cuotas";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Opciones Cuotas");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarOpcionCuota(string insId, int cuota)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(insId))
					return PartialView("_tabDatosOpcionesCuotasSelected", new MedioDePagoAbmOpcCuotaSelectedModel());

				var opcCuota = _medioDePagoServicio.GetOpcionCuota(insId, cuota, TokenCookie);
				var model = new MedioDePagoAbmOpcCuotaSelectedModel()
				{
					OpcionCuota = new OpcionCuotaModel() { Cuota = opcCuota.First().Cuota, Ins_Id = opcCuota.First().Ins_Id, Recargo = opcCuota.First().Recargo, Pos_Plan = opcCuota.First().Pos_Plan }
				};
				return PartialView("_tabDatosOpcionesCuotasSelected", model);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Opciones Cuotas";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Opciones Cuotas");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarCuentasFinYContable(string insId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(insId))
					return PartialView("_tabDatosCuentaFinYContable", new MedioDePagoAbmCuentaFinModel());

				var cuentasFin = _medioDePagoServicio.GetCuentaFinYContableLista(insId, TokenCookie);
				var model = new MedioDePagoAbmCuentaFinModel()
				{
					CuentaFin = new CuentaFinModel(),
					ComboTipo = ComboFinancierosEstados(),
					ComboCuentaGasto = ComboTipoGasto(),
					ComboAdministracion = ComboAdministracionesLista(),
					ComboCuentaContable = ComboCuentaPlanContableLista(),
					ListaCuentaFin = ObtenerGridCore<FinancieroListaDto>(cuentasFin)
				};
				return PartialView("_tabDatosCuentaFinYContable", model);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Cuenta Financiera y Contable";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Cuenta Financiera y Contable");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarCuentaFinYContable(string ctafId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctafId))
					return PartialView("_tabDatosCuentaFinYContableSelected", new MedioDePagoAbmCuentaFinSelectedModel());

				var cuentasFin = _medioDePagoServicio.GetCuentaFinYContable(ctafId, TokenCookie);
				var model = new MedioDePagoAbmCuentaFinSelectedModel()
				{
					CuentaFin = ObtenerCuentaFinModel(cuentasFin.First()),
					ComboTipo = ComboFinancierosEstados(),
					ComboCuentaGasto = ComboTipoGasto(),
					ComboAdministracion = ComboAdministracionesLista(),
					ComboCuentaContable = ComboCuentaPlanContableLista()
				};
				return PartialView("_tabDatosCuentaFinYContableSelected", model);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Cuenta Financiera y Contable Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Cuenta Financiera y Contable Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoCuentaFinLista.Count == 0 || actualizar)
				ObtenerTipoCuentaFin(_tipoCuentaFinServicio);
			if (TipoMonedaLista.Count == 0 || actualizar)
				ObtenerTipoMoneda(_tipoMonedaServicio);
			if (FinancierosEstadosLista.Count == 0 || actualizar)
				ObtenerFinancierosEstados(_financieroServicio);
			if (AdministracionesLista.Count == 0 || actualizar)
				ObtenerAdministracionesLista(_administracionServicio);
			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);
			if (CuentaPlanContableLista.Count == 0 || actualizar)
				ObtenerCuentaPlanContableLista(_financieroServicio);
		}
		private void CargarFinancierosRela(string tcf_id)
		{
			if (FinancierosRelaLista == null || FinancierosRelaLista.Count == 0)
				ObteneFinancierosRela(_financieroServicio, tcf_id);
		}
		private SelectList ComboFinancieroRela(string tcf_id)
		{
			CargarFinancierosRela(tcf_id);
			var lista = FinancierosRelaLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_denominacion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private CuentaFinModel ObtenerCuentaFinModel(FinancieroListaDto fin)
		{
			if (fin == null)
				return new CuentaFinModel();
			var cuentaFinModel = new CuentaFinModel()
			{
				Adm_Id = fin.Adm_Id,
				Ccb_Id = fin.Ccb_Id,
				Ccb_Id_Diferido = fin.Ccb_Id_Diferido,
				Ctaf_Activo = fin.ctaf_activo,
				Ctaf_Denominacion = fin.ctaf_denominacion,
				Ctaf_Estado = fin.Ctaf_Estado,
				Ctaf_Estado_Des = fin.Ctaf_Estado_Des,
				Ctaf_Id = fin.ctaf_id,
				Ctaf_Lista = fin.ctaf_lista,
				Ctaf_Saldo = fin.Ctaf_Saldo,
				Ctag_Id = fin.Ctag_Id,
				Cta_Id = fin.Cta_Id,
				Ins_Id = fin.Ins_Id,
				Ins_Desc = fin.Ins_Desc,
				Mon_Codigo = fin.Mon_Codigo,
				Tcf_Id = fin.Tcf_Id,
				Tcf_Desc = fin.Tcf_Desc,
			};
			return cuentaFinModel;
		}
		#endregion

		#region Carga De Listas en sección filtros Base
		[HttpPost]
		public JsonResult BuscarR01(string prefix)
		{
			var tipoNeg = TipoCuentaFinLista.Where(x => x.tcf_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipoNegs = tipoNeg.Select(x => new ComboGenDto { Id = x.tcf_id, Descripcion = x.tcf_lista });
			return Json(tipoNegs);
		}
		#endregion
	}
}
