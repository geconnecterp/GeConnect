using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentaSorteoCarga;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentaSorteoCargaController : VentaSorteoCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IRubroServicio _rubroServicio;
		private readonly IApiVentasServicio _apiVentasServicio;
		private readonly ICuentaServicio _cuentaServicio;
		public VentaSorteoCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentaSorteoCargaController> logger,
										  IRubroServicio rubroServicio, IApiVentasServicio apiVentasServicio, ICuentaServicio cuentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_rubroServicio = rubroServicio;
			_apiVentasServicio = apiVentasServicio;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroVtaSorteoCargaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "SORTEOS";
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
				var lista = sorteos.ListaEntidad ?? new List<SorteoCargaListaDto>();
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
				return PartialView("_gridMensaje", CrearRespuestaError("Error al agregar productos a ofertas"));
			}
		}

		[HttpPost]
		public async Task<IActionResult> ObtenerSorteoDatos(string so_sorteo)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (so_sorteo == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del sorteo no fue recepcionado."));

				var sorteo = await _apiVentasServicio.ObtenerSorteoDatos(so_sorteo, TokenCookie);
				if (!sorteo.Ok)
					throw new NegocioException(sorteo.Mensaje ?? "No se ha podido sorteo.");

				if (sorteo.ListaEntidad == null || sorteo.ListaEntidad.Count() == 0)
					throw new NegocioException("No se encontraron los datos del Sorteo");

				sorteo.ListaEntidad.ForEach(x => x.modo_lectura = true);
				return PartialView("_sorteoDatos", sorteo.ListaEntidad[0]);
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

		public IActionResult ObtenerSorteoTablas()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				return PartialView("_sorteoTablas");
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
		public async Task<IActionResult> ObtenerSorteoTablasSucursales(string so_sorteo)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (so_sorteo == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del sorteo no fue recepcionado."));

				var sorteoAdms = await _apiVentasServicio.ObtenerSorteoAdmDatos(so_sorteo, TokenCookie);
				if (!sorteoAdms.Ok)
					throw new NegocioException(sorteoAdms.Mensaje ?? "No se ha podido obtener la lista de sucursales del sorteo.");

				if (sorteoAdms.ListaEntidad == null || sorteoAdms.ListaEntidad.Count() == 0)
					throw new NegocioException("No se encontraron los datos de sucursales del Sorteo");

				return PartialView("_sorteoTablas_adm", ObtenerGridCoreSmart<SorteoCargaAdmDto>(sorteoAdms.ListaEntidad));
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
		public async Task<IActionResult> ObtenerSorteoTablasProductos(string so_sorteo)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (so_sorteo == null)
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del sorteo no fue recepcionado."));

				var sorteoProds = await _apiVentasServicio.ObtenerSorteoProdDatos(so_sorteo, TokenCookie);
				if (!sorteoProds.Ok)
					throw new NegocioException(sorteoProds.Mensaje ?? "No se ha podido obtener la lista de productos del sorteo.");
				if (sorteoProds.ListaEntidad == null || sorteoProds.ListaEntidad.Count() == 0)
					throw new NegocioException("No se encontraron los datos de productos del Sorteo");

				return PartialView("_sorteoTablas_prod", ObtenerGridCoreSmart<SorteoCargaProdDto>(sorteoProds.ListaEntidad));
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

			#region Carga de Rubros
			if (RubroLista.Count == 0)
			{
				ObtenerRubros(_rubroServicio);
			}
			var rubs = RubroLista
				.Select(r => new ComboGenDto
				{
					Id = r.Rub_Id,
					Descripcion = r.Rub_Id + " - " + r.Rub_Desc
				})
				.ToList();
			ViewBag.Rel02B2 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);
			#endregion


			if (CuentasLista.Count == 0)
				ObtenerCuentas(_cuentaServicio, 'D', "%");

			var listR03 = new List<ComboGenDto>();
			ViewBag.Rel03B2 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
		}
		#endregion
	}
}
