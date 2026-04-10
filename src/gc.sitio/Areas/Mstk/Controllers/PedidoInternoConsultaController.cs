using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models.PedidoInternoConsulta;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class PedidoInternoConsultaController : PedidoInternoConsultaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IProductoServicio _productoServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IPedidoInternoEstadoServicio _pedidoInternoEstadoServicio;
		public PedidoInternoConsultaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<PedidoInternoConsultaController> logger,
											   IProductoServicio productoServicio, IAdministracionServicio administracionServicio,
											   IPedidoInternoEstadoServicio pedidoInternoEstadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_productoServicio = productoServicio;
			_administracionServicio = administracionServicio;
			_pedidoInternoEstadoServicio = pedidoInternoEstadoServicio;
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

		#region Métodos Privados
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
		#endregion
	}
}
