using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Twilio.TwiML.Voice;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class AnticiposCargaController : AnticiposCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoAnticipoEmpleadoServicio _tipoAnticipoEmpleadoServicio;
		public AnticiposCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnticiposCargaController> logger,
										IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio,
										ITipoAnticipoEmpleadoServicio tipoAnticipoEmpleadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
			_tipoAnticipoEmpleadoServicio = tipoAnticipoEmpleadoServicio;
		}

		public IActionResult Index()
		{
			var model = new CargaDeAnticiposModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGA DE ANTICIPOS";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(true);

				model.ListaTipo = ComboTipoAnticipoEmpleados();
				model.Concepto = string.Empty;
				model.porc_interes = 0;
				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>([]);

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

		public IActionResult AbrirModalAgregarAnticipo(decimal intereses, string cta_id, string cta_desc)
		{
			var model = new CargaAnticipoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.cuotas = 0;
				model.importe = 0;
				model.intereses = intereses;
				model.cta_id = cta_id;
				model.cta_desc = cta_desc;

				return PartialView("_modalCargaAnticipo", model);
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

		public JsonResult AgregarAnticipo(string cta_id, string cta_desc, int cuotas, decimal importe, int intereses)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (string.IsNullOrEmpty(cta_id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un cliente." });
				if (importe <= 0)
					return Json(new { error = true, warn = false, msg = "El importe debe ser mayor a cero." });
				if (cuotas <= 0)
					return Json(new { error = true, warn = false, msg = "La cuota debe ser mayor a 0." });

				var nuevoAnticipo = new AnticipoDto
				{
					cta_id = cta_id,
					cta_denominacion = cta_desc,
					importe = importe,
					intereses = Math.Round((importe * intereses) / 100, 2),
					valor_cuota = (importe + Math.Round((importe * intereses) / 100, 2)) / cuotas,
					valor_total = (importe + Math.Round((importe * intereses) / 100, 2))
				};
				var listaTemp = AnticiposLista;
				listaTemp.Add(nuevoAnticipo);
				AnticiposLista = listaTemp;
				
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		[HttpPost]
		public IActionResult ActualizarListaDeAnticipos()
		{
			var model = new CargaDeAnticiposModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>(AnticiposLista);
				return PartialView("_grillaAnticipos", model);
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
		public JsonResult BuscarContrapartidas(string prefix)
		{
			var top = ProveedoresLista.Where(x => x.Cta_Lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Lista });
			return Json(tipos);
		}

		[HttpPost]
		public JsonResult BuscarClientes(string prefix)
		{
			var top = ClientesLista.Where(x => x.Cta_Lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = x.Cta_Lista });
			return Json(tipos);
		}

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio, "PS");

			if (TipoAnticipoEmpleadoLista.Count == 0 || actualizar)
				ObtenerTiposAnticipoEmpleado(_tipoAnticipoEmpleadoServicio);

			if (ClientesLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
		}
		#endregion
	}
}
