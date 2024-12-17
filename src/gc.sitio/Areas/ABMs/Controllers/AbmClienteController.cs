using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ABMs.Controllers
{
	[Area("ABMs")]
	public class AbmClienteController : ClienteControladorBase
	{
		private readonly AppSettings _settings;
		private readonly IABMClienteServicio _abmCliServ;
		private readonly ITipoNegocioServicio _tipoNegocioServicio;
		private readonly IZonaServicio _zonaServicio;
		private readonly ICondicionAfipServicio _condicionAfipServicio;
		private readonly INaturalezaJuridicaServicio _naturalezaJuridicaServicio;
		private readonly ICondicionIBServicio _condicionIBServicio;
		private readonly IFormaDePagoServicio _formaDePagoServicio;
		private readonly IProvinciaServicio _provinciaServicio;
		private readonly ITipoCanalServicio _tipoCanalServicio;
		private readonly ITipoCuentaBcoServicio _tipoCuentaBcoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoDocumentoServicio _tipoDocumentoServicio;
		private readonly IDepartamentoServicio _departamentoServicio;
		private readonly IListaDePrecioServicio _listaDePrecioServicio;
		private readonly IVendedorServicio _vendedorServicio;
		private readonly IRepartidorServicio _repartidorServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ILogger<AbmClienteController> _logger;

		public AbmClienteController(IZonaServicio zonaServicio, ITipoNegocioServicio tipoNegocioServicio, IOptions<AppSettings> options,
									IABMClienteServicio abmClienteServicio, IHttpContextAccessor accessor,
									ICondicionAfipServicio condicionAfipServicio, INaturalezaJuridicaServicio naturalezaJuridicaServicio,
									ICondicionIBServicio condicionIBServicio, IFormaDePagoServicio formaDePagoServicio,
									IProvinciaServicio provinciaServicio, ITipoCanalServicio tipoCanalServicio,
									ITipoCuentaBcoServicio tipoCuentaBcoServicio, ICuentaServicio cuentaServicio,
									ITipoDocumentoServicio tipoDocumentoServicio, ILogger<AbmClienteController> logger,
									IDepartamentoServicio departamentoServicio, IListaDePrecioServicio listaDePrecioServicio,
									IVendedorServicio vendedorServicio, IRepartidorServicio repartidorServicio,
									IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_tipoNegocioServicio = tipoNegocioServicio;
			_zonaServicio = zonaServicio;
			_abmCliServ = abmClienteServicio;
			_condicionAfipServicio = condicionAfipServicio;
			_naturalezaJuridicaServicio = naturalezaJuridicaServicio;
			_condicionIBServicio = condicionIBServicio;
			_formaDePagoServicio = formaDePagoServicio;
			_provinciaServicio = provinciaServicio;
			_tipoCanalServicio = tipoCanalServicio;
			_tipoCuentaBcoServicio = tipoCuentaBcoServicio;
			_cuentaServicio = cuentaServicio;
			_tipoDocumentoServicio = tipoDocumentoServicio;
			_departamentoServicio = departamentoServicio;
			_listaDePrecioServicio = listaDePrecioServicio;
			_vendedorServicio = vendedorServicio;
			_repartidorServicio = repartidorServicio;
			_financieroServicio = financieroServicio;
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

			var listR02 = new List<ComboGenDto>();
			ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

			var listR01 = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Buscar(QueryFilters query, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			List<ABMClienteSearchDto> lista;
			MetadataGrid metadata;
			GridCore<ABMClienteSearchDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (PaginaProd == pag && !buscaNew)
				{
					//es la misma pagina y hay registros, se realiza el reordenamiento de los datos.
					lista = ClientesBuscados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ClientesBuscados = lista;
				}
				else if (PaginaProd != pag)  //&& PaginaProd >= 0 && !query.Todo
				{
					//traemos datos desde la base
					query.Sort = sort;
					query.SortDir = sortDir;
					query.Registros = _settings.NroRegistrosPagina;
					query.Pagina = pag;

					var res = await _abmCliServ.BuscarClientes(query, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataCliente = res.Item2 ?? null;
					metadata = MetadataCliente;
					ClientesBuscados = lista;
				}
				//no deberia estar nunca la metadata en null.. si eso pasa podria haber una perdida de sesion o algun mal funcionamiento logico.
				grillaDatos = GenerarGrilla(ClientesBuscados, sort, _settings.NroRegistrosPagina, pag, MetadataCliente.TotalCount, MetadataCliente.TotalPages, sortDir);

				//string volver = Url.Action("index", "home", new { area = "" });
				//ViewBag.AppItem = new AppItem { Nombre = "Cargas Previas - Impresión de Etiquetas", VolverUrl = volver ?? "#" };

				return View("_gridAbmCliente", grillaDatos);
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
				return Json(new { error = false, Metadata = MetadataCliente });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, msg = "No se pudo obtener la información de paginación. Verifica" });
			}
		}

		[HttpPost]
		public async Task<IActionResult> BuscarCliente(string ctaId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var res = _cuentaServicio.GetCuentaParaABM(ctaId, TokenCookie);
				if (res == null)
					return PartialView("_tabDatosCliente", new CuentaABMDto());

				var ClienteModel = new CuentaAbmModel()
				{
					Cliente = res.First(),
					ComboAfip = ComboAfip(),
					ComboNatJud = ComboNatJud(),
					ComboTipoDoc = ComboTipoDoc(),
					ComboIngBruto = ComboIB(),
					ComboProvincia = ComboProv(),
					ComboDepartamento = ComboDepto(res.First().Prov_Id.ToString()),
					ComboTipoCuentaBco = ComboTipoCuentaBco(),
					ComboTipoNegocio = ComboTipoNegocio(),
					ComboListaDePrecios = ComboListaDePrecios(),
					ComboTipoCanal = ComboTipoCanal(),
					ComboVendedores = ComboVendedores(),
					ComboDiasDeLaSemana = ComboDiasDeLaSemana(),
					ComboZonas = ComboZonas(),
					ComboRepartidores = ComboRepartidores(),
					ComboFinancieros = ComboFinanciero("BA")
				};
				return PartialView("_tabDatosCliente", ClienteModel);
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
		public JsonResult BuscarR01(string prefix)
		{
			var tipoNeg = TipoNegocioLista.Where(x => x.ctn_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipoNegs = tipoNeg.Select(x => new ComboGenDto { Id = x.ctn_id, Descripcion = x.ctn_lista });
			return Json(tipoNegs);
		}

		[HttpPost]
		public JsonResult BuscarR02(string prefix)
		{
			var zona = ZonasLista.Where(x => x.zn_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var zonas = zona.Select(x => new ComboGenDto { Id = x.zn_id, Descripcion = x.zn_lista });
			return Json(zonas);
		}

		#region Métodos Privados
		protected SelectList ComboDepto(string prov_id)
		{
			CargarDepartametos(prov_id);
			var lista = DepartamentoLista.Select(x => new ComboGenDto { Id = x.dep_id, Descripcion = x.dep_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboFinanciero(string tcf_id)
		{
			CargarFinancieros(tcf_id);
			var lista = FinancierosLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = x.ctaf_denominacion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDepartametos(string prov_id)
		{
			if (DepartamentoLista == null || DepartamentoLista.Count == 0)
				ObtenerDepartamentos(_departamentoServicio, prov_id);
			else if (DepartamentoLista.First().prov_id != prov_id)
				ObtenerDepartamentos(_departamentoServicio, prov_id);
		}
		private void CargarFinancieros(string tcf_id)
		{
			if (FinancierosLista == null || FinancierosLista.Count == 0)
				ObteneFinancieros(_financieroServicio, tcf_id);
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (TipoNegocioLista.Count == 0 || actualizar)
				ObtenerTiposNegocio(_tipoNegocioServicio);

			if (ZonasLista.Count == 0 || actualizar)
				ObtenerZonas(_zonaServicio);

			if (CondicionesAfipLista.Count == 0 || actualizar)
				ObtenerCondicionesAfip(_condicionAfipServicio);

			if (NaturalezaJuridicaLista.Count == 0 || actualizar)
				ObtenerNaturalezaJuridica(_naturalezaJuridicaServicio);

			if (CondicionIBLista.Count == 0 || actualizar)
				ObtenerCondicionesIB(_condicionIBServicio);

			if (FormaDePagoLista.Count == 0 || actualizar)
				ObtenerFormasDePago(_formaDePagoServicio);

			if (ProvinciaLista.Count == 0 || actualizar)
				ObtenerProvincias(_provinciaServicio);

			if (TipoCanalLista.Count == 0 || actualizar)
				ObtenerTiposDeCanal(_tipoCanalServicio);

			if (TipoCuentaBcoLista.Count == 0 || actualizar)
				ObtenerTiposDeCuentaBco(_tipoCuentaBcoServicio);

			if (TipoDocumentoLista.Count == 0 || actualizar)
				ObtenerTiposDocumento(_tipoDocumentoServicio);

			if (ListaDePreciosLista.Count == 0 || actualizar)
				ObtenerListaDePrecios(_listaDePrecioServicio);

			if (VendedoresLista.Count == 0 || actualizar)
				ObtenerListaDeVendedores(_vendedorServicio);

			if (DiasDeLaSemanaLista.Count == 0 || actualizar)
				ObtenerDiasDeLaSemana();

			if (RepartidoresLista.Count == 0 || actualizar)
				ObtenerListaDeRepartidores(_repartidorServicio);
		}
		#endregion
	}
}
