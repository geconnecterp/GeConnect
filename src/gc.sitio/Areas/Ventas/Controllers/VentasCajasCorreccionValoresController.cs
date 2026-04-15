using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentasCajasCorreccionValoresController : VentasCajasCorreccionValoresControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICajaServicio _iCajaSrv;
		private readonly IApiVentasServicio _apiVentasServicio;
		private readonly IAdministracionServicio _administracionServicio;
		public VentasCajasCorreccionValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCorreccionValoresController> logger,
													  ICajaServicio cajaServicio, IAdministracionServicio administracionServicio, 
													  IApiVentasServicio apiVentasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
			_administracionServicio = administracionServicio;
			_apiVentasServicio = apiVentasServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroCtlValoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CORRECCIÓN DE VALORES RENDIDOS POR PV";
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
		public async Task<IActionResult> ObtenerDiasPorSucursal(string suc_id)
		{
			var model = new DiasPorSucursalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlProcesosLista(suc_id, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener días por sucursal");

				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener días por sucursal");
				
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
					model.ListaDias = HelperMvc<ComboGenDto>.ListaGenerica([]);
				else
					model.ListaDias = HelperMvc<ComboGenDto>.ListaGenerica(resultado.ListaEntidad.Select(x => new ComboGenDto { Id = x.caja_nro_proceso, Descripcion = $"{x.caja_habilitacion.ToString("dd/MM/yy")} ({x.caja_nro_proceso})" }));
				VtasPVCtlProcesoLista = resultado.ListaEntidad ?? [];
				return PartialView("_dias_por_suc", model);
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
		private void CargarDatosIniciales(FiltroCtlValoresModel model)
		{
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.ListaDias = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
