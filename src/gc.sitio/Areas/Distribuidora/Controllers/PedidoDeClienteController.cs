using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Pedidos;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Distribuidora.Models.PedidoDeCliente;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using X.PagedList;

namespace gc.sitio.Areas.Distribuidora.Controllers
{
	[Area("Distribuidora")]
	public class PedidoDeClienteController : PedidoDeClienteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IPedidoDeClienteEstadoServicio _pedidoDeClienteEstadoServicio;
		private readonly IVendedorServicio _vendedorServicio;
		private readonly IRepartidorServicio _repartidorServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IPedidoServicio _pedidoSv;

		public PedidoDeClienteController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<PedidoDeClienteController> logger,
										 ICuentaServicio cuentaServicio, IPedidoDeClienteEstadoServicio pedidoDeClienteEstadoServicio, IVendedorServicio vendedorServicio, 
										 IRepartidorServicio repartidorServicio, IRubroServicio rubroServicio, IPedidoServicio pedidoSv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_pedidoDeClienteEstadoServicio = pedidoDeClienteEstadoServicio;
			_vendedorServicio = vendedorServicio;
			_repartidorServicio = repartidorServicio;
			_rubroServicio = rubroServicio;
			_pedidoSv = pedidoSv;
		}

		public IActionResult Index()
		{
			var model = new FiltroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "PEDIDOS DE CLIENTES";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				//ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);
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

		[HttpPost]
		public async Task<IActionResult> BuscarPedidosDeCliente(QueryFilters filters)
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
					(filters.Rel04 == null || !filters.Rel04.Any()) &&
					(filters.Rel02 == null || !filters.Rel02.Any()) &&
					(filters.Rel03 == null || !filters.Rel03.Any()))
				{
					return PartialView("_gridMensaje", CrearRespuestaError("Debe seleccionar algún filtro para buscar los Pedidos"));
				}

				filters.Registros = _setting.NroRegistrosPagina;

				filters.Rel01 = filters.Rel01?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				filters.Rel02 = filters.Rel02?.Where(x => !string.IsNullOrEmpty(x)).ToList();
				filters.Rel03 = filters.Rel03?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
				filters.Rel04 = filters.Rel04?.Where(x => !string.IsNullOrEmpty(x.Id)).ToList();
				//debo realizar la busqueda de los presupuestos
				var pedidos = await _pedidoSv.BuscarPedidos(filters, TokenCookie);

				if (!pedidos.Ok)
				{
					throw new NegocioException(pedidos.Mensaje ?? "Hubo algun problema en la busqueda de Pedidos de Cliente.");
				}

				// Para operar con la lista de Productos Seleccionados
				var lista = pedidos.ListaEntidad ?? new List<PedidoListDto>();
				MetadataGeneral = pedidos.Meta;

				// Generar grid con productos mapeados
				GridCoreSmart<PedidoListDto> grid = GenerarGridPedidos(lista, filters.Pagina ?? 1, filters);

				return PartialView("_gridPedido", grid);
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
		public async Task<IActionResult> ObtenerPedidoDatos(string pcCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (pcCompte == null)
				{
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del pedido no fue recepcionado."));
				}

				var ped = await _pedidoSv.ObtenerPedido(pcCompte, TokenCookie);
				if (!ped.Ok)
				{
					throw new NegocioException(ped.Mensaje ?? "No se ha podido identificar el pedido.");
				}

				if (ped.ListaEntidad == null || ped.ListaEntidad.Count() == 0)
				{
					throw new NegocioException("No se encontraron los datos del Pedido de Cliente");
				}

				return PartialView("_pedidoDatos", ped.ListaEntidad[0]);
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
		public async Task<IActionResult> ObtenerPedidoProducto(string pcCompte)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				if (string.IsNullOrWhiteSpace(pcCompte))
				{
					return PartialView("_gridMensaje", CrearRespuestaError("El Identificador del pedido no fue recepcionado."));
				}

				var ped = await _pedidoSv.ObtenerDetalleDePedido(pcCompte, TokenCookie);
				if (!ped.Ok)
				{
					throw new NegocioException(ped.Mensaje ?? "No se ha podido obtener el detalle del pedido.");
				}

				// Generar grid con productos del presupuesto
				var productos = ped.ListaEntidad ?? [];
				ProductosActualesEnPedido = productos;

				var grid = GenerarGridPedidoProductos(productos);

				return PartialView("_pedidoProds", grid);
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error al obtener productos del pedido");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error al obtener productos del pedido");
				return PartialView("_gridMensaje", CrearRespuestaError("Error al cargar productos del pedido"));
			}
		}

		[HttpPost]
		public IActionResult NuevoPedido()
		{
			if (!VerificarAutenticacion(out IActionResult redirectResult))
				return redirectResult;
			
			PedidoDto pedido = new()
			{
				adm_id = AdministracionId,
				adm_nombre = AdministracionName
				
			};

			return PartialView("_pedidoDatos", pedido);
		}

		[HttpPost]
		public async Task<JsonResult> ConfirmarPedido([FromBody] ConfirmarPedidoDto dto)
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

				// Validaciones de entrada
				if (dto == null || string.IsNullOrEmpty(dto.Datos.cta_id))
				{
					return Json(new { ok = false, mensaje = "Los datos del pedido son requeridos" });
				}
				ConfirmarPedidoRequest request = new()
				{
					adm_id = AdministracionId,
					usu_id = UserName,
					abm = dto.Abm.ToString(),
					cta_id = dto.Datos.cta_id,
					pc_obs = dto.Datos.pc_obs,
					pc_cf = dto.Datos.pc_cf,
					pc_compte = dto.Datos.pc_compte,
					json_prod = JsonConvert.SerializeObject(dto.Productos),
					pc_fecha = dto.Datos.pc_fecha,
					pc_entrega = dto.Datos.pc_entrega.Value,
				};

				if (string.IsNullOrEmpty(request.json_prod))
				{
					return Json(new { ok = false, mensaje = "Al menos un producto es necesario informar en el pedido." });
				}

				// Llamada al servicio
				var respuesta = await _pedidoSv.ConfirmarPedido(request, TokenCookie);

				// Procesamiento de respuesta
				if (respuesta.Ok && !respuesta.EsError && !respuesta.EsWarn)
				{
					// Log y limpieza de datos temporales
					var msg = $"PEDIDO de {request.cta_id}-{request.cta_denominacion} fue guardado exitosamente.";
					_logger?.LogInformation(msg);
					return AnalizarRespuesta(respuesta, msg);
					// Respuesta de éxito
					//return Json(new
					//{
					//	ok = true,
					//	error = false,

					//	msg//respuesta.Mensaje ?? "Pedido guardado correctamente"
					//});
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error en la confirmación del pedido: {Mensaje}", respuesta.Mensaje);
					return Json(new
					{
						ok = false,
						error = respuesta.EsError,
						warn = respuesta.EsWarn,
						mensaje = respuesta.Mensaje ?? "Error al procesar el pedido"
					});
				}
			}
			catch (Exception ex)
			{
				// Manejo de excepciones no esperadas
				_logger?.LogError(ex, "Error inesperado al confirmar pedido");
				return Json(new
				{
					ok = false,
					error = true,
					msg = "Error interno al procesar la solicitud"
				});
			}
		}

		#region Metodos Privados
		private GridCoreSmart<PedidoProductoDto> GenerarGridPedidoProductos(List<PedidoProductoDto> productos)
		{
			const int registrosPorPagina = 50; // Mayor cantidad para productos
			var ordenados = productos.OrderBy(p => p.p_id).ToList();

			var pagedList = new StaticPagedList<PedidoProductoDto>(
				ordenados,
				1,
				registrosPorPagina,
				ordenados.Count
			);

			return new GridCoreSmart<PedidoProductoDto>
			{
				ListaDatos = pagedList,
				CantidadReg = ordenados.Count,
				PrimerRegistro = 1,
				UltimoRegistro = ordenados.Count,
				RegistroFinal = ordenados.Count,
				CantidadPaginas = 1,
				PaginaActual = 1,
				Sort = "p_id",
				SortDir = "ASC",
				DatoAux01 = $"Productos: {ordenados.Count} | Total: {ordenados.Sum(x => x.pcd_pvta):N2}"
			};
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

		private void CargarDatosIniciales(FiltroModel model)
		{
			//CLIENTE
			var listR01 = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

			if (CuentasLista.Count == 0)
				ObtenerCuentas(_cuentaServicio, 'D', "%");

			if (RubroLista.Count == 0)
			{
				ObtenerRubros(_rubroServicio);
			}

			if (ProveedoresLista.Count == 0)
			{
				ObtenerProveedores(_cuentaServicio, "BI");
			}

			#region Carga de Rubros
			var rubs = RubroLista
				.Select(r => new ComboGenDto
				{
					Id = r.Rub_Id,
					Descripcion = r.Rub_Id + " - " + r.Rub_Desc
				})
				.ToList();
			ViewBag.Rel02B2 = HelperMvc<ComboGenDto>.ListaGenerica(rubs);
			#endregion

			var estados = _pedidoDeClienteEstadoServicio.GetPedidoDeClienteEstados(TokenCookie);
			if (estados != null && estados.Count > 0)
				model.ListaEstados = ObtenerListaEstados(estados);
			else
				model.ListaEstados = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var vendedores = _vendedorServicio.GetVendedorLista(TokenCookie);
			if (vendedores != null && vendedores.Count > 0)
				model.ListaVendedores = ObtenerListaVendedores(vendedores);
			else
				model.ListaVendedores = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var repartidores = _repartidorServicio.GetRepartidorLista(TokenCookie);
			if (repartidores != null && repartidores.Count > 0)
				model.ListaRepartidores = ObtenerListaRepartidores(repartidores);
			else
				model.ListaRepartidores = HelperMvc<ComboGenDto>.ListaGenerica([]);

			var EstadosList = new List<ComboGenDto>();
			ViewBag.EstadosList = HelperMvc<ComboGenDto>.ListaGenerica(EstadosList);
			var VendedoresList = new List<ComboGenDto>();
			ViewBag.VendedoresList = HelperMvc<ComboGenDto>.ListaGenerica(VendedoresList);
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			var RepartidoresList = new List<ComboGenDto>();
			ViewBag.RepartidoresList = HelperMvc<ComboGenDto>.ListaGenerica(RepartidoresList);
			var listR03 = new List<ComboGenDto>();
			ViewBag.Rel03B2 = HelperMvc<ComboGenDto>.ListaGenerica(listR03);
		}

		private SelectList ObtenerListaEstados(List<PedidoDeClienteEstadoDto> estadosLista)
		{
			var lista = estadosLista.Select(x => new ComboGenDto { Id = x.pce_id, Descripcion = x.pce_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaVendedores(List<VendedorDto> vendedorLista)
		{
			var lista = vendedorLista.Select(x => new ComboGenDto { Id = x.ve_id, Descripcion = x.ve_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerListaRepartidores(List<RepartidorDto> repartidorLista)
		{
			var lista = repartidorLista.Select(x => new ComboGenDto { Id = x.rp_id, Descripcion = x.rp_lista });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
