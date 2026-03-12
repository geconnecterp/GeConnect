using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.OrdenDeReparto;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto;
using gc.sitio.core.Servicios.Contratos;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Distribuidora.Controllers
{
	[Area("Distribuidora")]
	public class OrdenDeRepartoController : OrdenDeRepartoControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IOrdenDeRepartoServicio _ordenDeRepartoServicio;
		private readonly IRepartidorServicio _repartidorServicio;
		private readonly IPedidoServicio _pedidoSv;
		private readonly IProductoServicio _productoServicio;

		public OrdenDeRepartoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDeRepartoController> logger,
										IOrdenDeRepartoServicio ordenDeRepartoServicio, IRepartidorServicio repartidorServicio, IPedidoServicio pedidoSv,
										IProductoServicio productoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_ordenDeRepartoServicio = ordenDeRepartoServicio;
			_repartidorServicio = repartidorServicio;
			//OrdenDeRepartoLista = [];
			_pedidoSv = pedidoSv;
			_productoServicio = productoServicio;
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
					throw new NegocioException(ordenes.Mensaje ?? "Hubo algun problema en la busqueda de Ordenes de Reparto.");

				// Para operar con la lista de OR Seleccionados
				var lista = ordenes.ListaEntidad ?? [];
				MetadataGeneral = ordenes.Meta;
				OrdenDeRepartoLista = lista;

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

		[HttpPost]
		public async Task<IActionResult> ObtenerPedidosDeLaOrdenDeReparto(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
				{
					return PartialView("_gridMensaje", CrearRespuestaError("No se recibió el número de orden de reparto para obtener los pedidos."));
				}
				var pedidosResponse = await _ordenDeRepartoServicio.ObtenerPedidosDeLaOrdenDeReparto(orCompte, TokenCookie);
				if (!pedidosResponse.Ok)
				{
					throw new NegocioException(pedidosResponse.Mensaje ?? "Hubo algun problema al obtener los pedidos de la orden de reparto.");
				}
				var pedidos = pedidosResponse.ListaEntidad ?? [];
				GridCoreSmart<PedidoEnOrdenDeRepartoDto> grid = ObtenerGridCoreSmart<PedidoEnOrdenDeRepartoDto>(pedidos);
				return PartialView("_partialPedidosDeOR", grid);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener los pedidos de la orden de reparto"));
			}
		}

		[HttpPost]
		public async Task<IActionResult> AbrirOrdenDeRepartoEnABM(char accion, string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (accion == '\0' || accion == ' ')
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Acción."));
				if (accion == 'M' && string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));

				var or = ObtenerOrdenDeRepartoPorAccion(accion, orCompte);
				var query = new QueryFilters
				{
					Rel01 = new List<string>(),
					Rel02 = new List<string>() { "P" },
					Rel03 = new List<ComboGenDto>(),
					Rel04 = new List<ComboGenDto>(),
					//FechaD = new DateTime(1950, 1, 1),
					//FechaH = new DateTime(2500, 12, 31),
					Registros = 5000,
					Pagina = 1
				};
				var pedidos = await _pedidoSv.BuscarPedidos(query, TokenCookie);
				var model = new OrdenDeRepartoABMModel
				{
					Accion = accion,
					OrdenDeReparto = or,
					ListaRepartidores = ObtenerListaRepartidores(_repartidorServicio.GetRepartidorLista(TokenCookie)),
					ListaPedidosEnOrdenDeReparto = ObtenerGridCoreSmart<PedidoEnOrdenDeRepartoDto>(ObtenerListaDePedidosEnOrdenDeRepartoPorAccion(accion, orCompte)),
					ListaPedidosPendientes = GenerarGridPedidos(pedidos.ListaEntidad, query.Pagina ?? 1, query),
					RepartidorSeleccionado = accion == 'M' ? or.rp_id.ToString() : string.Empty,
				};
				return PartialView("_gridOR_ABM", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener los pedidos de la orden de reparto"));
			}
		}

		[HttpPost]
		public async Task<JsonResult> ConfirmarOrdenDeReparto([FromBody] ConfirmaOrdenDeRepartoRequest dto)
		{
			try
			{
				// Verificar autenticación - consistente con otros métodos
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { ok = false, mensaje = "No autorizado" });

				if (dto == null)
				{
					return Json(new { ok = false, mensaje = "Los datos de confirmación no fueron recepcionados. Verifique." });
				}

				dto.json = JsonConvert.SerializeObject(
					dto.pc.Select(x => new { pc_compte = x })
				);
				dto.adm_id = AdministracionId;
				dto.usu_id = UserName;

				if (string.IsNullOrEmpty(dto.json))
					return Json(new { ok = false, mensaje = "Al menos un pedido es necesario agregar a la orden de reparto." });

				// Llamada al servicio
				var respuesta = await _ordenDeRepartoServicio.ConfirmarOrdenDeReparto(dto, TokenCookie);

				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"ORDEN DE REPARTO fue guardada exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en la confirmación de la orden de reparto: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error al procesar la orden de reparto"
					});
				}
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, "Error inesperado al confirmar la orden de reparto");
				return Json(new
				{
					ok = false,
					error = true,
					msg = "Error interno al procesar la orden de reparto"
				});
			}
		}

		[HttpPost]
		public async Task<IActionResult> AbrirOrdenDeRepartoEnPonerEnCurso(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));

				var itemsAutDepo = await _productoServicio.TRObtenerAutDepositos(AdministracionId, TokenCookie);
				var or = ObtenerOrdenDeRepartoPorAccion('M', orCompte);
				var model = new OrdenDeRepartoPonerEnCursoModel
				{

					OrdenDeReparto = or,
					ListaDepositos = ObtenerGridCoreSmart<TRAutDepoDto>(itemsAutDepo ?? []),
					ListaAnalizaAut = ObtenerGridCoreSmart<AnalizarAutOrdenDeRepartoDto>([])
				};
				return PartialView("_gridOR_PonerEnCurso", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al abrir la orden de reparto para poner en curso"));
			}
		}

		[HttpPost]
		public async Task<IActionResult> AnalizarAutDeOREnPonerEnCurso(string orCompte, string listaDepo)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));
				if (string.IsNullOrEmpty(listaDepo))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Debe seleccionar al menos un depósito."));

				var itemsAnaliza = await _ordenDeRepartoServicio.AnalizarAutOrdenDeReparto(new AnalizarAutOrdenDeRepartoRequest() { or_compte = orCompte, dep_ids = listaDepo, palet_nro = 0, stk_existente = false, sustituto = false }, TokenCookie);
				AnalizarAutOrdenDeRepartoLista = itemsAnaliza.ListaEntidad ?? [];
				return PartialView("_gridOR_PonerEnCurso_TablaAnalizaAut", ObtenerGridCoreSmart<AnalizarAutOrdenDeRepartoDto>(itemsAnaliza.ListaEntidad == null ? [] : itemsAnaliza.ListaEntidad.ToList()));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al abrir la orden de reparto para poner en curso"));
			}
		}

		[HttpPost]
		public async Task<JsonResult> APonerEnCursoOrdenDeReparto([FromBody] APonerEnCursoOrdenDeRepartoRequest dto)
		{
			try
			{
				// Verificar autenticación - consistente con otros métodos
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { ok = false, mensaje = "No autorizado" });

				if (dto == null)
					return Json(new { ok = false, mensaje = "Los datos de análisis no fueron recepcionados. Verifique." });
				if (AnalizarAutOrdenDeRepartoLista== null || !AnalizarAutOrdenDeRepartoLista.Any())
					return Json(new { ok = false, mensaje = "No se han analizado los datos de la orden de reparto para poner en curso. Verifique." });

				var json = JsonConvert.SerializeObject(
					AnalizarAutOrdenDeRepartoLista.Select(x => new
					{
						x.p_id,
						x.p_desc,
						x.pedido,
						x.stk,
						x.stk_adm,
						x.box_id,
						x.depo_id,
						x.depo_nombre,
						x.a_enviar,
						x.a_enviar_box,
						x.fv,
						x.pc_compte,
						x.cta_id,
						x.cta_denominacion,
						x.unidad_palet,
						x.palet,
						x.or_compte,
						x.p_sustituto,
						x.p_id_sustituto,
						x.nota,
						x.p_id_prov,
						x.adm_id
					})
				);

				dto.json = json;
				dto.adm_id = AdministracionId;
				dto.usu_id = UserName;

				// Llamada al servicio
				var respuesta = await _ordenDeRepartoServicio.APonerEnCursoOrdenDeReparto(dto, TokenCookie);

				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Analisis y puesta en curso realizado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en el análisis y puesta en curso de la orden de reparto: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en el análisis y puesta en curso de la orden de reparto"
					});
				}
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, "Error inesperado en el análisis y puesta en curso de la orden de reparto");
				return Json(new
				{
					ok = false,
					error = true,
					msg = "Error interno en el análisis y puesta en curso de la orden de reparto"
				});
			}
		}

		[HttpPost]
		public async Task<IActionResult> AbrirOrdenDeRepartoParaConsolidar(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));

				var itemsAutDepo = await _productoServicio.TRObtenerAutDepositos(AdministracionId, TokenCookie);
				var or = ObtenerOrdenDeRepartoPorAccion('M', orCompte);
				var model = new OrdenDeRepartoPonerEnCursoModel
				{

					OrdenDeReparto = or,
					ListaDepositos = ObtenerGridCoreSmart<TRAutDepoDto>(itemsAutDepo ?? []),
					ListaAnalizaAut = ObtenerGridCoreSmart<AnalizarAutOrdenDeRepartoDto>([])
				};
				return PartialView("_gridOR_PonerEnCurso", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al abrir la orden de reparto para poner en curso"));
			}
		}

		#region Metodos Privados
		private List<PedidoEnOrdenDeRepartoDto> ObtenerListaDePedidosEnOrdenDeRepartoPorAccion(char accion, string orCompte)
		{
			if (accion == 'M')
			{
				var pedidosResponse = _ordenDeRepartoServicio.ObtenerPedidosDeLaOrdenDeReparto(orCompte, TokenCookie).Result;
				if (!pedidosResponse.Ok)
				{
					throw new NegocioException(pedidosResponse.Mensaje ?? "Hubo algun problema al obtener los pedidos de la orden de reparto.");
				}
				return pedidosResponse.ListaEntidad ?? [];
			}
			else if (accion == 'A')
			{
				return [];
			}
			else
			{
				throw new NegocioException("Acción no válida para abrir la orden de reparto.");
			}
		}
		private OrdenDeRepartoDto ObtenerOrdenDeRepartoPorAccion(char accion, string orCompte)
		{
			if (accion == 'M')
			{
				var or = OrdenDeRepartoLista.Where(x => x.or_compte == orCompte).FirstOrDefault();
				if (or == null)
					throw new NegocioException("Hubo algun problema al obtener la orden de reparto.");

				return or;
			}
			else if (accion == 'A')
			{
				var or = new OrdenDeRepartoDto
				{
					ore_id = Convert.ToChar(OrdenDeRepartoEstadoDto.ObtenerId(OrdenDeRepartoEstado.EnCurso)),
					or_fecha = DateTime.Now
				};
				return or;
			}
			else
			{
				throw new NegocioException("Acción no válida para abrir la orden de reparto.");
			}
		}
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
			var lista = estadosLista.Select(x => new ComboGenDto { Id = x.ore_id, Descripcion = x.ore_desc });
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
		private GridCoreSmart<PedidoListDto> GenerarGridPedidos(List<PedidoListDto> lista, int page, QueryFilters filtro)
		{
			var pedidos = lista
				.OrderBy(c => c.pc_compte)
				.ToList();

			const int registrosPorPagina = 10;
			var pagedList = new StaticPagedList<PedidoListDto>(
				pedidos,
				page,
				registrosPorPagina,
				pedidos.Count
			);

			var grid = new GridCoreSmart<PedidoListDto>
			{
				ListaDatos = pagedList, //lista de combos
				CantidadReg = pedidos.Count, //cantidad actual de registros
				PrimerRegistro = ((page - 1) * registrosPorPagina) + 1, //especifica cual es le # inicial de registros
				UltimoRegistro = Math.Min(page * registrosPorPagina, pedidos.Count), //define cual es el ultimo registro
				RegistroFinal = pedidos.Count, //indica cual es el ultimo registro
				CantidadPaginas = (int)Math.Ceiling((double)pedidos.Count / registrosPorPagina),//calcula la cantidad de paginas
				PaginaActual = page,//especifica que pagina es la actual
				Sort = filtro.Sort ?? "pc_compte",
				SortDir = filtro.SortDir ?? "ASC",
				DatoAux01 = $"Pedidos cargados: {DateTime.Now:HH:mm:ss}"
			};

			return grid;
		}
		#endregion
	}
}
