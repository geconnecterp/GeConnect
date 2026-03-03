using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
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

		#region Metodos Privados
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
