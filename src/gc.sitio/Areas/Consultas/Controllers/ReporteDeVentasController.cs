using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Consultas.Models.ReporteDeVentas;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ReporteDeVentasController : ReporteDeVentasControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IApiVentasServicio _apiVentaServicio;

		public ReporteDeVentasController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ReporteDeVentasController> logger,
										 IAdministracionServicio administracionServicio, IApiVentasServicio apiVentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_apiVentaServicio = apiVentaServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroReporteDeVentasModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE VENTAS";
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
		public IActionResult InicializarPantallPrincipal(string sucursalesText, DateTime desde, DateTime hasta)
		{
			var model = new PrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Titulo = $"Sucursales - {sucursalesText} - Desde: {desde:dd/MM/yyyy} Hasta: {hasta:dd/MM/yyyy}";
				return PartialView("_pantallaPrincipal", model);
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
		public async Task<IActionResult> BuscarProcesosDeCaja(DateTime Desde, DateTime Hasta, string Sucursales, int Pagina, int Registros)
		{
			var model = new ProcesosDeCajaModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (string.IsNullOrEmpty(Sucursales))
					return PartialView("_gridMensaje", CrearRespuestaError("El filtro de busqueda no fue recepcionado."));

				var request = new CajaProcesoListaRequest
				{
					Desde = Desde,
					Hasta = Hasta,
					adm_list = Sucursales,
					Pagina = Pagina,
					Registros = Registros
				};

				request.Registros = _setting.NroRegistrosPagina;
				var lista = await _apiVentaServicio.ObtenerCajaProcesoLista(request, TokenCookie);

				if (!lista.Ok)
				{
					throw new NegocioException(lista.Mensaje ?? "Hubo algun problema en la busqueda de Procesos de Caja.");
				}

				// Para operar con la lista de Productos Seleccionados
				var procesos = lista.ListaEntidad ?? new List<CajaProcesoListaDto>();
				MetadataGeneral = lista.Meta;

				// Generar grid con productos mapeados
				model.ListaProcesos = GenerarGridProcesos(procesos, request.Pagina, request);
				model.ListaCierres = ObtenerGridCoreSmart<CajaProcesoListaDto>(new List<CajaProcesoListaDto>());

				return PartialView("_partialProcDeCaja", model);
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
		private GridCoreSmart<CajaProcesoListaDto> GenerarGridProcesos(List<CajaProcesoListaDto> lista, int page, CajaProcesoListaRequest filtro)
		{
			var pedidos = lista
				.OrderBy(c => c.caja_nro_proceso)
				.ToList();

			const int registrosPorPagina = 10;
			var pagedList = new StaticPagedList<CajaProcesoListaDto>(
				pedidos,
				page,
				registrosPorPagina,
				pedidos.Count
			);

			var grid = new GridCoreSmart<CajaProcesoListaDto>
			{
				ListaDatos = pagedList, //lista de combos
				CantidadReg = pedidos.Count, //cantidad actual de registros
				PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
				UltimoRegistro = Math.Min(page * registrosPorPagina, pedidos.Count), //define cual es el ultimo registro
				RegistroFinal = pedidos.Count, //indica cual es el ultimo registro
				CantidadPaginas = (int)Math.Ceiling((double)pedidos.Count / registrosPorPagina),//calcula la cantidad de paginas
				PaginaActual = page,//especifica que pagina es la actual
				Sort = filtro.Sort ?? "caja_nro_proceso",
				SortDir = filtro.SortDir ?? "ASC",
				DatoAux01 = $"Sorteos cargados: {DateTime.Now:HH:mm:ss}"
			};

			return grid;
		}
		private void CargarDatosIniciales(FiltroReporteDeVentasModel model)
		{
			var hoy = DateTime.Today;
			model.Hasta = hoy;
			model.Desde = hoy.AddDays(-7);

			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursales = ObtenerLista(sucursales);
				var suc = sucursales.Where(x => x.Adm_id == AdministracionId).FirstOrDefault();
				if (suc != null && suc.Adm_central == 'S')
					model.HabilitarCambioDeSucursalSeleccionada = true;
				else
					model.HabilitarCambioDeSucursalSeleccionada = false;
				model.SucursalSeleccionada = AdministracionId;
			}
			else
			{
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.HabilitarCambioDeSucursalSeleccionada = false;
			}
			var tImpuestosList = new List<ComboGenDto>();
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
