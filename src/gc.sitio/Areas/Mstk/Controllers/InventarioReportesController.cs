using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Tipos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class InventarioReportesController : InventarioReportesControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IInventarioServicio _inventarioServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IInventarioEstadoServicio _inventarioEstadoServicio;
		public InventarioReportesController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioReportesController> logger,
											IInventarioServicio inventarioServicio, IAdministracionServicio administracionServicio,
											IInventarioEstadoServicio inventarioEstadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
			_administracionServicio = administracionServicio;
			_inventarioEstadoServicio = inventarioEstadoServicio;
		}

		public IActionResult Index()
		{
			var model = new InventarioReporteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTES DE INVENTARIOS";
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

		public IActionResult InicializarPantallPrincipal()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_inventarioReportePantallaPrincipal");
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult BuscarInventarioLista(GetInventarioListaRequest request)
		{
			var model = new InventarioCargaGrillaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				request.usu_id = "%";
				var lista = _inventarioServicio.GetInventarioLista(request, TokenCookie);
				model.GrillaInventario = ObtenerGridCoreSmart<InventarioListaDto>(lista);
				ListaInventarioEnReporte = lista;
				return PartialView("_gridInventario", model);
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
		private void CargarDatosIniciales(InventarioReporteModel model)
		{
			model.Desde = DateTime.Now.Date.AddMonths(-1);
			model.Hasta = DateTime.Now.Date;
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", Token);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursales = ObtenerSucursales(sucursales);
				var suc = sucursales.Where(x => x.Adm_id == AdministracionId).FirstOrDefault();
				if (suc != null && suc.Adm_central == 'S')
					model.HabilitarCambioDeSucursalSeleccionada = false;
				else
					model.HabilitarCambioDeSucursalSeleccionada = true;
			}
			else
			{
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.HabilitarCambioDeSucursalSeleccionada = false;
			}
			model.SucursalSeleccionada = AdministracionId;
			var estados = _inventarioEstadoServicio.GetInventarioEstadoLista(TokenCookie);
			if (estados != null && estados.Count > 0)
				model.ListaEstados = ObtenerEstadosDeInventario(estados);
			else
				model.ListaEstados = HelperMvc<ComboGenDto>.ListaGenerica([]);
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

		private SelectList ObtenerEstadosDeInventario(List<InventarioEstadoDto> estados)
		{
			var lista = estados.Select(e => new ComboGenDto
			{
				Id = e.inve_id,
				Descripcion = e.inve_desc
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
