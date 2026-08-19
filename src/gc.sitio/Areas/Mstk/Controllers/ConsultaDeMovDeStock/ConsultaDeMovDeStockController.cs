using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaDeMovDeStock
{
	[Area("Mstk")]
	public class ConsultaDeMovDeStockController : ConsultaDeMovDeStockControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoMovStkServicio _tipoMovStkServicio;
		private readonly IDepositoServicio _depositoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IConsultasServicio _consultaServicio;
		public ConsultaDeMovDeStockController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeMovDeStockController> logger,
											  ITipoMovStkServicio tipoMovStkServicio, IDepositoServicio depositoServicio, ICuentaServicio cuentaServicio, 
											  IConsultasServicio consultaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoMovStkServicio = tipoMovStkServicio;
			_depositoServicio = depositoServicio;
			_cuentaServicio = cuentaServicio;
			_consultaServicio = consultaServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeMovDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA DE MOVIMIENTO DE STOCK";
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

		public IActionResult CargarBoxesDesdeDeposito(string depId)
		{
			var model = new ListaBoxesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var tm = _depositoServicio.BuscarBoxPorDeposito("%", TokenCookie).Result;
				var lista = tm.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = x.Box_desc });
				model.ListaBoxs= HelperMvc<ComboGenDto>.ListaGenerica(lista);
				return PartialView("_listaBoxes", model);
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

		public async Task<IActionResult> BuscarMovStockProductos(BuscarMovStockProductosRequest request, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new MovDeStockListaModel();
			var lista = new List<MovStkProductoDto>();
			MetadataGrid metadata;
			GridCoreSmart<MovStkProductoDto> grillaDatos;
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (!buscaNew)
				{
					lista = ListaProductoMovStk.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaProductoMovStk = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					//request.Pagina = pag;

					var res = await _consultaServicio.ConsultarProductoMovStk(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaProductoMovStk = lista;

				}
				metadata = MetadataMovStockProd;
				grillaDatos = GenerarGrillaSmart(ListaProductoMovStk, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaProductoMovStk = grillaDatos;

				// Construcción de leyenda
				model.LeyendaTipoMov = ConstruirLeyenda("Tipo de Movimiento", request.lMovTipo, request.lMovTipoTextos);
				model.LeyendaDep = ConstruirLeyenda("Depositos", request.lDep, request.lDepTextos);
				model.LeyendaBox = ConstruirLeyenda("Box", request.lBox, request.lBoxTextos);
				model.LeyendaProv = ConstruirLeyenda("Proveedores", request.lProv, request.lProvTextos);
				model.FechaDesde = ConstruirLeyenda("Desde", request.desde.ToString("dd/MM/yyyy"));
				model.FechaHasta = ConstruirLeyenda("Hasta", request.hasta.ToString("dd/MM/yyyy"));
				model.Producto = ConstruirLeyenda("Producto", request.pId);

				// Leyenda final
				var partesLeyenda = new List<string>();

				if (!string.IsNullOrWhiteSpace(model.LeyendaTipoMov))
					partesLeyenda.Add(model.LeyendaTipoMov);

				if (!string.IsNullOrWhiteSpace(model.LeyendaDep))
					partesLeyenda.Add(model.LeyendaDep);

				if (!string.IsNullOrWhiteSpace(model.LeyendaProv))
					partesLeyenda.Add(model.LeyendaProv);

				if (!string.IsNullOrWhiteSpace(model.LeyendaBox))
					partesLeyenda.Add(model.LeyendaBox);

				if (!string.IsNullOrWhiteSpace(model.FechaDesde))
					partesLeyenda.Add(model.FechaDesde);

				if (!string.IsNullOrWhiteSpace(model.FechaHasta))
					partesLeyenda.Add(model.FechaHasta);

				if (!string.IsNullOrWhiteSpace(model.Producto))
					partesLeyenda.Add(model.Producto);

				return PartialView("_grillaMovStockProductos", model);
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

		#region metodos privados

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
		private void CargarDatosIniciales(ConsultaDeMovDeStockModel model)
		{
			model.FechaHasta = DateTime.Now;
			model.FechaDesde = DateTime.Now.AddDays(-35);
			model.Texto = string.Empty;
			model.ListaTipoMovimientos = ComboTipoMovStk();
			model.ListaBoxs = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			var BoxsList = new List<ComboGenDto>();
			ViewBag.BoxsList = HelperMvc<ComboGenDto>.ListaGenerica(BoxsList);
			var DepositosList = new List<ComboGenDto>();
			ViewBag.DepositosList = HelperMvc<ComboGenDto>.ListaGenerica(DepositosList);
			var TipoMovimientosList = new List<ComboGenDto>();
			ViewBag.TipoMovimientosList = HelperMvc<ComboGenDto>.ListaGenerica(TipoMovimientosList);
			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");
			var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
			if (depositos != null && depositos.Count > 0)
				model.ListaDepositos = ComboDepositos(depositos);
			else
				model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ComboTipoMovStk()
		{
			var tm = _tipoMovStkServicio.ObtenerTiposDeMovimientosDeStock(TokenCookie);
			var lista = tm.Select(x => new ComboGenDto { Id = x.sm_tipo, Descripcion = x.sm_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboBoxes()
		{
			var tm = _depositoServicio.BuscarBoxPorDeposito("%", TokenCookie).Result;
			var lista = tm.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = x.Box_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
