
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ABMs.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class AbmProveedorController : ProveedorControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ILogger<AbmProveedorController> _logger;
		private readonly IProveedorServicio _proveedorServicio;
		private readonly IABMProveedorServicio _abmProvServ;
		private readonly ITipoOpeIvaServicio _tipoOpeServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ICondicionAfipServicio _condicionAfipServicio;
		private readonly INaturalezaJuridicaServicio _naturalezaJuridicaServicio;
		private readonly ICondicionIBServicio _condicionIBServicio;
		private readonly ITipoDocumentoServicio _tipoDocumentoServicio;
		private readonly IDepartamentoServicio _departamentoServicio;
		private readonly ITipoProveedorServicio _tipoProveedorServicio;
		private readonly ITipoGastoServicio _tipoGastoServicio;
		private readonly ITipoRetGanServicio _tipoRetGanServicio;
		private readonly ITipoRetIbServicio _tipoRetIbServicio;
		private readonly IFormaDePagoServicio _formaDePagoServicio;
		private readonly IProvinciaServicio _provinciaServicio;
		private readonly ITipoCuentaBcoServicio _tipoCuentaBcoServicio;
		private readonly ITipoObsServicio _tipoObsServicio;
		private readonly ITipoContactoServicio _tipoContactoServicio;
		private readonly IAbmServicio _abmServicio;
		public AbmProveedorController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<AbmProveedorController> logger,
									  IProveedorServicio proveedorServicio, IABMProveedorServicio abmProvServ, ITipoOpeIvaServicio tipoOpeIvaServicio,
									  ICuentaServicio cuentaServicio, ICondicionAfipServicio condicionAfipServicio, INaturalezaJuridicaServicio naturalezaJuridicaServicio,
									  ICondicionIBServicio condicionIBServicio, ITipoDocumentoServicio tipoDocumentoServicio, IDepartamentoServicio departamentoServicio,
									  ITipoProveedorServicio tipoProveedorServicio, ITipoGastoServicio tipoGastoServicio, ITipoRetGanServicio tipoRetGanServicio,
									  ITipoRetIbServicio tipoRetIbServicio, IFormaDePagoServicio formaDePagoServicio, IProvinciaServicio provinciaServicio,
									  ITipoCuentaBcoServicio tipoCuentaBcoServicio, ITipoObsServicio tipoObsServicio, ITipoContactoServicio tipoContactoServicio,
									  IAbmServicio abmServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_logger = logger;
			_proveedorServicio = proveedorServicio;
			_abmProvServ = abmProvServ;
			_tipoOpeServicio = tipoOpeIvaServicio;
			_cuentaServicio = cuentaServicio;
			_condicionAfipServicio = condicionAfipServicio;
			_naturalezaJuridicaServicio = naturalezaJuridicaServicio;
			_condicionIBServicio = condicionIBServicio;
			_tipoDocumentoServicio = tipoDocumentoServicio;
			_departamentoServicio = departamentoServicio;
			_tipoProveedorServicio = tipoProveedorServicio;
			_tipoGastoServicio = tipoGastoServicio;
			_tipoRetGanServicio = tipoRetGanServicio;
			_tipoRetIbServicio = tipoRetIbServicio;
			_formaDePagoServicio = formaDePagoServicio;
			_provinciaServicio = provinciaServicio;
			_tipoCuentaBcoServicio = tipoCuentaBcoServicio;
			_tipoObsServicio = tipoObsServicio;
			_tipoContactoServicio = tipoContactoServicio;
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

		/// <summary>
		/// Método que llena el Tab "Proveedor" en el ABM
		/// </summary>
		/// <param name="ctaId">Valor de proveedor seleccionado en la grilla principal</param>
		/// <returns>ProveedorAbmModel</returns>
		[HttpPost]
		public async Task<IActionResult> BuscarProveedor(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("_tabDatosProveedor", new ProveedorABMDto());

				var res = _proveedorServicio.GetProveedorParaABM(ctaId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosProveedor", new ProveedorABMDto());

				var cfp = _cuentaServicio.GetCuentaFormaDePago(ctaId, TokenCookie);
				var ccon = _cuentaServicio.GetCuentaContactos(ctaId, TokenCookie);
				var cobs = _cuentaServicio.GetCuentaObs(ctaId, TokenCookie);
				var cnota = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);

				var proveedorModel = new ProveedorAbmModel()
				{
					Proveedor = res.First(),
					ComboAfip = ComboAfip(),
					ComboNatJud = ComboNatJud(),
					ComboTipoDoc = ComboTipoDoc(),
					ComboIngBruto = ComboIB(),
					ComboProvincia = ComboProv(),
					ComboDepartamento = ComboDepto(res.First().Prov_Id.ToString()),
					ComboTipoOpe = ComboTipoOpe(),
					ComboTipoOc = ComboTipoProv(),
					ComboTipoGasto = ComboTipoGasto(),
					ComboTipoRetGan = ComboTipoRetGan(),
					ComboTipoRetIB = ComboTipoRetIb(),
					CuentaFormasDePago = ObtenerGridCore<CuentaFPDto>(cfp),
					CuentaContactos = ObtenerGridCore<CuentaContactoDto>(ccon),
					CuentaObs = ObtenerGridCore<CuentaObsDto>(cobs),
					CuentaNota = ObtenerGridCore<CuentaNotaDto>(cnota)
				};
				return PartialView("_tabDatosProveedor", proveedorModel);
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
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMProveedorSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMProveedorSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaProd == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = ProveedoresBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ProveedoresBuscados = lista;
				}
				//else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				else
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _abmProvServ.BuscarProveedores(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataProveedor = res.Item2 ?? null;
					metadata = MetadataProveedor;
					ProveedoresBuscados = lista;
				}
				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(ProveedoresBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataProveedor.TotalCount, MetadataProveedor.TotalPages, sortDir);

				//string volver = Url.Action("index", "home", new { area = "" });
				//ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

				return View("_gridAbmProveedor", grillaDatos);
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

		[HttpPost]
		public JsonResult ObtenerDatosPaginacion()
		{
			try
			{
				return Json(new { error = false, Metadata = MetadataProveedor });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
			}
		}

		#region Formas de Pago
		/// <summary>
		/// Método que llena el Tab "Formas de Pago" en el ABM
		/// </summary>
		/// <param name="ctaId"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> BuscarFormasDePago(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosFormaDePago", new CuentaAbmFPModel());

				var cfp = _cuentaServicio.GetCuentaFormaDePago(ctaId, TokenCookie);
				var FPModel = new CuentaAbmFPModel()
				{
					ComboTipoCuentaBco = ComboTipoCuentaBco(),
					ComboFormasDePago = ComboFormaDePago(),
					CuentaFormasDePago = ObtenerGridCore<CuentaFPDto>(cfp),
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosFormaDePago", FPModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}


		/// <summary>
		/// Método que trae los campos de la Forma de Pago seleccionada en la grilla del tab Formas de Pago
		/// </summary>
		/// <param name="ctaId">Cuenta seleccionad en grilla principal</param>
		/// <param name="fpId">Forma de pago seleccionada en la grilla de FP del Tab Formas de Pago</param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> BuscarDatosFormasDePago(string ctaId, string fpId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(fpId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosFormaDePagoSelected", new CuentaAbmFPSelectedModel());

				var cfp = _cuentaServicio.GetFormaDePagoPorCuentaYFP(ctaId, fpId, TokenCookie);
				if (cfp == null)
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosFormaDePagoSelected", new CuentaAbmFPSelectedModel());

				var FPSelectedModel = new CuentaAbmFPSelectedModel()
				{
					ComboTipoCuentaBco = ComboTipoCuentaBco(),
					ComboFormasDePago = ComboFormaDePago(),
					FormaDePago = ObtenerFormaDePagoModel(cfp.First())
				};

				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosFormaDePagoSelected", FPSelectedModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago -> FP Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Formas de Pago -> FP Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Otros Contactos
		/// <summary>
		/// Método que llena el Tab "Formas de Pago" en el ABM
		/// </summary>
		/// <param name="ctaId"></param>
		/// <returns></returns>
		[HttpPost]
		public async Task<IActionResult> BuscarOtrosContactos(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosOtroContacto", new CuentaAbmOCModel());

				var coc = _cuentaServicio.GetCuentaContactos(ctaId, TokenCookie);
				var FPModel = new CuentaAbmOCModel()
				{
					ComboTipoContacto = ComboTipoContacto(),
					CuentaOtrosContactos = ObtenerGridCore<CuentaContactoDto>(coc),
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosOtroContacto", FPModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarDatosOtrosContactos(string ctaId, string tcId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(tcId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosOtroContactoSelected", new CuentaAbmOCSelectedModel());
				var coc = _cuentaServicio.GetCuentContactosporCuentaYTC(ctaId, tcId, TokenCookie);
				if (coc == null)
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosOtroContactoSelected", new CuentaAbmOCSelectedModel());
				var model = new CuentaAbmOCSelectedModel()
				{
					ComboTipoContacto = ComboTipoContacto(),
					OtroContacto = ObtenerOtroContactoModel(coc.First())
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosOtroContactoSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> OC Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Notas
		[HttpPost]
		public async Task<IActionResult> BuscarNotas(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosNota", new CuentaABMNotaModel());

				var cno = _cuentaServicio.GetCuentaNota(ctaId, TokenCookie);
				var FPModel = new CuentaABMNotaModel()
				{
					CuentaNotas = ObtenerGridCore<CuentaNotaDto>(cno),
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosNota", FPModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Notas";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Notas");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarDatosNotas(string ctaId, string usuId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(usuId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosNotaSelected", new CuentaABMNotaSelectedModel());
				var cno = _cuentaServicio.GetCuentaNotaDatos(ctaId, usuId, TokenCookie);
				if (cno == null)
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosNotaSelected", new CuentaABMNotaSelectedModel());
				var model = new CuentaABMNotaSelectedModel()
				{
					Nota = ObtenerNotaModel(cno.First())
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosNotaSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Notas Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Observaciones
		[HttpPost]
		public async Task<IActionResult> BuscarObservaciones(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObs", new CuentaABMObservacionesModel());

				var cob = _cuentaServicio.GetCuentaObs(ctaId, TokenCookie);
				if (cob == null)
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObs", new CuentaABMObservacionesModel());

				var ObsModel = new CuentaABMObservacionesModel()
				{
					ComboTipoObs = ComboTipoObservaciones(),
					CuentaObservaciones = ObtenerGridCore<CuentaObsDto>(cob),
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObs", ObsModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Observaciones";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Observaciones");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarDatosObservaciones(string ctaId, string toId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(toId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObsSelected", new CuentaABMObservacionesSelectedModel());
				var cob = _cuentaServicio.GetCuentaObsDatos(ctaId, toId, TokenCookie);
				if (cob == null)
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObsSelected", new CuentaABMObservacionesSelectedModel());
				var model = new CuentaABMObservacionesSelectedModel()
				{
					ComboTipoObs = ComboTipoObservaciones(),
					Observacion = ObtenerObservacionModel(cob.First())
				};
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_tabDatosObsSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Familias
		[HttpPost]
		public async Task<IActionResult> BuscarFamilias(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrEmpty(ctaId))
					return PartialView("_tabDatosFliaProv", new ProveedorABMFliaGrupoModel());

				var pg = _cuentaServicio.ObtenerProveedoresABMFamiliaLista(ctaId, TokenCookie);
				if (pg == null)
					return PartialView("_tabDatosFliaProv", new ProveedorABMFliaGrupoModel());

				var ObsModel = new ProveedorABMFliaGrupoModel()
				{
					ListaProveedorGrupo = ObtenerGridCore<ProveedorGrupoDto>(pg),
				};
				return PartialView("_tabDatosFliaProv", ObsModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Familias";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Familias");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarDatosFamilia(string ctaId, string pgId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(ctaId) || string.IsNullOrWhiteSpace(pgId))
					return PartialView("_tabDatosFliaProvSelected", new ProveedorABMFliaGrupoSelectedModel());

				var pg = _cuentaServicio.ObtenerProveedoresABMFamiliaDatos(ctaId, pgId, TokenCookie);
				if (pg == null)
					return PartialView("_tabDatosFliaProvSelected", new ProveedorABMFliaGrupoSelectedModel());
				if (pg.Count == 0)
					return PartialView("_tabDatosFliaProvSelected", new ProveedorABMFliaGrupoSelectedModel());
				var model = new ProveedorABMFliaGrupoSelectedModel()
				{
					ProveedorGrupo = ObtenerGrupoModel(pg.First()),
				};
				return PartialView("_tabDatosFliaProvSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Otros Contactos -> Obs Selected");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Carga De Listas en sección filtros Base
		[HttpPost]
		public JsonResult BuscarR01(string prefix)
		{
			var tipoNeg = TipoOpeIvaLista.Where(x => x.ope_iva_descripcion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipoNegs = tipoNeg.Select(x => new ComboGenDto { Id = x.ope_iva, Descripcion = x.ope_lista });
			return Json(tipoNegs);
		}
		#endregion

		#region Metodo Para obtener los departamentos desde una seleccion de provincia

		[HttpPost]
		public IActionResult ObtenerDepartamentos(string provId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (string.IsNullOrWhiteSpace(provId))
					return PartialView("~/Areas/ABMs/Views/AbmCliente/_comboLocalidad", ComboDepto());
				return PartialView("~/Areas/ABMs/Views/AbmCliente/_comboLocalidad", ComboDepto(provId));
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda de Departamentos";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda de Departamentos");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Guardado de datos
		#region Proveedor
		/// <summary>
		/// Metodo que engloba las tres operaciones de ABM de Proveedor (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
		/// </summary>
		/// <param name="proveedor">Tipo ProveedorAbmValidationModel</param>
		/// <param name="destinoDeOperacion"></param>
		/// <param name="tipoDeOperacion"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DataOpsProveedor(ProveedorAbmValidationModel proveedor, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(proveedor, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					proveedor = HelperGen.PasarAMayusculas(proveedor);
					var jsonstring = JsonConvert.SerializeObject(proveedor, new JsonSerializerSettings());
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
		public IActionResult NuevoProveedor()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var newObj = new ProveedorABMDto();
				var cfp = new List<CuentaFPDto>();
				var ccon = new List<CuentaContactoDto>();
				var cobs = new List<CuentaObsDto>();
				var cnota = new List<CuentaNotaDto>();
				var ProveedorModel = new ProveedorAbmModel()
				{
					Proveedor = newObj,
					ComboAfip = ComboAfip(),
					ComboNatJud = ComboNatJud(),
					ComboTipoDoc = ComboTipoDoc(),
					ComboIngBruto = ComboIB(),
					ComboProvincia = ComboProv(),
					ComboDepartamento = ComboDepto(),
					ComboTipoOpe = ComboTipoOpe(),
					ComboTipoOc = ComboTipoProv(),
					ComboTipoGasto = ComboTipoGasto(),
					ComboTipoRetGan = ComboTipoRetGan(),
					ComboTipoRetIB = ComboTipoRetIb(),
					CuentaFormasDePago = ObtenerGridCore<CuentaFPDto>(cfp),
					CuentaContactos = ObtenerGridCore<CuentaContactoDto>(ccon),
					CuentaObs = ObtenerGridCore<CuentaObsDto>(cobs),
					CuentaNota = ObtenerGridCore<CuentaNotaDto>(cnota)
				};
				return PartialView("_tabDatosProveedor", ProveedorModel);
			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Alta de Cliente - Nueva entidad";
				_logger.LogError(ex, "Error en la invocación de la API - Alta de Cliente - Nueva entidad");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#region Familia
		/// <summary>
		/// Metodo que engloba las tres operaciones de ABM de Familia de Prod de Proveedor (Alta, baja y modificacion) invocadas al presionar ACEPTAR en la vista
		/// </summary>
		/// <param name="familia">Tipo ProveedorFamiliaAbmValidationModel</param>
		/// <param name="destinoDeOperacion"></param>
		/// <param name="tipoDeOperacion"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult DataOpsProveedorFamilia(ProveedorFamiliaAbmValidationModel familia, string destinoDeOperacion, char tipoDeOperacion)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return Json(new { error = false, warn = true, auth = true, msg = "Su sesión se ha terminado. Debe volver a autenticarse." });
				}

				var respuestaDeValidacion = ValidarJsonAntesDeGuardar(familia, tipoDeOperacion);
				if (respuestaDeValidacion == "")
				{
					familia = HelperGen.PasarAMayusculas(familia);
					var jsonstring = JsonConvert.SerializeObject(familia, new JsonSerializerSettings());
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
		public IActionResult NuevaFamilia()
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var proveedorGrupo = new ProveedorGrupoModel();
				var model = new ProveedorABMFliaGrupoSelectedModel()
				{
					ProveedorGrupo = proveedorGrupo
				};
				return PartialView("_tabDatosFliaProvSelected", model);

			}
			catch (Exception ex)
			{
				string msg = "Error en la invocación de la API - Busqueda datos TAB -> Familia -> Nueva";
				_logger.LogError(ex, "Error en la invocación de la API - Busqueda datos TAB -> Familia -> Nueva");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}
		#endregion

		#endregion

		#region Métodos privados
		private string ValidarJsonAntesDeGuardar(ProveedorAbmValidationModel prov, char abm)
		{
			
			return "";
		}
		private string ValidarJsonAntesDeGuardar(ProveedorFamiliaAbmValidationModel cuenta, char abm)
		{
			return "";
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoOpeIvaLista.Count == 0 || actualizar)
				ObtenerTiposOpeIva(_tipoOpeServicio);

			if (CondicionesAfipLista.Count == 0 || actualizar)
				ObtenerCondicionesAfip(_condicionAfipServicio);

			if (NaturalezaJuridicaLista.Count == 0 || actualizar)
				ObtenerNaturalezaJuridica(_naturalezaJuridicaServicio);

			if (CondicionIBLista.Count == 0 || actualizar)
				ObtenerCondicionesIB(_condicionIBServicio);

			if (TipoDocumentoLista.Count == 0 || actualizar)
				ObtenerTiposDocumento(_tipoDocumentoServicio);

			if (TipoProvLista.Count == 0 || actualizar)
				ObtenerTiposProveedor(_tipoProveedorServicio);

			if (TipoGastoLista.Count == 0 || actualizar)
				ObtenerTipoGastos(_tipoGastoServicio);

			if (TipoRetGanLista.Count == 0 || actualizar)
				ObtenerTipoRetGan(_tipoRetGanServicio);

			if (TipoRetIBLista.Count == 0 || actualizar)
				ObtenerTipoRetIB(_tipoRetIbServicio);

			if (FormaDePagoLista.Count == 0 || actualizar)
				ObtenerFormasDePago(_formaDePagoServicio);

			if (ProvinciaLista.Count == 0 || actualizar)
				ObtenerProvincias(_provinciaServicio);

			if (TipoCuentaBcoLista.Count == 0 || actualizar)
				ObtenerTiposDeCuentaBco(_tipoCuentaBcoServicio);

			if (TipoObservacionesLista.Count == 0 || actualizar)
				ObtenerTipoObservaciones(_tipoObsServicio, "P");

			if (TipoContactoLista.Count == 0 || actualizar)
				ObtenerTipoContacto(_tipoContactoServicio, "P");
		}
		private static ObservacionesModel ObtenerObservacionModel(CuentaObsDto obs)
		{
			var mod = new ObservacionesModel();
			if (obs == null)
				return mod;

			#region map
			mod.To_Lista = obs.to_lista;
			mod.To_Id = obs.to_id;
			mod.To_Desc = obs.to_desc;
			mod.Cta_Obs = obs.cta_obs;
			mod.Cta_Id = obs.cta_id;
			#endregion
			return mod;
		}
		private static NotaModel ObtenerNotaModel(CuentaNotaDto nota)
		{
			var nom = new NotaModel();
			if (nota == null)
				return nom;

			#region map
			nom.Usu_Lista = nota.usu_lista;
			nom.Usu_Id = nota.usu_id;
			nom.Usu_Apellidoynombre = nota.usu_apellidoynombre;
			nom.Nota = nota.nota;
			nom.Fecha = nota.fecha;
			nom.Cta_Id = nota.cta_id;
			#endregion
			return nom;
		}

		private static ProveedorGrupoModel ObtenerGrupoModel(ProveedorGrupoDto pg)
		{
			var pgr = new ProveedorGrupoModel();
			if (pg == null)
				return pgr;
			#region map
			pgr.Pg_Desc = pg.pg_desc;
			pgr.Pg_Lista = pg.pg_lista;
			pgr.Pg_Actu = pg.pg_actu;
			pgr.Pg_Fecha_Carga_Precios = pg.pg_fecha_carga_precios;
			pgr.Pg_Fecha_Consulta_Precios = pg.pg_fecha_consulta_precios;
			pgr.Cta_Id = pg.cta_id;
			pgr.Pg_Actu_Fecha = pg.pg_actu_fecha;
			pgr.Pg_Observaciones = pg.pg_observaciones;
			pgr.Pg_Id = pg.pg_id;
			pgr.Pg_Fecha_Cambio_Precios = pg.pg_fecha_cambio_precios;
			#endregion
			return pgr;
		}

		private static OtroContactoModel ObtenerOtroContactoModel(CuentaContactoDto contacto)
		{
			var ocm = new OtroContactoModel();
			if (contacto == null)
				return ocm;
			#region mapp
			ocm.Tc_Lista = contacto.tc_lista;
			ocm.Tc_Id = contacto.tc_id;
			ocm.Tc_Desc = contacto.tc_desc;
			ocm.Cta_Te = contacto.cta_te;
			ocm.Cta_Nombre = contacto.cta_nombre;
			ocm.Cta_Id = contacto.cta_id;
			ocm.Cta_Email = contacto.cta_email;
			ocm.Cta_Celu = contacto.cta_celu;
			#endregion
			return ocm;
		}

		private static FormaDePagoModel ObtenerFormaDePagoModel(CuentaFPDto fp)
		{
			var fpm = new FormaDePagoModel();
			if (fp == null)
				return fpm;
			#region mapp
			fpm.Fp_Deufault = fp.fp_deufault;
			fpm.Fp_Lista = fp.fp_lista;
			fpm.Fp_Desc = fp.fp_desc;
			fpm.Fp_Id = fp.fp_id;
			fpm.Cta_Valores_A_Nombre = fp.cta_valores_a_nombre;
			fpm.Tcb_Lista = fp.tcb_lista;
			fpm.Cta_Bco_Cuenta_Cbu = fp.cta_bco_cuenta_cbu;
			fpm.Cta_Bco_Cuenta_Nro = fp.cta_bco_cuenta_nro;
			fpm.Cta_Obs = fp.cta_obs;
			fpm.Tcb_Id = fp.tcb_id;
			fpm.Tcb_Desc = fp.tcb_desc;
			fpm.Fp_Dias = fp.fp_dias;
			fpm.Cta_Id = fp.cta_id;
			#endregion
			return fpm;
		}
		protected SelectList ComboDepto(string prov_id)
		{
			CargarDepartametos(prov_id);
			var lista = DepartamentoLista.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboDepto()
		{
			var nuevaListaDpto = new List<DepartamentoDto>();
			var lista = nuevaListaDpto.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDepartametos(string prov_id)
		{
			if (DepartamentoLista == null || DepartamentoLista.Count == 0)
				ObtenerDepartamentos(_departamentoServicio, prov_id);
			else if (DepartamentoLista.First().prov_id != prov_id)
				ObtenerDepartamentos(_departamentoServicio, prov_id);
		}
		#endregion
	}
}
