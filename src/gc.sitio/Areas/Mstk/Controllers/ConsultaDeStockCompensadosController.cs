using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeStockCompensadosController : ConsultaDeStockCompensadosControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IConsultasServicio _consultaServicio;
		private readonly IProductoServicio _productoServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.REPORTE_STOCK_COMPENSADO.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		public ConsultaDeStockCompensadosController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeStockCompensadosController> logger,
													ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IConsultasServicio consultaServicio, IProductoServicio productoServicio,
													IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_consultaServicio = consultaServicio;
			_productoServicio = productoServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: CC_NR_NP
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeStockCompensadosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE PRODUCTOS CON STOCK COMPENSADOS";
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

		public async Task<IActionResult> BuscarStockProductos(ConsultarStockCompensadoRequest request, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new ProductoStkCompensadoListaModel();
			var lista = new List<ProductoStkCompensadoDto>();
			MetadataGrid metadata;
			GridCoreSmart<ProductoStkCompensadoDto> grillaDatos;

			try
			{
				if (!buscaNew)
				{
					lista = ListaProductoStkCompensados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaProductoStkCompensados = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					//request.Pagina = pag;

					var res = await _consultaServicio.ConsultarProductoStkCompensado(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaProductoStkCompensados = lista;

				}
				metadata = MetadataStockProdCompensados;
				grillaDatos = GenerarGrillaSmart(ListaProductoStkCompensados, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaProductoStkComp = grillaDatos;

				// Construcción de leyenda
				model.LeyendaProv = ConstruirLeyenda("Proveedores", request.lProv, request.lProvTextos);
				model.LeyendaRub = ConstruirLeyenda("Rubros", request.lRub, request.lRubTextos);
				model.LeyendaEstado = ConstruirLeyenda("Estado", request.chkEstadoTextos);

				// Leyenda final
				var partesLeyenda = new List<string>();

				if (!string.IsNullOrWhiteSpace(model.LeyendaProv))
					partesLeyenda.Add(model.LeyendaProv);

				if (!string.IsNullOrWhiteSpace(model.LeyendaRub))
					partesLeyenda.Add(model.LeyendaRub);

				if (!string.IsNullOrWhiteSpace(model.LeyendaEstado))
					partesLeyenda.Add(model.LeyendaEstado);

				model.Leyenda = string.Join(" | ", partesLeyenda);

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

		public async Task<IActionResult> BuscarInfoProdStkDeposito(string pId, string admId)
		{
			var model = new InfoAdicionalModel();
			try
			{
				var info = await _productoServicio.InfoProductoStkD(pId, AdministracionId, TokenCookie);
				model.GrillaProdStkD = ObtenerGridCoreSmart<InfoProdStkD>(info);
				return PartialView("_infoProdPorDeposito", model);
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
		private static string ConstruirLeyenda(string titulo, List<string>? lista, string textos)
		{
			if (lista == null || lista.Count == 0)
				return string.Empty;

			// Caso especial: único valor "%"
			if (lista.Count == 1 && lista[0] == "%")
				return $"{titulo}: Todos";

			// Caso normal
			if (!string.IsNullOrWhiteSpace(textos))
				return $"{titulo}: {textos}";

			return string.Empty;
		}
		private static string ConstruirLeyenda(string titulo, string textos)
		{
			// Caso normal
			if (!string.IsNullOrWhiteSpace(textos))
				return $"{titulo}: {textos}";

			return string.Empty;
		}
		private SelectList ComboRubros()
		{
			var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(ConsultaDeStockCompensadosModel model)
		{
			try
			{
				if (ProveedoresLista.Count == 0)
					ObtenerProveedores(_cuentaServicio, "BI");
				if (RubroLista.Count == 0)
					ObtenerRubros(_rubroServicio);

				model.ListaRubros = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var RubrosList = new List<ComboGenDto>();
				ViewBag.RubrosList = HelperMvc<ComboGenDto>.ListaGenerica(RubrosList);
				var Rel01List = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			}
			catch (Exception ex)
			{
				throw new Exception($"Error al cargar los datos iniciales: {ex.Message}");
			}
		}
		#endregion
	}
}
