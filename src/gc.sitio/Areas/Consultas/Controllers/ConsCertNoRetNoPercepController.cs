using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ConsCertNoRetNoPercepController : ConsCertNoRetNoPercepControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoImpuestoServicio _tipoImpuestoServicio;
		public ConsCertNoRetNoPercepController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsCertNoRetNoPercepController> logger,
											   ITipoImpuestoServicio tipoImpuestoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoImpuestoServicio = tipoImpuestoServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsCertNoRetNoPercepModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CERTIFICADOS DE NO RETENCIÓN NO PERCEPCIÓN";
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

		#region CargarDatosIniciales
		private void CargarDatosIniciales(ConsCertNoRetNoPercepModel model)
		{
			if (TipoImpuestoLista.Count == 0)
			{
				ObtenerTiposDeImpuestos(_tipoImpuestoServicio);
			}
			model.ListaTipoImpuesto = ComboTipoImpuestos();
			model.NoVencidos = false;
			model.Vencidos = false;
			model.CertNoPercepcion = false;
			model.CertNoRetencion = false;

			var tImpuestosList = new List<ComboGenDto>();
			ViewBag.TipoImpuestosList = HelperMvc<ComboGenDto>.ListaGenerica(tImpuestosList);
		}
		#endregion
	}
}
