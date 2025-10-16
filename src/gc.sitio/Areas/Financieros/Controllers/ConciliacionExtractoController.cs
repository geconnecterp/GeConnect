using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
				if (regs == null || regs.Count <= 0)
					return PartialView("_grillaExtractoBancario", model);

				var resultado1 = ValidadorJson.ValidarExtracto(regs[0].json_e);
				var resultado2 = ValidadorJson.ValidarSistema(regs[0].json_s);
				var cuenta = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id).ToList();

				if (resultado1.JsonVacio && resultado2.JsonVacio)
					return BadRequest("No se han obtenido resultados con los filtros seleccionados.");

				if (!resultado1.EsValido && !resultado2.EsValido)
				{
					var errores = string.Join(", ", resultado1.Errores.Distinct());
					errores += string.Join(", ", resultado2.Errores.Distinct());
					return BadRequest($"Se ha producido un error interno al intentar obtener los datos. Errores: {errores}");
				}

				if (cuenta != null && cuenta.Count() > 0)
					model.CuentaBanco = $"{cuenta[0].ctaf_denominacion} ({cuenta[0].ctaf_id})";

				model.Extracto = 0;
				model.Diferencia = 0;
				model.Sistema = 0;
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(resultado2.GrillaSistema ?? []);
				ListaItemsSistema = resultado2.GrillaSistema ?? [];
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(resultado1.GrillaExtracto ?? []);
				ListaItemsExtracto = resultado1.GrillaExtracto ?? [];
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

		public IActionResult ObtenerModalRegistrosConciliados(FinancieroConciliaNrosRequest request)
		{
			var model = new ModalRegistrosConciliadosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var resultado = _financieroServicio.GetFinancieroConciliaNro(request, TokenCookie);
				if (resultado == null || resultado.Count <= 0)
					return PartialView("_grillaExtractoBancario", model);

				var resultado1 = ValidadorJson.ValidarExtracto(resultado[0].json_e, false);
				var resultado2 = ValidadorJson.ValidarSistema(resultado[0].json_s, false);

				model.RegistroConciliado = $"Registro Conciliado N° {request.conciliado_nro}";
				model.ConciliadoNro = request.conciliado_nro;
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(resultado1.GrillaExtracto ?? []);
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(resultado2.GrillaSistema ?? []);
				return PartialView("_modalRegistrosConciliados", model);
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

		public IActionResult ObtenerModalRegistrosAConciliar()
		{
			var model = new ModalRegistrosConciliadosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.RegistroConciliado = $"Registros A Conciliar";
				var listaTempExtracto = ListaItemsExtracto.Where(x => x.a_conciliar == "S").ToList();
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(listaTempExtracto ?? []);
				var listaTempSistema = ListaItemsSistema.Where(x => x.a_conciliar == "S").ToList();
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(listaTempSistema ?? []);
				return PartialView("_modalRegistrosAConciliar", model);
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

		public JsonResult DesconciliarRegistrosConciliados(FinancieroExtractoDesconciliaRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = "Request vacío" });

				var respuesta = _financieroServicio.FinancieroExtractoDesconcilia(request, TokenCookie);
				if (respuesta == null || respuesta.Entidad == null)
					return Json(new { error = true, warn = false, msg = "Se ha producido un error al intentar desconciliar los registros. Intente mas tarde." });
				if (respuesta.Entidad.resultado != 0)
					return Json(new { error = true, warn = false, msg = $"{respuesta.Entidad.resultado_msj} ({respuesta.Entidad.resultado})" });
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

		public IActionResult ActualizarRegistrosLuegoDeDesconciliar(string ctaf_id, int conciliado_nro)
		{
			var model = new CargarDatosExtractoYSistemaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaTempExtracto = ListaItemsExtracto;
				foreach (var item in listaTempExtracto.Where(x => x.conciliado_nro == conciliado_nro))
				{
					item.conciliado = "N";
					item.conciliado_nro = null;
				}
				ListaItemsExtracto = listaTempExtracto;

				var listaTempSistema = ListaItemsSistema;
				foreach (var item in listaTempSistema.Where(x => x.conciliado_nro == conciliado_nro))
				{
					item.conciliado = "N";
					item.conciliado_nro = null;
				}
				ListaItemsSistema = listaTempSistema;
				model.CuentaBanco = ListaCuentaBancos.Where(x => x.ctaf_id == ctaf_id).Select(x => $"{x.ctaf_denominacion} ({x.ctaf_id})").FirstOrDefault() ?? string.Empty;
				model.Extracto = 0;
				model.Diferencia = 0;
				model.Sistema = 0;
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(ListaItemsSistema);
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(ListaItemsExtracto);
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

		public IActionResult ActuRegsPorConciliacionPreviaManual(FinancieroActuRegsPorConciPrevManualRequest request)
		{
			var model = new CargarDatosExtractoYSistemaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
					return BadRequest("Request vacío");
				if ((request.itemsExtractoMarcados == null || request.itemsExtractoMarcados.Count <= 0) && (request.itemsSistemaMarcados == null || request.itemsSistemaMarcados.Count <= 0))
					return BadRequest("No se han seleccionado registros del extracto y sistema para conciliar.");

				var maxExtracto = ListaItemsExtracto.Max(x => x.conciliado_nro) + 1;
				var listaTempExtracto = ListaItemsExtracto;
				var extracto = 0.00M;
				foreach (var item in listaTempExtracto.Where(x => request.itemsExtractoMarcados.Contains(x.orden)))
				{
					item.a_conciliar = "S";
					item.a_conciliar_tipo = "M";
					item.conciliado_nro = maxExtracto;
					extracto += item.importe;
				}
				ListaItemsExtracto = listaTempExtracto;

				var maxSistema = ListaItemsSistema.Max(x => x.conciliado_nro) + 1;
				var listaTempSistema = ListaItemsSistema;
				var sistema = 0.00M;
				foreach (var item in listaTempSistema.Where(x => request.itemsSistemaMarcados.Contains(x.orden)))
				{
					item.a_conciliar = "S";
					item.a_conciliar_tipo = "M";
					item.conciliado_nro = maxSistema;
					sistema += item.importe;
				}
				ListaItemsSistema = listaTempSistema;

				model.CuentaBanco = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id).Select(x => $"{x.ctaf_denominacion} ({x.ctaf_id})").FirstOrDefault() ?? string.Empty;
				model.Extracto = 0.00M;
				model.Sistema = 0.00M;
				model.Diferencia = 0.00M;
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(ListaItemsSistema);
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(ListaItemsExtracto);
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

		public IActionResult DesunirConciliacionPreviaManual(string ctaf_id)
		{
			var model = new CargarDatosExtractoYSistemaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaTempExtracto = ListaItemsExtracto;
				foreach (var item in listaTempExtracto.Where(x => x.a_conciliar == "S"))
				{
					item.a_conciliar = "N";
					item.a_conciliar_tipo = "";
					item.conciliado_nro = null;
				}
				ListaItemsExtracto = listaTempExtracto;

				var listaTempSistema = ListaItemsSistema;
				foreach (var item in listaTempSistema.Where(x => x.a_conciliar == "S"))
				{
					item.a_conciliar = "N";
					item.a_conciliar_tipo = "";
					item.conciliado_nro = null;
				}
				ListaItemsSistema = listaTempSistema;

				model.CuentaBanco = ListaCuentaBancos.Where(x => x.ctaf_id == ctaf_id).Select(x => $"{x.ctaf_denominacion} ({x.ctaf_id})").FirstOrDefault() ?? string.Empty;
				model.Extracto = 0;
				model.Sistema = 0;
				model.Diferencia = 0;
				model.GrillaSistema = ObtenerGridCoreSmart<RegistroSistemaDto>(ListaItemsSistema);
				model.GrillaExtracto = ObtenerGridCoreSmart<RegistroExtractoDto>(ListaItemsExtracto);
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

		public JsonResult ValidarAntesDeConfirmar()
		{
			try
			{
				if ((ListaItemsExtracto == null || ListaItemsExtracto.Count <= 0) && (ListaItemsSistema == null || ListaItemsSistema.Count <= 0))
					return Json(new { error = true, warn = false, msg = "No existen datos para realizar la conciliación del extracto." });
				if (ListaItemsExtracto.Count(x => x.a_conciliar == "S") <= 0 && ListaItemsSistema.Count(x => x.a_conciliar == "S") <= 0)
					return Json(new { error = true, warn = false, msg = "No existen datos para realizar la conciliación del extracto." });
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

		public JsonResult FinancieroConciliacionExtractoConfirmar(string ctaf_id)
		{
			try
			{
				if (ctaf_id == null)
					return Json(new { error = true, warn = false, msg = "Request vacío" });

				var request = new FinancieroConciliacionExtractoConfirmarRequest
				{
					ctaf_id = ctaf_id,
					usu_id = UserName,
					adm_id = AdministracionId,
					json_e = JsonConvert.SerializeObject(ListaItemsExtracto.Where(x => x.a_conciliar == "S").ToList()),
					json_s = JsonConvert.SerializeObject(ListaItemsSistema.Where(x => x.a_conciliar == "S").ToList())
				};
				Console.WriteLine($"json_e: {request.json_e}");
				Console.WriteLine($"json_s: {request.json_s}");
				Console.WriteLine($"usu_id: {request.usu_id}");
				Console.WriteLine($"adm_id: {request.adm_id}");
				Console.WriteLine($"ctaf_id: {request.ctaf_id}");
				var respuesta = _financieroServicio.FinancieroConciliacionExtractoConfirmar(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La conciliación del extracto se realizó con éxito.");
				//return Json(new { error = false, warn = false, msg = "" });
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

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaItemsExtracto = [];
				ListaItemsSistema = [];
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
			public bool JsonVacio { get; set; }
			public List<string> Errores { get; set; } = new();
			public List<RegistroExtractoDto>? GrillaExtracto { get; set; }
			public List<RegistroSistemaDto>? GrillaSistema { get; set; }
		}

		public static class ValidadorJson
		{
			public static ResultadoValidacionJson ValidarExtracto(string json, bool validarEstructura = true)
			{
				var resultado = new ResultadoValidacionJson();

				try
				{
					if (string.IsNullOrEmpty(json))
					{
						resultado.Errores.Add("JSON vacío.");
						resultado.JsonVacio = true;
						return resultado;
					}

					var registros = JsonConvert.DeserializeObject<List<RegistroExtractoDto>>(json);
					if (!validarEstructura)
					{
						resultado.GrillaExtracto = registros;
						return resultado;
					}

					if (registros == null || registros.Count == 0)
					{
						resultado.Errores.Add("JSON vacío o inválido.");
						return resultado;
					}
					var orden = 0;
					registros.ForEach(x => x.orden = ++orden);
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
						if (r.a_conciliar == null) resultado.Errores.Add($"[{i}] a_cociliar nulo.");
						if (r.a_conciliar_tipo == null) resultado.Errores.Add($"[{i}] a_cociliar_tipo nulo.");
					}

					resultado.GrillaExtracto = registros;
				}
				catch (Exception ex)
				{
					resultado.Errores.Add($"Error al deserializar: {ex.Message}");
				}

				return resultado;
			}

			public static ResultadoValidacionJson ValidarSistema(string json, bool validarEstructura = true)
			{
				var resultado = new ResultadoValidacionJson();

				try
				{
					if (string.IsNullOrEmpty(json))
					{
						resultado.Errores.Add("JSON vacío.");
						resultado.JsonVacio = true;
						return resultado;
					}

					var registros = JsonConvert.DeserializeObject<List<RegistroSistemaDto>>(json);
					if (!validarEstructura)
					{
						resultado.GrillaSistema = registros;
						return resultado;
					}

					if (registros == null || registros.Count == 0)
					{
						resultado.Errores.Add("JSON vacío o inválido.");
						return resultado;
					}
					var orden = 0;
					registros.ForEach(x => x.orden = ++orden);
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
						if (r.a_conciliar == null) resultado.Errores.Add($"[{i}] a_cociliar nulo.");
						if (r.a_conciliar_tipo == null) resultado.Errores.Add($"[{i}] a_cociliar_tipo nulo.");
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
