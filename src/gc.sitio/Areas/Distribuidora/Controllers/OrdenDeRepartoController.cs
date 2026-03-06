using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Distribuidora.Controllers
{
	[Area("Distribuidora")]
	public class OrdenDeRepartoController : OrdenDeRepartoControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IOrdenDeRepartoServicio _ordenDeRepartoServicio;
		private readonly IRepartidorServicio _repartidorServicio;

		public OrdenDeRepartoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDeRepartoController> logger,
										IOrdenDeRepartoServicio ordenDeRepartoServicio, IRepartidorServicio repartidorServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_ordenDeRepartoServicio = ordenDeRepartoServicio;
			_repartidorServicio = repartidorServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroDeORModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "ORDENES DE REPARTO";
				ViewData["Titulo"] = titulo;

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


		/// <summary>
		/// Inicializamos vista principal (Tabs)
		/// </summary>
		/// <returns></returns>
		public IActionResult InicializarView()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				return PartialView("_mainView");
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

		[HttpPost]
		public async Task<IActionResult> BuscarOrdenesDeReparto(QueryFilters filters)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (filters == null)
				{
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));
				}

				if ((filters.Rel01 == null || !filters.Rel01.Any()) &&
					(filters.Rel02 == null || !filters.Rel02.Any()))
				{
					return PartialView("_gridMensaje", CrearRespuestaError("Debe seleccionar algún filtro para buscar las ordenes de reparto"));
				}

				filters.Registros = _setting.NroRegistrosPagina;

				filters.Rel01 = filters.Rel01?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				filters.Rel02 = filters.Rel02?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				//debo realizar la busqueda de los presupuestos
				var ordenes = await _ordenDeRepartoServicio.BuscarOrdenesDeReparto(filters, TokenCookie);

				if (!ordenes.Ok)
				{
					throw new NegocioException(ordenes.Mensaje ?? "Hubo algun problema en la busqueda de Ordenes de Reparto.");
				}

				// Para operar con la lista de OR Seleccionados
				var lista = ordenes.ListaEntidad ?? new List<OrdenDeRepartoListaDto>();
				MetadataGeneral = ordenes.Meta;

				// Generar grid con productos mapeados
				GridCoreSmart<OrdenDeRepartoListaDto> grid = GenerarGridOrdenesDeReparto(lista, filters.Pagina ?? 1, filters);

				return PartialView("_partialOR", grid);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
			}
		}

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroDeORModel model)
		{
			ObtenerEstadoOrdenDeReparto(_ordenDeRepartoServicio);
			if (OrdenDeRepartoEstados != null && OrdenDeRepartoEstados.Count > 0)
				model.ListaEstados = ObtenerListaEstados(OrdenDeRepartoEstados);
			else
				model.ListaEstados = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var repartidores = _repartidorServicio.GetRepartidorLista(TokenCookie);
			if (repartidores != null && repartidores.Count > 0)
				model.ListaRepartidores = ObtenerListaRepartidores(repartidores);
			else
				model.ListaRepartidores = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var EstadosList = new List<ComboGenDto>();
			ViewBag.EstadosList = HelperMvc<ComboGenDto>.ListaGenerica(EstadosList);
			var RepartidoresList = new List<ComboGenDto>();
			ViewBag.RepartidoresList = HelperMvc<ComboGenDto>.ListaGenerica(RepartidoresList);
		}
		private SelectList ObtenerListaEstados(List<OrdenDeRepartoEstadoDto> estadosLista)
		{
			var lista = estadosLista.Select(x => new ComboGenDto { Id = x.ore_id, Descripcion = x.ore_desc});
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaRepartidores(List<RepartidorDto> repartidorLista)
		{
			var lista = repartidorLista.Select(x => new ComboGenDto { Id = x.rp_id, Descripcion = x.rp_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private GridCoreSmart<OrdenDeRepartoListaDto> GenerarGridOrdenesDeReparto(List<OrdenDeRepartoListaDto> lista, int page, QueryFilters filtro)
		{
			var or = lista
				.OrderBy(c => c.or_compte)
				.ToList();

			const int registrosPorPagina = 10;
			var pagedList = new StaticPagedList<OrdenDeRepartoListaDto>(
				or,
				page,
				registrosPorPagina,
				or.Count
			);

			var grid = new GridCoreSmart<OrdenDeRepartoListaDto>
			{
				ListaDatos = pagedList, //lista de combos
				CantidadReg = or.Count, //cantidad actual de registros
				PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
				UltimoRegistro = Math.Min(page * registrosPorPagina, or.Count), //define cual es el ultimo registro
				RegistroFinal = or.Count, //indica cual es el ultimo registro
				CantidadPaginas = (int)Math.Ceiling((double)or.Count / registrosPorPagina),//calcula la cantidad de paginas
				PaginaActual = page,//especifica que pagina es la actual
				Sort = filtro.Sort ?? "or_compte",
				SortDir = filtro.SortDir ?? "ASC",
				DatoAux01 = $"OR cargados: {DateTime.Now:HH:mm:ss}"
			};

			return grid;
		}
		#endregion
	}
}
