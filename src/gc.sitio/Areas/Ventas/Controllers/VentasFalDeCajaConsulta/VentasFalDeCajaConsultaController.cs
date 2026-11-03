using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasFalDeCajaCarga;
using gc.sitio.Areas.Ventas.Models.VentasFalDeCajaConsulta;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers.VentasFalDeCajaConsulta
{
	[Area("Ventas")]
	public class VentasFalDeCajaConsultaController : VentasFalDeCajaConsultaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICajaServicio _iCajaSrv;
		private readonly IAdministracionServicio _administracionServicio;

		public VentasFalDeCajaConsultaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasFalDeCajaConsultaController> logger,
												 ICajaServicio cajaServicio, IAdministracionServicio administracionServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
			_administracionServicio = administracionServicio;
		}

		public IActionResult Index()
		{
			var model = new VentasFalDeCajaConsultaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "FALTANTE DE CAJAS - CONSULTA";
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
		public IActionResult CargarPantallaPrincipal(DateTime desde, DateTime hasta, string lSucTextos)
		{
			var model = new PantallaPrincipalModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Desde = desde;
				model.Hasta = hasta;
				model.Sucursal = lSucTextos;
				return PartialView("_partial_Principal", model);
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

		public IActionResult CargarFaltantesDeCajasListado(DateTime desde, DateTime hasta, string lSuc)
		{
			var model = new GridCoreSmart<FaltanteDeCajaListaDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var request = new FaltanteDeCajaListaRequest
				{
					adm_list = lSuc,
					desde = desde,
					hasta = hasta
				};
				var response = _iCajaSrv.FaltanteDeCajaConsulta(request, TokenCookie).Result;
				if (response != null && response.Count > 0)
				{
					model = ObtenerGridCoreSmart<FaltanteDeCajaListaDto>(response);
					return PartialView("_partial_GridFaltantesDeCajas", model);
				}
				return PartialView("_partial_GridFaltantesDeCajas", ObtenerGridCoreSmart<FaltanteDeCajaListaDto>(new List<FaltanteDeCajaListaDto>()));
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

		#region Metodos Privados
		private void CargarDatosIniciales(VentasFalDeCajaConsultaModel model)
		{
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.Desde = DateTime.Now.AddMonths(-1);
			model.Hasta = DateTime.Now;
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);

		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
