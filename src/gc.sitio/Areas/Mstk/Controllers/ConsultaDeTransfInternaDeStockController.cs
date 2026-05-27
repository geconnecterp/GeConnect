using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.ConsultaDeTransfInternaDeStock;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeTransfInternaDeStockController : ConsultaDeTransfInternaDeStockControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ITipoTRServicio _tipoTRServicio;
		private readonly IProductoServicio _productoServicio;

		public ConsultaDeTransfInternaDeStockController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeTransfInternaDeStockController> logger,
														IAdministracionServicio administracionServicio, ITipoTRServicio tipoTRServicio, IProductoServicio productoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_tipoTRServicio = tipoTRServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeTransfInternaDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE TRANSFERENCIA INTERNA DE STOCK";
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
		public IActionResult InicializarPantallPrincipal(string sucursalesEnvText, string sucursalesRecText, string tiposText, DateTime desde, DateTime hasta)
		{
			var model = new PrincipalConsTransfIntDeStkModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.SucursalesEnv = sucursalesEnvText;
				model.SucursalesRec = sucursalesRecText;
				model.Tipos = tiposText;
				model.Desde = desde;
				model.Hasta = hasta;
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
		public IActionResult BuscarTransferencias(string tipoIdsLista, string sucursalEnvioIdsLista, string sucursalRecibeIdsLista, DateTime Desde, DateTime Hasta)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var request = new TRObtenerListaRequest
				{
					desde = Desde,
					hasta = Hasta,
					tit_id = tipoIdsLista,
					adm_id_des = sucursalEnvioIdsLista,
					adm_id_gen = sucursalRecibeIdsLista
				};
				var lista = _productoServicio.TRObtenerLista(request, TokenCookie).Result;
				var model = ObtenerGridCoreSmart<TRObtenerListaDto>(lista);

				return PartialView("_grillaTransferencias", model);
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
		private void CargarDatosIniciales(ConsultaDeTransfInternaDeStockModel model)
		{
			var hoy = DateTime.Today;
			model.Desde = new DateTime(hoy.Year - 1, hoy.Month, 1);
			model.Hasta = DateTime.Today;

			var tipos = _tipoTRServicio.GetTiposTRLista(TokenCookie);
			if (tipos != null && tipos.Count > 0)
				model.ListaTipos = HelperMvc<ComboGenDto>.ListaGenerica(tipos.Select(x => new ComboGenDto { Id = x.tit_id, Descripcion = x.tit_desc }));
			else
				model.ListaTipos = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursalesEnvia = ObtenerLista(sucursales);
				model.ListaSucursalesRecibe = ObtenerLista(sucursales);
			}
			else
			{
				model.ListaSucursalesEnvia = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.ListaSucursalesRecibe = HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
			ViewBag.SucursalesEnviaList = HelperMvc<ComboGenDto>.ListaGenerica([]);
			ViewBag.SucursalesRecibeList = HelperMvc<ComboGenDto>.ListaGenerica([]);
			ViewBag.TiposList = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
