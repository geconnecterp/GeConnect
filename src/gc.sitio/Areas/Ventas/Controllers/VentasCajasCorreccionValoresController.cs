using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentasCajasCorreccionValoresController : VentasCajasCorreccionValoresControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICajaServicio _iCajaSrv;
		private readonly IApiVentasServicio _apiVentasServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ITipoCuentaFinServicio _tipoCuentaServicio;
		public VentasCajasCorreccionValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCorreccionValoresController> logger,
													  ICajaServicio cajaServicio, IAdministracionServicio administracionServicio,
													  IApiVentasServicio apiVentasServicio, ITipoCuentaFinServicio tipoCuentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
			_administracionServicio = administracionServicio;
			_apiVentasServicio = apiVentasServicio;
			_tipoCuentaServicio = tipoCuentaServicio;
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

		[HttpPost]
		public async Task<IActionResult> CargarDatosDeCierres(string admDesc, string admId, string nroProceso)
		{
			var model = new InitializeViewModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(nroProceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlCierresLista(nroProceso, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de corrección");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de corrección");
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
					model.GrillaVtasPVCtlCierres = ObtenerGridCoreSmart<VtasPVCtlCierresDto>([]);
				else
					model.GrillaVtasPVCtlCierres = ObtenerGridCoreSmart<VtasPVCtlCierresDto>(resultado.ListaEntidad ?? []);

				model.GrillaVtasPVCtlRend = ObtenerGridCoreSmart<VtasPVCtlRendDto>([]);
				model.GrillaVtasPVCtlRendDetalle = ObtenerGridCoreSmart<VtasPVCtlRendDetalleDto>([]);
				model.Sucursal = admDesc;
				model.Fecha = VtasPVCtlProcesoLista.FirstOrDefault(x => x.caja_nro_proceso == nroProceso)?.caja_habilitacion.ToString("dd/MM/yyyy") ?? string.Empty;
				model.NroProceso = nroProceso;
				return PartialView("_datos_correccion", model);
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
		public async Task<IActionResult> ObtenerRendDeCierreSeleccionado(string nro_proceso, int nro_cierre)
		{
			var model = new GridCoreSmart<VtasPVCtlRendDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlRendLista(nro_proceso, nro_cierre, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de rendición de cierre");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de rendición de cierre");
				model = ObtenerGridCoreSmart<VtasPVCtlRendDto>(resultado.ListaEntidad ?? []);
				return PartialView("_datos_correccion_VtasPVCtlRend", model);
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
		public async Task<IActionResult> ObtenerDetalleDeRendDeCierreSeleccionado(string nro_proceso, int nro_cierre, int caja_nro_rend, string tcf_id, bool pendiente)
		{
			var model = new GridCoreSmart<VtasPVCtlRendDetalleDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlRendDetalleLista(nro_proceso, nro_cierre, caja_nro_rend, tcf_id, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de detalle de rendición de cierre");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de detalle de rendición de cierre");
				var lista = resultado.ListaEntidad ?? [];
				if (lista != null && lista.Count > 0)
				{
					foreach (var item in lista)
						item.pendiente = pendiente;
				}
				model = ObtenerGridCoreSmart<VtasPVCtlRendDetalleDto>(lista ?? []);
				VtasPVCtlRendDetalleLista = lista ?? [];
				return PartialView("_datos_correccion_VtasPVCtlRendDetalle", model);
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
		public JsonResult ActualizarImporteEnItemDeDetalleDeArqueo(string ins_id, decimal importe)
		{
			try
			{
				var listaTemp = VtasPVCtlRendDetalleLista;
				var item = listaTemp.FirstOrDefault(x => x.ins_id == ins_id);
				if (item != null)
				{
					item.rend_importe_ok = importe;
					VtasPVCtlRendDetalleLista = listaTemp;
				}
				return Json(new { Ok = true, error = false });
			}
			catch (Exception)
			{
				return Json(new { Ok = false, error = true });
			}
		}

		[HttpPost]
		public JsonResult CargaCtlNuevoItemDetalle(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend)
		{
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (caja_nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				var request = new CargaCtlNuevoItemDetalleRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					caja_nro_rend = caja_nro_rend,
					tcf_id = "",
					nuevo_tcf = false,
					adm_id = AdministracionId,
					usu_id = UserName
				};
				var resultado = _apiVentasServicio.CargaCtlNuevoItemDetalle(request, TokenCookie).Result;
				if (resultado == null)
					throw new NegocioException("Error al cargar nuevo item de detalle");
				// Procesamiento de respuesta
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
				{
					return Json(new
					{
						ok = true,
						error = false,
					});
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error: {Mensaje}", resultado.Mensaje);
					return Json(new
					{
						ok = false,
						error = resultado.EsError,
						warn = resultado.EsWarn,
						msg = resultado.Mensaje ?? "Error"
					});
				}
			}
			catch (Exception)
			{
				return Json(new { Ok = false, error = true });
			}
		}

		[HttpPost]
		public JsonResult GuardarCtlDetalle(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (caja_nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var json = ArmarJsonDetalle();
				if (string.IsNullOrEmpty(json))
					throw new NegocioException("Error al intentar parsear productos");
				var request = new GuardarCtlDetalleRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					caja_nro_rend = caja_nro_rend,
					tcf_id = tcf_id,
					adm_id = AdministracionId,
					usu_id = UserName,
					json_rend = json
				};
				PrintProperties(request);
				var resultado = _apiVentasServicio.GuardarCtlDetalle(request, TokenCookie).Result;
				if (resultado == null)
					throw new NegocioException("Error al guardar detalle");
				// Procesamiento de respuesta
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
				{
					return Json(new
					{
						ok = true,
						error = false,
					});
				}
				else
				{
					// Log y respuesta de error/advertencia
					var msj = ObtenerMensajeDesdeError(resultado.Mensaje ?? "");
					_logger?.LogWarning("Error: {Mensaje}", msj);
					return Json(new
					{
						ok = false,
						error = true,
						warn = resultado.EsWarn,
						msg = msj
					});
				}
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, warn = false, msg = ex.Message });
			}
		}

		[HttpPost]
		public JsonResult ConfirmarCtlArqueo(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (caja_nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var request = new ConfirmarCtlArqueoRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					caja_nro_rend = caja_nro_rend,
					tcf_id = tcf_id,
					adm_id = AdministracionId,
					usu_id = UserName
				};
				var resultado = _apiVentasServicio.ConfirmarCtlArqueo(request, TokenCookie).Result;
				if (resultado == null)
					throw new NegocioException("Error al guardar detalle");
				// Procesamiento de respuesta
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
				{
					return Json(new
					{
						ok = true,
						error = false,
					});
				}
				else
				{
					var msj = ObtenerMensajeDesdeError(resultado.Mensaje ?? "");
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error: {Mensaje}", resultado.Mensaje);
					return Json(new
					{
						ok = false,
						error = true,
						warn = resultado.EsWarn,
						msg = msj
					});
				}
			}
			catch (Exception)
			{
				return Json(new { Ok = false, error = true });
			}
		}

		[HttpPost]
		public JsonResult AnularCtlArqueo(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (caja_nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var request = new AnularCtlArqueoRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					caja_nro_rend = caja_nro_rend,
					tcf_id = tcf_id,
					adm_id = AdministracionId,
					usu_id = UserName
				};
				var resultado = _apiVentasServicio.AnularCtlArqueo(request, TokenCookie).Result;
				if (resultado == null)
					throw new NegocioException("Error al guardar detalle");
				// Procesamiento de respuesta
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
				{
					return Json(new
					{
						ok = true,
						error = false,
					});
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error: {Mensaje}", resultado.Mensaje);
					return Json(new
					{
						ok = false,
						error = resultado.EsError,
						warn = resultado.EsWarn,
						msg = resultado.Mensaje ?? "Error"
					});
				}
			}
			catch (Exception)
			{
				return Json(new { Ok = false, error = true });
			}
		}

		public IActionResult AbrirModalAgregarMedioDePago()
		{
			var model = new MedioDePagoAgregarModel();
			try
			{
				var lista = _tipoCuentaServicio.ObtenerTipoCuentaFin(TokenCookie);
				var listaCombo = lista.Select(x => new ComboGenDto { Id = x.tcf_id, Descripcion = x.tcf_lista });
				model.ListaMedioDePago = HelperMvc<ComboGenDto>.ListaGenerica(listaCombo ?? []);
				return PartialView("_modalMedioDePagoAgregar", model);
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

		public JsonResult AgregarMedioDePago(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
					throw new NegocioException("Faltan datos obligatorios: nro_proceso");
				if (caja_nro_cierre <= 0)
					throw new NegocioException("Faltan datos obligatorios: nro_cierre");
				if (caja_nro_rend <= 0)
					throw new NegocioException("Faltan datos obligatorios: caja_nro_rend");
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var request = new AgregarMedioDePagoRequest()
				{
					caja_nro_proceso = caja_nro_proceso,
					caja_nro_cierre = caja_nro_cierre,
					caja_nro_rend = caja_nro_rend,
					tcf_id = tcf_id,
					adm_id = AdministracionId,
					nuevo_tcf = true,
					usu_id = UserName
				};
				var resultado = _apiVentasServicio.AgregarMedioDePago(request, TokenCookie).Result;
				if (resultado == null)
					throw new NegocioException("Error al guardar detalle");
				// Procesamiento de respuesta
				if (resultado.Ok && !resultado.EsError && !resultado.EsWarn)
				{
					return Json(new
					{
						ok = true,
						error = false,
					});
				}
				else
				{
					// Log y respuesta de error/advertencia
					_logger?.LogWarning("Error: {Mensaje}", resultado.Mensaje);
					return Json(new
					{
						ok = false,
						error = resultado.EsError,
						warn = resultado.EsWarn,
						msg = resultado.Mensaje ?? "Error"
					});
				}
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, msg = ex.Message });
			}
		}

		#region Métodos Privados
		private static string ObtenerMensajeDesdeError(string msj)
		{
			if (EsJsonValido(msj))
			{
				JObject json = ParsearJsonSeguro(msj);

				if (json != null && json["error"] != null)
				{
					var err = json["error"].First();

					int status = (int)err["status"];
					string title = (string)err["title"];
					string detail = (string)err["detail"];
					string typeException = (string)err["typeException"];

					Console.WriteLine($"Status: {status}");
					Console.WriteLine($"Title: {title}");
					Console.WriteLine($"Detail: {detail}");
					Console.WriteLine($"Type: {typeException}");
					return $"({status} {detail})";
				}
				else
					return "El mensaje NO es JSON válido.";
			}
			else
			{
				return "El mensaje NO es JSON válido.";
			}
		}

		private string ArmarJsonDetalle()
		{
			try
			{
				var propsAExcluir = new[] { "concepto_valor", "ins_detalle_bool", "pendiente" };
				var settings = new JsonSerializerSettings
				{
					ContractResolver = new IgnoreAndTrimResolver(propsAExcluir),
					Formatting = Formatting.None
				};

				string json = JsonConvert.SerializeObject(VtasPVCtlRendDetalleLista, settings);
				return json;
			}
			catch (Exception)
			{
				return string.Empty;
			}
		}

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

	public class IgnoreAndTrimResolver : DefaultContractResolver
	{
		private readonly HashSet<string> _propsAExcluir;

		public IgnoreAndTrimResolver(IEnumerable<string> propsAExcluir)
		{
			_propsAExcluir = new HashSet<string>(propsAExcluir, StringComparer.OrdinalIgnoreCase);
		}

		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			var props = base.CreateProperties(type, memberSerialization);

			// 1) Excluir propiedades
			props = props
				.Where(p => !_propsAExcluir.Contains(p.PropertyName))
				.ToList();

			// 2) Recortar strings
			foreach (var prop in props)
			{
				if (prop.PropertyType == typeof(string))
				{
					var originalProvider = prop.ValueProvider;

					prop.ValueProvider = new TrimStringValueProvider(originalProvider);
				}
			}

			return props;
		}

		class TrimStringValueProvider : IValueProvider
		{
			private readonly IValueProvider _inner;

			public TrimStringValueProvider(IValueProvider inner)
			{
				_inner = inner;
			}

			public object GetValue(object target)
			{
				var value = _inner.GetValue(target) as string;
				return value?.Trim();
			}

			public void SetValue(object target, object value)
			{
				_inner.SetValue(target, value);
			}
		}
	}

}
