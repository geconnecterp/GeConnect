using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.RelacionarComprobanteSinRP;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.RelacionarComprobanteSinRP;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Twilio.Rest.Api.V2010.Account;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class RelacionarComprobanteSinRPController : RelacionarComprobanteSinRPControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IConsultasServicio _consultasServicio;
		public RelacionarComprobanteSinRPController(ICuentaServicio cuentaServicio, IConsultasServicio consultasServicio,
													IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<RelacionarComprobanteSinRPController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_consultasServicio = consultasServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "JUSTIFICAR y/o RELACIONAR COMPROBANTE SIN RP";
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

		[HttpPost]
		public IActionResult InicializarComprobantes(string ctaId)
		{
			var model = new RelacionarComprobanteSinRPModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrEmpty(ctaId))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar una cuenta." });

				var listaComprobantes = _cuentaServicio.GetCompteJbi(ctaId, TokenCookie);
				model.GrillaComprobantes = ObtenerGridCoreSmart<CompteJbiDto>(listaComprobantes);

				var listaRP = _cuentaServicio.GetCompteCargaRprAsoc(ctaId, TokenCookie);
				var listaRpMapped = MappRP(listaRP);
				model.GrillaRP = ObtenerGridCoreSmart<CompteRPDto>(listaRpMapped);

				return PartialView("_tabRelacionarComprobanteSinRP", model);
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
		public async Task<IActionResult> InicializarDetalleRP(string compteId)
		{
			GridCoreSmart<ConsRecepcionProveedorDetalleDto> grillaDatos;
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				string sort = string.Empty;
				string sortDir = string.Empty;
				int pag = 1;
				var detalle = await _consultasServicio.ConsultaRecepcionProveedorDetalle(compteId, TokenCookie);
				grillaDatos = GenerarGrillaSmart(detalle.ListaEntidad, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);

				return PartialView("_gridRecProvDet", grillaDatos);
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
		public JsonResult ConfirmarJustificacion(ConfirmarJustificacionAuxiliarRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = "No se recibieron los datos necesarios para confirmar la justificación." });

				if (string.IsNullOrEmpty(request.cta_id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar una cuenta." });

				if (request.comprobantes == null || request.comprobantes.Count == 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar al menos un comprobante para justificar." });

				var jsonComptes = JsonConvert.SerializeObject(request.comprobantes, new JsonSerializerSettings());
				var jsonRps = request.rps != null ? JsonConvert.SerializeObject(request.rps, new JsonSerializerSettings()) : JsonConvert.SerializeObject(new List<RpParaJustificar>(), new JsonSerializerSettings());
				Console.WriteLine($"cta_id: {request.cta_id}");
				Console.WriteLine($"adm_id: {AdministracionId}");
				Console.WriteLine($"usu_id: {UserName}");
				Console.WriteLine($"cta_id: {request.cta_id}");
				Console.WriteLine($"json_comptes: {jsonComptes}");
				Console.WriteLine($"json_rp: {jsonRps}");
				var response = _cuentaServicio.ConfirmaCompteJbi(new ConfirmarJustificacionRequest() { cta_id = request.cta_id, adm_id = AdministracionId, usu_id = UserName, json_comptes = jsonComptes, json_rp = jsonRps }, TokenCookie);
				return AnalizarRespuesta(response, "La Justificación se realizó con Éxito");
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar confirmar los datos - CONFIRMARJUSTIFICACION" });
			}
		}

		[HttpPost]
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - RELACIONARCOMPROBANTESINRP" });
			}
		}

		#region Métodos Privados
		private List<CompteRPDto> MappRP(List<RprAsociadosDto> listaRpr)
		{
			return [.. listaRpr.Select(r => new CompteRPDto
			{
				tco_id_rp = r.tco_id_rp,
				tco_desc_rp = r.tco_desc_rp,
				cm_compte_rp = r.cm_compte_rp,
				justificado = 'N',
				justificado_bool = false,
				cm_fecha_rp = r.cm_fecha_rp,
				adm_id = r.adm_id,
				cm_importe_rp = r.cm_importe_rp,
				rpe_id = r.rpe_id,
				rpe_desc = r.rpe_desc,
				rp_compte = r.rp_compte,
				rp_fecha = r.rp_fecha,
				cta_id = r.cta_id,
				usu_id = r.usu_id,
				dia_movi = r.dia_movi,
				adm_nombre = r.adm_nombre,
			})];
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
		}
		#endregion
	}
}
