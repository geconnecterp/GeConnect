using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ConsVencTipoCtaTipoCompteController : ConsVencTipoCtaTipoCompteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoCanalServicio _tipoCanalServicio;
		private readonly ITipoOpeIvaServicio _tipoOpeIvaServicio;
		private readonly ITipoComprobanteServicio _tipoComprobanteServicio;
		private readonly IConsultasServicio _consultaServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.CV_TC_TC.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		public ConsVencTipoCtaTipoCompteController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsVencTipoCtaTipoCompteController> logger,
														ITipoCanalServicio tipoCanalServicio, ITipoOpeIvaServicio tipoOpeIvaServicio,
														ITipoComprobanteServicio tipoComprobanteServicio, IConsultasServicio consultasServicio,
														IDocManagerServicio docManager, IOptions<DocsManager> docsManager,) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCanalServicio = tipoCanalServicio;
			_tipoOpeIvaServicio = tipoOpeIvaServicio;
			_tipoComprobanteServicio = tipoComprobanteServicio;
			_consultaServicio = consultasServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: ADE
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsVencTipoCtaTipoCompteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA DE VENCIMIENTOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

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

		public async Task<IActionResult> BuscarVencimientos(ConsultarVencimientosRequest request, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new VencimientoListaModel();
			var lista = new List<VencimientoListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<VencimientoListaDto> grillaDatos;

			try
			{
				if (!buscaNew)
				{
					lista = ListaVencimientos.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaVencimientos = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					request.Pagina = pag;

					var res = await _consultaServicio.ConsultarVencimientos(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaVencimientos = lista;

				}
				metadata = MetadataVencimientos;
				grillaDatos = GenerarGrillaSmart(ListaVencimientos, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaVencimientos = grillaDatos;
				return PartialView("_gridVencimientos", model);
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

		#region Métodos Provados
		private void CargarDatosIniciales(ConsVencTipoCtaTipoCompteModel model)
		{
			model.FechaVencDesde = DateTime.Today;
			model.FechaVencHasta = DateTime.Today;
			model.FechaGenDesde = DateTime.Today;
			model.FechaGenHasta = DateTime.Today;

			if (TipoCanalLista.Count == 0)
				ObtenerTiposDeCanal(_tipoCanalServicio);

			model.ListaTipoClientes = ComboTipoCanal();

			if (TipoOpeIvaLista.Count == 0)
				ObtenerTiposOpeIva(_tipoOpeIvaServicio);

			model.ListaTipoProveedores = ComboTipoOpe();

			var listaTemp = _tipoComprobanteServicio.BuscarTipoComprobanteListaPorTipoAfip("%", "CO", Token).Result;
			TiposComprobante = listaTemp;
			if (listaTemp != null && listaTemp.Count > 0)
			{
				var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
				model.ListaTipoCompte = HelperMvc<ComboGenDto>.ListaGenerica(lista);
			}
			else
			{
				model.ListaTipoCompte = HelperMvc<ComboGenDto>.ListaGenerica(new List<ComboGenDto>());
			}

			var tClientesList = new List<ComboGenDto>();
			ViewBag.TipoClientesList = HelperMvc<ComboGenDto>.ListaGenerica(tClientesList);

			var tProvList = new List<ComboGenDto>();
			ViewBag.TipoProveedoresList = HelperMvc<ComboGenDto>.ListaGenerica(tProvList);

			var tComptesList = new List<ComboGenDto>();
			ViewBag.TipoComptesList = HelperMvc<ComboGenDto>.ListaGenerica(tComptesList);
		}
		#endregion
	}
}
