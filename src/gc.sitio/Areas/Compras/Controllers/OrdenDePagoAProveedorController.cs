using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Helpers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OfficeOpenXml.FormulaParsing.Excel.Functions;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDePagoAProveedorController : OrdenDePagoAProveedorControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		public OrdenDePagoAProveedorController(IOrdenDePagoServicio ordenDePagoServicio, ICuentaServicio cuentaServicio, IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<OrdenDePagoAProveedorController> logger) : base(options, contexto, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "ORDEN DE PAGO A PROVEEDORES";
				CargarDatosIniciales(true);
				return View();
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

		public IActionResult ValidarProveedor(string cta_id)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var res = _ordenDePagoServicio.GetOPValidacionesPrev(cta_id, TokenCookie);
				OPValidacionPrevLista = res;
				var model = ObtenerGridCoreSmart<OPValidacionPrevDto>(res);
				return PartialView("_grillaValidacionesPrev", model);
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
		public JsonResult CancelarDesdeGrillaDeValidacionesPrevias()
		{
			try
			{
				OPValidacionPrevLista = [];
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar Cancelar la Validación previa" });
			}
		}

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
		}
		#endregion
	}
}
