using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.DocManager;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Tipos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models.PedidoInternoConsulta;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Implementacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using X.PagedList;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class PedidoInternoConsultaController : PedidoInternoConsultaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IProductoServicio _productoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IPedidoInternoEstadoServicio _pedidoInternoEstadoServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1;
		private AppModulo _modulo_2;
		private string APP_MODULO_1 = AppModulos.PEDIDO_INTERNO.ToString();
		private string APP_MODULO_2 = AppModulos.PEDIDO_INTERNO_LISTA.ToString();
		private readonly IDocManagerServicio _docMSv;
		public PedidoInternoConsultaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<PedidoInternoConsultaController> logger,
											   IProductoServicio productoServicio, IAdministracionServicio administracionServicio,
											   IPedidoInternoEstadoServicio pedidoInternoEstadoServicio, IDocManagerServicio docManager, 
											   IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_productoServicio = productoServicio;
			_administracionServicio = administracionServicio;
			_pedidoInternoEstadoServicio = pedidoInternoEstadoServicio;

			// PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA DE PEDIDOS INTERNOS";
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
		public async Task<IActionResult> BuscarPedidosInternos(QueryFilters filters)
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
					return PartialView("_gridMensaje", CrearRespuestaError("Debe seleccionar algún filtro para buscar los pedidos internos"));
				}

				filters.Registros = _setting.NroRegistrosPagina;

				filters.Rel01 = filters.Rel01?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				filters.Rel02 = filters.Rel02?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				filters.Adm_id = AdministracionId;
				filters.Usu_id = UserName;
				//debo realizar la busqueda de los presupuestos
				var pedidos = await _productoServicio.PedidosInternosLista(filters, TokenCookie);

				if (!pedidos.Ok)
					throw new NegocioException(pedidos.Mensaje ?? "Hubo algun problema en la busqueda de Ordenes de Reparto.");

				// Para operar con la lista de OR Seleccionados
				var lista = pedidos.ListaEntidad ?? [];
				MetadataGeneral = pedidos.Meta;
				PedidosInternosLista = lista;

				// Generar grid con productos mapeados
				GridCoreSmart<PedidoInternoListaDto> grid = GenerarGridOrdenesDeReparto(lista, filters.Pagina ?? 1, filters);

				return PartialView("_partial_pedidos_internos", grid);
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
		public JsonResult CambiarEstadoPedidoInterno(PedidoInternoCambiarEstadoRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(request.PiCompte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Pedido Interno." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;

				var respuesta = _productoServicio.CambiarEstadoPedidoInterno(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Cambios de estado en pedido interno realizado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Cambios de estado en pedido interno: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en cambios de estado en pedido interno"
					});
				}
			}
			catch (NegocioException ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, ex.Message);
				return Json(new
				{
					ok = false,
					error = true,
					mensaje = ex.Message
				});
			}
		}

		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.PedidoInterno:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Imprimir Pedido Interno";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.PedidoInternoLista:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Imprimir Listado de Pedidos Internos";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					default:
						break;
				}

				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente.", adm_id = AdministracionId, usu_id = UserName });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		public IActionResult DetallePedidoInterno(string pi_compte)
		{
			var model = new PedidoInternoDetalleModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(pi_compte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se ha recibido un identificador de Pedido Interno válido."));
				
				var pedido = _productoServicio.PIDetalle(pi_compte, TokenCookie).Result;
				if (pedido == null)
					return PartialView("_gridMensaje", CrearRespuestaError("No se encontró el Pedido Interno solicitado."));

				model.DetalleDePedidoInterno = ObtenerGridCoreSmart<PIDetalleDto>(pedido);
				model.Leyenda = pi_compte;
				return PartialView("_partial_pedido_interno_detalle", model);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al obtener detalle de Pedido Interno");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener detalle de Pedido Interno"));
			}
		}

		/* Se deshabilitan los tab de RTR para en el futuro agregar el desarrollo*/
		//public IActionResult DetalleRTRPedidoInterno(string pi_compte)
		//{
		//	var model = new PedidoInternoRTRDetalleModel();
		//	try
		//	{
		//		if (!VerificarAutenticacion(out IActionResult redirectResult))
		//			return redirectResult;
		//		if (string.IsNullOrEmpty(pi_compte))
		//			return PartialView("_gridMensaje", CrearRespuestaError("No se ha recibido un identificador de Pedido Interno válido."));

		//		var pedido = _productoServicio.PIDetalle(pi_compte, TokenCookie).Result;
		//		if (pedido == null)
		//			return PartialView("_gridMensaje", CrearRespuestaError("No se encontró el Pedido Interno solicitado."));

		//		model.DetalleRTR = ObtenerGridCoreSmart<PIDetalleDto>(pedido);
		//		return PartialView("_partial_pedido_interno_rtr", model);
		//	}
		//	catch (Exception ex)
		//	{
		//		_logger?.LogError(ex, "Error al obtener detalle de RTR");
		//		return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener detalle de RTR"));
		//	}
		//}

		#region Métodos Privados
		enum TipoDeReporte
		{
			PedidoInterno = 1,
			PedidoInternoLista = 2,
		}
		private void CargarDatosIniciales(FiltrosModel model)
		{
			model.FechaDesde = DateTime.Now.Date.AddMonths(-1);
			model.FechaHasta = DateTime.Now.Date;
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", Token);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerSucursales(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.SucursalSeleccionada = AdministracionId;
			var estados = _pedidoInternoEstadoServicio.GetPedidoInternoEstados(TokenCookie);
			if (estados != null && estados.Count > 0)
				model.ListaEstados = ObtenerEstadosDePI(estados);
			else
				model.ListaEstados = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var SucursalesList = new List<ComboGenDto>();
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica(SucursalesList);
			var EstadosList = new List<ComboGenDto>();
			ViewBag.EstadosList = HelperMvc<ComboGenDto>.ListaGenerica(EstadosList);
		}

		private SelectList ObtenerSucursales(List<AdministracionDto> administraciones)
		{
			var lista = administraciones.Select(a => new ComboGenDto
			{
				Id = a.Adm_id,
				Descripcion = a.Adm_nombre
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerEstadosDePI(List<PedidoInternoEstadoDto> estados)
		{
			var lista = estados.Select(e => new ComboGenDto
			{
				Id = e.pie_id,
				Descripcion = e.pie_lista
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private GridCoreSmart<PedidoInternoListaDto> GenerarGridOrdenesDeReparto(List<PedidoInternoListaDto> lista, int page, QueryFilters filtro)
		{
			var pi = lista
				.OrderBy(c => c.pi_compte)
				.ToList();

			const int registrosPorPagina = 10;
			var pagedList = new StaticPagedList<PedidoInternoListaDto>(
				pi,
				page,
				registrosPorPagina,
				pi.Count
			);

			var grid = new GridCoreSmart<PedidoInternoListaDto>
			{
				ListaDatos = pagedList, //lista de combos
				CantidadReg = pi.Count, //cantidad actual de registros
				PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
				UltimoRegistro = Math.Min(page * registrosPorPagina, pi.Count), //define cual es el ultimo registro
				RegistroFinal = pi.Count, //indica cual es el ultimo registro
				CantidadPaginas = (int)Math.Ceiling((double)pi.Count / registrosPorPagina),//calcula la cantidad de paginas
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
