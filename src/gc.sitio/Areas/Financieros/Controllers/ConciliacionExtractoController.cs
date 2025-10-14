using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ConciliacionExtractoController : ConciliacionExtractoControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public ConciliacionExtractoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ConciliacionExtractoController> logger,
											  IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroConciliacionExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONCILIACIÓN DE EXTRACTO";
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

		public IActionResult CargarDatosExtractoYSistema(FinancieroConciliaDatosRequest request)
		{
			var model = new CargarDatosExtractoYSistemaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var regs = _financieroServicio.GetFinancieroConciliaDatos(request, TokenCookie);
				if (regs==null || regs.Count<=0)
					return PartialView("_grillaExtractoBancario", model);

				var resultado1 = ValidadorJson.ValidarExtracto(regs[0].json_e);
				var resultado2 = ValidadorJson.ValidarSistema(regs[0].json_s);
				var cuenta = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id).ToList();
				
				if (!resultado1.EsValido || !resultado2.EsValido)
				{
					return BadRequest("Se ha producido un error interno al intentar obtener los datos.");
				}
				
				if (cuenta != null && cuenta.Count() > 0)
					model.CuentaBanco = $"{cuenta[0].ctaf_denominacion} ({cuenta[0].ctaf_id})";

				model.Extracto = 0;
				model.Diferencia = 0;
				model.Sistema = 0;
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(resultado2.GrillaSistema ?? []);
				Console.WriteLine($"json_extracto: {regs[0].json_e}");
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(resultado1.GrillaExtracto ?? []);
				Console.WriteLine($"json_sistema: {regs[0].json_s}");
				return PartialView("_datosExtractoYSistema", model);
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

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		#region Útiles
		public class ResultadoValidacionJson
		{
			public bool EsValido => !Errores.Any();
			public List<string> Errores { get; set; } = new();
			public List<RegistroExtractoDto>? GrillaExtracto { get; set; }
			public List<RegistroSistemaDto>? GrillaSistema { get; set; }
		}

		public static class ValidadorJson
		{
			public static ResultadoValidacionJson ValidarExtracto(string json)
			{
				var resultado = new ResultadoValidacionJson();

				try
				{
					var registros = JsonConvert.DeserializeObject<List<RegistroExtractoDto>>(json);
					if (registros == null || registros.Count == 0)
					{
						resultado.Errores.Add("JSON vacío o inválido.");
						return resultado;
					}

					for (int i = 0; i < registros.Count; i++)
					{
						var r = registros[i];
						if (string.IsNullOrWhiteSpace(r.ctaf_id)) resultado.Errores.Add($"[{i}] ctaf_id vacío.");
						if (r.ext_fecha == default) resultado.Errores.Add($"[{i}] ext_fecha inválido.");
						if (r.ext_fecha_movi == default) resultado.Errores.Add($"[{i}] ext_fecha_movi inválido.");
						if (string.IsNullOrWhiteSpace(r.extr_id)) resultado.Errores.Add($"[{i}] extr_id vacío.");
						if (string.IsNullOrWhiteSpace(r.extr_desc)) resultado.Errores.Add($"[{i}] extr_desc vacío.");
						if (r.concepto == null) resultado.Errores.Add($"[{i}] concepto nulo.");
						if (string.IsNullOrWhiteSpace(r.ct_tipo)) resultado.Errores.Add($"[{i}] ct_tipo vacío.");
						if (string.IsNullOrWhiteSpace(r.conciliado)) resultado.Errores.Add($"[{i}] conciliado vacío.");
						if (r.a_cociliar == null) resultado.Errores.Add($"[{i}] a_cociliar nulo.");
						if (r.a_cociliar_tipo == null) resultado.Errores.Add($"[{i}] a_cociliar_tipo nulo.");
					}

					resultado.GrillaExtracto = registros;
				}
				catch (Exception ex)
				{
					resultado.Errores.Add($"Error al deserializar: {ex.Message}");
				}
				
				return resultado;
			}

			public static ResultadoValidacionJson ValidarSistema(string json)
			{
				var resultado = new ResultadoValidacionJson();

				try
				{
					var registros = JsonConvert.DeserializeObject<List<RegistroSistemaDto>>(json);
					if (registros == null || registros.Count == 0)
					{
						resultado.Errores.Add("JSON vacío o inválido.");
						return resultado;
					}

					for (int i = 0; i < registros.Count; i++)
					{
						var r = registros[i];
						if (string.IsNullOrWhiteSpace(r.ctaf_id)) resultado.Errores.Add($"[{i}] ctaf_id vacío.");
						if (r.cf_fecha_concilia == default) resultado.Errores.Add($"[{i}] cf_fecha_concilia inválido.");
						if (string.IsNullOrWhiteSpace(r.dia_movi)) resultado.Errores.Add($"[{i}] dia_movi vacío.");
						if (string.IsNullOrWhiteSpace(r.cf_compte)) resultado.Errores.Add($"[{i}] cf_compte vacío.");
						if (r.cf_item <= 0) resultado.Errores.Add($"[{i}] cf_item inválido.");
						if (string.IsNullOrWhiteSpace(r.tco_id)) resultado.Errores.Add($"[{i}] tco_id vacío.");
						if (r.concepto == null) resultado.Errores.Add($"[{i}] concepto nulo.");
						if (string.IsNullOrWhiteSpace(r.ct_tipo)) resultado.Errores.Add($"[{i}] ct_tipo vacío.");
						if (string.IsNullOrWhiteSpace(r.conciliado)) resultado.Errores.Add($"[{i}] conciliado vacío.");
						if (r.a_cociliar == null) resultado.Errores.Add($"[{i}] a_cociliar nulo.");
						if (r.a_cociliar_tipo == null) resultado.Errores.Add($"[{i}] a_cociliar_tipo nulo.");
					}

					resultado.GrillaSistema = registros;
				}
				catch (Exception ex)
				{
					resultado.Errores.Add($"Error al deserializar: {ex.Message}");
				}

				return resultado;
			}
		}
		#endregion

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroConciliacionExtractoModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			ListaCuentaBancos = ctfLista;
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();
		}
		#endregion
	}
}
