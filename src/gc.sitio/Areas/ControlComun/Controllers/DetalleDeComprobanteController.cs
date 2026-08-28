using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.ControlComun.Models;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class DetalleDeComprobanteController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IConsultasServicio _consultasServicio;
		public DetalleDeComprobanteController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<DetalleDeComprobanteController> logger,
											  IConsultasServicio consultasServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_consultasServicio = consultasServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AbrirComponente(DetalleDeComprobanteRequest request)
		{
			RespuestaGenerica<EntidadBase> response = new();
			var model = new DetalleDeCompteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var cab = _consultasServicio.BuscarDetalleDeComprobanteCab(request, TokenCookie);
				if (cab == null || cab.Count <= 0)
				{
					response.Mensaje = $"No se encontró comprobante. Tipo: {request.tco_id} Comprobante: {request.cm_compte} Mov: {request.dia_movi}";
					response.Ok = false;
					response.EsWarn = true;
					response.EsError = false;
					return PartialView("_gridMensaje", response);
				}
				var cabModel = new DetalleDeCompteCabModel();
				MapperCab(cab.FirstOrDefault(), cabModel);
				var listaiva = _consultasServicio.BuscarDetalleDeComprobanteIva(request, TokenCookie);
				var listaper = _consultasServicio.BuscarDetalleDeComprobantePer(request, TokenCookie);

				FormatearDatos(cabModel);
				model.Cab = cabModel;
				model.ListIva = ObtenerGridCoreSmart<DetalleDeComprobanteIvaDto>(listaiva);
				model.ListaPer = ObtenerGridCoreSmart<DetalleDeComprobantePerDto>(listaper);

				return View("~/areas/ControlComun/views/DetalleDeComprobante/Index.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{
				string msg = "Error en la obtención de la configuración para el componente.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		#region Metodos Privados


		private void MapperCab(DetalleDeComprobanteCabDto dto, DetalleDeCompteCabModel model)
		{
			if (dto == null)
				return;

			model.adm_id = dto.adm_id;
			model.afip_desc = dto.afip_desc;
			model.tco_desc = dto.tco_desc;
			model.tco_letra = dto.tco_letra;
			model.cm_cae = dto.cm_cae;
			model.cm_cuit = dto.cm_cuit;
			model.cm_nombre = dto.cm_nombre;
			model.cm_total = dto.cm_total;
			model.afip_id = dto.afip_id;
			model.cm_domicilio = dto.cm_domicilio;
			model.cm_exento = dto.cm_exento;
			model.cm_fecha = dto.cm_fecha;
			model.cm_gravado = dto.cm_gravado;
			model.cm_cae_vto = dto.cm_cae_vto;
			model.cm_ii	= dto.cm_ii;
			model.cm_iva = dto.cm_iva;
			model.cm_libro_iva = dto.cm_libro_iva;
			model.cm_percepciones = dto.cm_percepciones;
			model.usu_id = dto.usu_id;
			model.tco_id = dto.tco_id;
			model.mon_codigo = dto.mon_codigo;
			model.dia_movi = dto.dia_movi;
			model.cta_id = dto.cta_id;
			model.cm_compte = dto.cm_compte;
		}

		private static void FormatearDatos(DetalleDeCompteCabModel model)
		{
			if (!string.IsNullOrWhiteSpace(model.cm_cuit) && model.cm_cuit.Length == 11)
				model.cm_cuit = $"{model.cm_cuit.Substring(0, 2)}-{model.cm_cuit.Substring(2, 8)}-{model.cm_cuit.Substring(10, 1)}";
			if (string.IsNullOrWhiteSpace(model.cm_libro_iva) || model.cm_libro_iva.Length == 6)
				model.cm_libro_iva = $"{model.cm_libro_iva.Substring(0, 4)}-{model.cm_libro_iva.Substring(4, 2)}";
		}

		#endregion


	}
}
