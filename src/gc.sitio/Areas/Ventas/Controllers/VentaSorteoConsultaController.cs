using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentaSorteoCarga;
using gc.sitio.Areas.Ventas.Models.VentaSorteoConsulta;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentaSorteoConsultaController : VentaSorteoConsultaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IApiVentasServicio _apiVentasServicio;

		public VentaSorteoConsultaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentaSorteoConsultaController> logger,
											 IApiVentasServicio apiVentasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_apiVentasServicio = apiVentasServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroVtaSorteoCargaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA DE SORTEOS";
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

		[HttpPost]
		public async Task<IActionResult> BuscarSorteoLista(QueryFilters filters)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (filters == null)
				{
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));
				}

				filters.Registros = _setting.NroRegistrosPagina;
				var sorteos = await _apiVentasServicio.BuscarSorteosLista(filters, TokenCookie);

				if (!sorteos.Ok)
				{
					throw new NegocioException(sorteos.Mensaje ?? "Hubo algun problema en la busqueda de Sorteos.");
				}

				// Para operar con la lista de Productos Seleccionados
				var lista = sorteos.ListaEntidad ?? [];
				MetadataGeneral = sorteos.Meta;

				// Generar grid con productos mapeados
				GridCoreSmart<SorteoCargaListaDto> grid = GenerarGridPedidos(lista, filters.Pagina ?? 1, filters);

				return PartialView("_gridSorteos", grid);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al cargar lista de Sorteos"));
			}
		}

		public IActionResult CargarTabsInicial()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				return PartialView("_tabsSorteoDatos");
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error"));
			}
		}

		[HttpPost]
		public async Task<IActionResult> ObtenerSorteoDatos(string so_sorteo)
		{
			var model = new SorteoDatosModel();

			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (string.IsNullOrWhiteSpace(so_sorteo))
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del sorteo no fue recepcionado."));

				// === Obtener datos del sorteo ===
				var sorteo = await _apiVentasServicio.ObtenerSorteoDatos(so_sorteo, TokenCookie);

				if (sorteo?.Ok != true || sorteo.ListaEntidad == null || sorteo.ListaEntidad.Count == 0)
				{
					model.Datos = new SorteoCargaDatosDto();
				}
				else
				{
					model.Datos = sorteo.ListaEntidad.FirstOrDefault() ?? new SorteoCargaDatosDto();
				}

				// === Obtener sucursales ===
				var sucursales = await _apiVentasServicio.ObtenerSorteoAdmDatos(so_sorteo, TokenCookie);

				if (sucursales?.ListaEntidad != null)
					model.Sucursales = ObtenerGridCoreSmart<SorteoCargaAdmDto>(sucursales.ListaEntidad)
									   ?? new GridCoreSmart<SorteoCargaAdmDto>();
				else
					model.Sucursales = new GridCoreSmart<SorteoCargaAdmDto>();

				// === Obtener productos ===
				var productos = await _apiVentasServicio.ObtenerSorteoProdDatos(so_sorteo, TokenCookie);

				if (productos?.ListaEntidad != null)
					model.Productos = ObtenerGridCoreSmart<SorteoCargaProdDto>(productos.ListaEntidad)
									  ?? new GridCoreSmart<SorteoCargaProdDto>();
				else
					model.Productos = new GridCoreSmart<SorteoCargaProdDto>();

				return PartialView("_tabsSorteoDatos_Datos", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error"));
			}
		}


		#region Metodos Privados
		private GridCoreSmart<SorteoCargaListaDto> GenerarGridPedidos(List<SorteoCargaListaDto> lista, int page, QueryFilters filtro)
		{
			var pedidos = lista
				.OrderBy(c => c.so_sorteo)
				.ToList();

			const int registrosPorPagina = 10;
			var pagedList = new StaticPagedList<SorteoCargaListaDto>(
				pedidos,
				page,
				registrosPorPagina,
				pedidos.Count
			);

			var grid = new GridCoreSmart<SorteoCargaListaDto>
			{
				ListaDatos = pagedList, //lista de combos
				CantidadReg = pedidos.Count, //cantidad actual de registros
				PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
				UltimoRegistro = Math.Min(page * registrosPorPagina, pedidos.Count), //define cual es el ultimo registro
				RegistroFinal = pedidos.Count, //indica cual es el ultimo registro
				CantidadPaginas = (int)Math.Ceiling((double)pedidos.Count / registrosPorPagina),//calcula la cantidad de paginas
				PaginaActual = page,//especifica que pagina es la actual
				Sort = filtro.Sort ?? "so_sorteo",
				SortDir = filtro.SortDir ?? "ASC",
				DatoAux01 = $"Sorteos cargados: {DateTime.Now:HH:mm:ss}"
			};

			return grid;
		}

		private void CargarDatosIniciales(FiltroVtaSorteoCargaModel model)
		{
			model.Desde = DateTime.Now.AddDays(-7);
			model.Hasta = DateTime.Now;
		}
		#endregion
	}
}
