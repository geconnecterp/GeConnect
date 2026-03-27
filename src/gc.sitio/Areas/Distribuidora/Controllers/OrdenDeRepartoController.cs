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
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Distribuidora.Models.OrdenDeReparto;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
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

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1;
		private AppModulo _modulo_2;
		private AppModulo _modulo_4;
		private string APP_MODULO_1 = AppModulos.ORDEN_DE_REPARTO_HOJA_DE_RUTA.ToString();
		private string APP_MODULO_2 = AppModulos.ORDEN_DE_REPARTO_HOJA_DE_PRODUCTO.ToString();
		private string APP_MODULO_4 = AppModulos.PEDIDO_DE_CLIENTE.ToString();
		private readonly IDocManagerServicio _docMSv;

		public OrdenDeRepartoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDeRepartoController> logger,
										IOrdenDeRepartoServicio ordenDeRepartoServicio, IRepartidorServicio repartidorServicio, IPedidoServicio pedidoSv,
										IProductoServicio productoServicio, IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_ordenDeRepartoServicio = ordenDeRepartoServicio;
			_repartidorServicio = repartidorServicio;
			//OrdenDeRepartoLista = [];
			_pedidoSv = pedidoSv;
			_productoServicio = productoServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_modulo_4 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_4);
			_docMSv = docManager; //instancio el servicio de impresión
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
				var listaRep = ObtenerListaRepartidores(_repartidorServicio.GetRepartidorLista(TokenCookie));
				var listaPedidosEnOrdenDeReparto = ObtenerGridCoreSmart<PedidoEnOrdenDeRepartoDto>(ObtenerListaDePedidosEnOrdenDeRepartoPorAccion(accion, orCompte));
				var query = new QueryFilters
				{
					Rel01 = new List<string>(),
					Rel02 = new List<string>() { "P" },
					Rel03 = new List<ComboGenDto>(),
					Rel04 = new List<ComboGenDto>(),
					//FechaD = new DateTime(1950, 1, 1),
					//FechaH = new DateTime(2500, 12, 31),
					Registros = 500,
					Pagina = 1
				};
				var pedidos = _pedidoSv.BuscarPedidos(query, TokenCookie).Result;
				var model = new OrdenDeRepartoABMModel
				{
					Accion = accion,
					OrdenDeReparto = or,
					ListaRepartidores = listaRep,
					ListaPedidosEnOrdenDeReparto = listaPedidosEnOrdenDeReparto,
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
				if (AnalizarAutOrdenDeRepartoLista == null || !AnalizarAutOrdenDeRepartoLista.Any())
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
		public async Task<IActionResult> CargarVistaConsolidarOrdenDeReparto(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));

				var respuestaGen = await _ordenDeRepartoServicio.ObtenerPedidosDeLaOrdenDeReparto(orCompte, TokenCookie);
				if (respuestaGen == null)
					return PartialView("_gridMensaje", CrearRespuestaError("No se han podido obtener los pedidos de la orden de reparto."));
				if (respuestaGen.ListaEntidad == null || respuestaGen.ListaEntidad.Count == 0)
					return PartialView("_gridMensaje", CrearRespuestaError("No se han encontrado pedidos asociados a la orden de reparto."));

				CargarDetalleDeLosPedidosDeLaOrdenDeReparto(orCompte, respuestaGen.ListaEntidad);
				CalcularUpDownEnPedidosDeLaOrdenDeReparto(respuestaGen.ListaEntidad);

				var respuestaGen2 = await _ordenDeRepartoServicio.AConsolidarConteos(orCompte, TokenCookie);
				var listaConteos = respuestaGen2.ListaEntidad ?? [];
				var model = new OrdenDeRepartoConsolidarModel
				{
					OrdenDeReparto = ObtenerOrdenDeRepartoPorAccion('M', orCompte),
					ListaPedidosEnOrdenDeReparto = ObtenerGridCoreSmart<PedidoEnOrdenDeRepartoDto>(respuestaGen.ListaEntidad),
					ListaConteosDeLaOrdenDeReparto = ObtenerGridCoreSmart<AConsolidarConteosDto>(listaConteos),
					ListaDetallesAConsolidar = ObtenerGridCoreSmart<AConsolidarPedidoClienteDetalleDto>([]),
					ListaDetalleProductoSeleccionado = ObtenerGridCoreSmart<AConsolidarPedidoClienteDetalleDto>([])
				};
				return PartialView("_gridOR_Consolidar", model);
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
		public IActionResult CargarDetalleDelPedidoDeLaOrdenEnConsolidar(string orCompte, string pcCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));
				if (string.IsNullOrEmpty(pcCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Pedido de Cliente."));

				var itemsDetalle = AConsolidarPedidoClienteDetalleLista.Where(x => x.or_compte == orCompte && x.pc_compte == pcCompte).ToList();
				return PartialView("_gridOR_Consolidar_PedidoDeORDetalle", ObtenerGridCoreSmart<AConsolidarPedidoClienteDetalleDto>(itemsDetalle == null ? [] : [.. itemsDetalle]));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener el detalle de productos del pedido en Consolidar."));
			}
		}

		[HttpPost]
		public async Task<IActionResult> CargarConteosEnConsolidar(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));

				var itemsConteos = await _ordenDeRepartoServicio.AConsolidarConteos(orCompte, TokenCookie);
				return PartialView("_gridOR_Consolidar_PedidoDeORConteos", ObtenerGridCoreSmart<AConsolidarConteosDto>(itemsConteos.ListaEntidad == null ? [] : [.. itemsConteos.ListaEntidad]));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener el detalle de productos del pedido en Consolidar."));
			}
		}

		[HttpPost]
		public IActionResult CargarDetalleDelProductoEnConteoEnConsolidar(string orCompte, string pId)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));
				if (string.IsNullOrEmpty(pId))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Producto."));

				var itemsDetalle = AConsolidarPedidoClienteDetalleLista.Where(x => x.or_compte == orCompte && x.p_id == pId).ToList();
				return PartialView("_gridOR_Consolidar_PedidoDeORConteosDetalle", ObtenerGridCoreSmart<AConsolidarPedidoClienteDetalleDto>(itemsDetalle == null ? [] : [.. itemsDetalle]));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener el detalle de productos del pedido en Consolidar."));
			}
		}

		/// <summary>
		/// Funcion que se usa para actualizar los valores en session de la lista de productos a consolidar, luego de realizar una reasignacion de cantidades en la vista de consolidar.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult GuardarReasignacionEnDatosDeSesion([FromBody] GuardarReasignacionEnDatosDeSesionRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (request.Detalle == null || request.Detalle.Count <= 0)
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Productos reasignados." });

				var listaTemp = AConsolidarPedidoClienteDetalleLista;
				foreach (var i in request.Detalle)
				{
					var item = listaTemp.Where(x => x.or_compte == i.orCompte && x.pc_compte == i.pcCompte && x.p_id == i.pId).First();
					if (item == null)
						continue;
					item.cantidad = i.Cantidad;
				}
				AConsolidarPedidoClienteDetalleLista = listaTemp;
				return Json(new { error = false, warn = false, mensaje = "OK" });
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

		[HttpPost]
		public JsonResult ConfirmarConsolidarOrdenDeReparto(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(orCompte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Orden de Reparto." });

				var request = new AConciliarOrdenDeRepartoRequest
				{
					or_compte = orCompte,
					adm_id = AdministracionId,
					usu_id = UserName,
					json = JsonConvert.SerializeObject(MapearAConsolidarOrden(AConsolidarPedidoClienteDetalleLista))
				};

				var respuesta = _ordenDeRepartoServicio.AConsolidarOrdenDeReparto(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Consolidar orden de reparto realizado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en consolidar la orden de reparto: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en consolidar la orden de reparto"
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


		[HttpPost]
		public async Task<IActionResult> CargarVistaCambioPrecioOrdenDeReparto(CambioDePrecioRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(request.or_compte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));
				if (string.IsNullOrEmpty(request.lp_id))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Lista de Precios."));

				var respuestaGen = await _ordenDeRepartoServicio.CambioDePreciosLista(request, TokenCookie);
				if (respuestaGen == null)
					return PartialView("_gridMensaje", CrearRespuestaError("No se han podido obtener los datos para cambio de precios."));

				CambioPrecioLista = respuestaGen.ListaEntidad ?? [];
				var model = new OrdenDeRepartoCambioPrecioModel
				{
					OrdenDeReparto = ObtenerOrdenDeRepartoPorAccion('M', request.or_compte),
					ListaCambioPrecios = ObtenerGridCoreSmart<CambioDePrecioDto>(respuestaGen.ListaEntidad ?? []),
				};
				return PartialView("_gridOR_CambioPrecio", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al abrir la orden de reparto para cambio de precio"));
			}
		}

		[HttpPost]
		public JsonResult ConfirmarCambioPreciosEnOrdenDeReparto(string orCompte, List<CambiaPrecioOrdenDeReparto> prods)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(orCompte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Orden de Reparto." });
				if (prods == null || prods.Count <= 0)
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Productos y precios para modificar." });

				var request = new CambioDePrecioConfirmaRequest
				{
					or_compte = orCompte,
					adm_id = AdministracionId,
					usu_id = UserName,
					json = JsonConvert.SerializeObject(prods)
				};

				var respuesta = _ordenDeRepartoServicio.CambioDePreciosEnOrdenDeReparto(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Cambios de precios en orden de reparto realizado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Cambios de precios en orden de reparto: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en cambios de precios en la orden de reparto"
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

		[HttpPost]
		public JsonResult CambiarEstadoOrdenDeReparto(CambiarEstadoRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(request.or_compte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Orden de Reparto." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;

				var respuesta = _ordenDeRepartoServicio.CambiarEstadoOrdenDeReparto(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Cambios de estado en orden de reparto realizado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Cambios de estado en orden de reparto: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en cambios de estado en la orden de reparto"
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
					case TipoDeReporte.RepoHojaDeRuta:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Imprimir Hoja de Ruta";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.RepoHojaDeProducto:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Imprimir Hoja de Producto";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					case TipoDeReporte.RepoOrdenDeReparto:
						#region Gestor Impresion - Inicializacion de variables
						//titulo = "Valorizado por Rubros";
						//DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_3);
						//ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_3);
						#endregion
						break;
					case TipoDeReporte.RepoPedidoDeCliente:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Pedido de Cliente";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					//case TipoDeReporte.RepoConteoPorUsu:
					//	#region Gestor Impresion - Inicializacion de variables
					//	titulo = "Planilla por Usuarios";
					//	DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_5);
					//	ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_5);
					//	#endregion
					//	break;
					default:
						break;
				}

				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		[HttpPost]
		public JsonResult PasarPedidoDeClienteACF(PasarPedidoACFRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(request.pc_compte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Pedido de Cliente." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;

				var respuesta = _pedidoSv.PasarPedidoACF(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Se ha actualizado el pedido de cliente a CF exitósamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Paso a CF de pedido de cliente: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en el paso a CF del pedido de cliente."
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

		[HttpPost]
		public IActionResult CargarPedidosDeLaOrdenDeReparto(string orCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(orCompte))
					return PartialView("_gridMensaje", CrearRespuestaError("No se han provisto los datos necesarios: Orden de Reparto."));
				var listaPedidosEnOrdenDeReparto = ObtenerGridCoreSmart<PedidoEnOrdenDeRepartoDto>(ObtenerListaDePedidosEnOrdenDeRepartoPorAccion('M', orCompte));
				var model = new PedidoDeClienteModel
				{
					ListaPedidosEnOrdenDeReparto = listaPedidosEnOrdenDeReparto
				};
				return PartialView("_gridPC_PedidosDeOR", model);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al obtener los pedidos de la orden de reparto."));
			}
		}

		[HttpPost]
		public JsonResult DividePedidoDeCliente(DividePedidoDeClienteRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(new { error = true, ok = false, mensaje = "No autorizado" });
				if (string.IsNullOrEmpty(request.pc_compte))
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Pedido de Cliente." });
				if (request.divide <= 0)
					return Json(new { error = true, ok = false, mensaje = "No se han provisto los datos necesarios: Unidades a dividir el Pedido de Cliente." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;

				var respuesta = _pedidoSv.DividePedidoDeCliente(request, TokenCookie).Result;
				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"Se ha dividido el pedido de cliente exitósamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("División de pedido de cliente: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error en la división del pedido de cliente."
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

		#region Metodos Privados
		enum TipoDeReporte
		{
			RepoHojaDeRuta = 1,
			RepoHojaDeProducto = 2,
			RepoOrdenDeReparto = 3,
			RepoPedidoDeCliente = 4,
		}
		private void CalcularUpDownEnPedidosDeLaOrdenDeReparto(List<PedidoEnOrdenDeRepartoDto> pedidosDeLaOrdenDeReparto)
		{
			if (AConsolidarPedidoClienteDetalleLista == null || AConsolidarPedidoClienteDetalleLista.Count == 0)
				return;

			var listaTemp = AConsolidarPedidoClienteDetalleLista;
			// 1) Agrupar por OR y Pedido
			var grupos = listaTemp
				.GroupBy(x => new { x.or_compte, x.pc_compte })
				.ToList();

			foreach (var grupo in grupos)
			{
				string or = grupo.Key.or_compte;
				string pc = grupo.Key.pc_compte;

				// Sublista de productos de ese OR + Pedido
				var productosDelPedido = grupo.ToList();
				var pedido = pedidosDeLaOrdenDeReparto.Where(x => x.or_compte == or && x.pc_compte == pc).FirstOrDefault();

				if (productosDelPedido == null || productosDelPedido.Count <= 0)
					continue;
				if (pedido == null)
					continue;

				if (productosDelPedido.Where(x => (x.pcd_pedida - x.cantidad) < 0).Any())
					pedido.mostrar_down = true;
				if (productosDelPedido.Where(x => (x.pcd_pedida - x.cantidad) > 0).Any())
					pedido.mostrar_up = true;

			}
		}

		private void CargarDetalleDeLosPedidosDeLaOrdenDeReparto(string orCompte, List<PedidoEnOrdenDeRepartoDto> lista)
		{
			if (lista == null || lista.Count <= 0)
				return;
			var listaIdsPedidos = lista.Select(x => x.pc_compte).ToList();
			if (listaIdsPedidos == null || listaIdsPedidos.Count <= 0)
				return;
			var listaTemp = new List<AConsolidarPedidoClienteDetalleDto>();
			//Dejo este codigo comentado por las dudas.
			//foreach (var item in listaIdsPedidos)
			//{
			//	var itemsDetalle = _ordenDeRepartoServicio.AConsolidarPedidoClienteDetalle(new AConsolidarPedidoClienteDetalleRequest() { or_compte = orCompte, pc_compte = item, p_id = "%" }, TokenCookie).Result.ListaEntidad;
			//	if (itemsDetalle != null && itemsDetalle.Count > 0)
			//		listaTemp.AddRange(itemsDetalle);
			//}
			var itemsDetalle = _ordenDeRepartoServicio.AConsolidarPedidoClienteDetalle(new AConsolidarPedidoClienteDetalleRequest() { or_compte = orCompte, pc_compte = "%", p_id = "%" }, TokenCookie).Result.ListaEntidad;
			listaTemp.AddRange(itemsDetalle ?? []);
			AConsolidarPedidoClienteDetalleLista = listaTemp;
		}

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
			if (lista == null)
				return ObtenerGridCoreSmart<PedidoListDto>([]);

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

		private static List<ConsolidarOrdenDeReparto> MapearAConsolidarOrden(List<AConsolidarPedidoClienteDetalleDto> origen)
		{
			if (origen == null)
				return [];

			return [.. origen.Select(x => new ConsolidarOrdenDeReparto
			{
				pc_compte = x.pc_compte,
				pcd_item = x.pcd_item,
				p_id = x.p_id,
				p_desc = x.p_desc,
				pcd_pedida = x.pcd_pedida,
				cantidad = x.cantidad,
				pcd_origen = x.pcd_origen,
				p_id_reemplazo = x.p_id_remplazo
			})];
		}

		private class ConsolidarOrdenDeReparto
		{

			public string pc_compte { get; set; } = string.Empty;
			public string pcd_item { get; set; } = string.Empty;
			public string p_id { get; set; } = string.Empty;
			public string p_desc { get; set; } = string.Empty;
			public decimal pcd_pedida { get; set; }
			public decimal cantidad { get; set; }
			public char pcd_origen { get; set; }
			public string p_id_reemplazo { get; set; } = string.Empty;
		}

		public class CambiaPrecioOrdenDeReparto
		{
			public string p_id { get; set; } = string.Empty;
			public decimal pcd_pvta { get; set; }
			public decimal p_vta_ctl { get; set; }
		}
		#endregion
	}
}
