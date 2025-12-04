using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeStockController : ConsultaDeStockControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IConsultasServicio _consultaServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.REPORTE_STOCK.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		public ConsultaDeStockController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeStockController> logger,
										 IDepositoServicio depositoServicio, IAdministracionServicio administracionServicio,
										 IDocManagerServicio docManager, IOptions<DocsManager> docsManager, ICuentaServicio cuentaServicio,
										 IRubroServicio rubroServicio, IConsultasServicio consultaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_depositoServicio = depositoServicio;
			_administracionServicio = administracionServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: CC_NR_NP
			_docMSv = docManager; //instancio el servicio de impresión
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_consultaServicio = consultaServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE STOCK";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
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

		public async Task<IActionResult> BuscarStockProductos(ConsultarStockRequest request, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new ProductoStkListaModel();
			var lista = new List<ProductoStkDto>();
			MetadataGrid metadata;
			GridCoreSmart<ProductoStkDto> grillaDatos;

			try
			{
				if (!buscaNew)
				{
					lista = ListaProductoStk.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaProductoStk = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					//request.Pagina = pag;

					var res = await _consultaServicio.ConsultarProductoStk(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaProductoStk = lista;

				}
				metadata = MetadataStockProd;
				grillaDatos = GenerarGrillaSmart(ListaProductoStk, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaProductoStk = grillaDatos;
				model.AgrupadoPor = request.agrupador;
				return PartialView("_grillaProductos", model);
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

		public IActionResult ObtenerProveedoresFamilia(string ctaId)
		{
			var model = new ProveedoresFamiliaModel();
			try
			{
				model.ListaFamilias = ComboProveedoresFamilia(ctaId, _cuentaServicio);
				return PartialView("_listaProveedoresFamilia", model);
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

		public IActionResult ObtenerRubros()
		{
			var model = new Models.ListaRubroModel();
			try
			{
				model.ListaRubros = ComboRubros();
				return PartialView("_listaRubros", model);
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

		#region Métodos Privados
		private SelectList ComboRubros()
		{
			var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(ConsultaDeStockModel model)
		{
			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");
			if (RubroLista.Count == 0)
				ObtenerRubros(_rubroServicio);

			var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
			if (depositos != null && depositos.Count > 0)
				model.ListaDepositos = ObtenerListaDepositos(depositos);
			else
				model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);

			model.ListaAgrupadores = ObtenerListaAgrupadores();
			model.ListaFamilias = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.ListaRubros = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var SucursalesList = new List<ComboGenDto>();
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica(SucursalesList);
			var DepositosList = new List<ComboGenDto>();
			ViewBag.DepositosList = HelperMvc<ComboGenDto>.ListaGenerica(DepositosList);
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			var RubrosList = new List<ComboGenDto>();
			ViewBag.RubrosList = HelperMvc<ComboGenDto>.ListaGenerica(RubrosList);
			var FamiliaList = new List<ComboGenDto>();
			ViewBag.FamiliaList = HelperMvc<ComboGenDto>.ListaGenerica(FamiliaList);
		}
		private SelectList ObtenerListaAgrupadores()
		{
			var opciones = new List<SelectListItem>
			{
				new() { Value = "0", Text = "Sin Agrupar" },
				new() { Value = "1", Text = "Por Sector" },
				new() { Value = "2", Text = "Por Grupo de Rubros" },
				new() { Value = "3", Text = "Por Rubros" },
				new() { Value = "4", Text = "Por Proveedor" }
			};

			return new SelectList(opciones, "Value", "Text");

		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
