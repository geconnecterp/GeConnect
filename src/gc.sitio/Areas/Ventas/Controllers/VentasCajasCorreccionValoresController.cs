using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionValores;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.ABM;
using gc.sitio.core.Servicios.Contratos.Cajas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Query.Internal;
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
		private readonly IABMMedioDePagoServicio _iABMMedioDePagoServicio;
		private readonly IBancoServicio _bancoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		public VentasCajasCorreccionValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCorreccionValoresController> logger,
													  ICajaServicio cajaServicio, IAdministracionServicio administracionServicio,
													  IApiVentasServicio apiVentasServicio, ITipoCuentaFinServicio tipoCuentaServicio,
													  IABMMedioDePagoServicio iABMMedioDePagoServicio, IBancoServicio bancoServicio,
													  ICuentaServicio cuentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_iCajaSrv = cajaServicio;
			_administracionServicio = administracionServicio;
			_apiVentasServicio = apiVentasServicio;
			_cuentaServicio = cuentaServicio;
			_iABMMedioDePagoServicio = iABMMedioDePagoServicio;
			_tipoCuentaServicio = tipoCuentaServicio;
			_bancoServicio = bancoServicio;
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
				{
					model.GrillaVtasPVCtlCierres = ObtenerGridCoreSmart<VtasPVCtlCierresDto>([]);
					VtasPVCtlCierresLista = [];
				}
				else
				{
					model.GrillaVtasPVCtlCierres = ObtenerGridCoreSmart<VtasPVCtlCierresDto>(resultado.ListaEntidad ?? []);
					VtasPVCtlCierresLista = resultado.ListaEntidad ?? [];
				}

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
					json_rend = json,
					app = "CTL"
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
				PrintProperties(request);
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

		public JsonResult ConfirmacionContable(string caja_nro_proceso, int caja_nro_cierre)
		{
			try
			{
				var lista = VtasPVCtlCierresLista;
				if (lista == null || lista.Count<=0)
					throw new NegocioException("Error al obtener los datos de cierres para confirmación contable.");

				var dictRespuestas = new Dictionary<string, RespuestaGenerica<RespuestaDto>>();
				foreach (var item in lista)
				{
					var request = new ConfirmacionContableRequest()
					{
						caja_nro_proceso = item.caja_nro_proceso,
						caja_nro_cierre = item.caja_nro_cierre,
						adm_id = AdministracionId,
						usu_id = UserName
					};
					var resultado = _apiVentasServicio.ConfirmacionContable(request, TokenCookie).Result;
					dictRespuestas[item.caja_nro_cierre.ToString()] = resultado;
				}

				// Evaluar fallos
				var fallidos = dictRespuestas
					.Where(x =>
						x.Value == null ||
						!x.Value.Ok ||
						x.Value.EsError ||
						(x.Value.Entidad != null && x.Value.Entidad.resultado != 0)
					)
					.Select(x => x.Value?.Mensaje
									?? x.Value?.Entidad?.resultado_msj
									?? "Error desconocido")
					.Distinct() // 👈 SOLO MENSAJES ÚNICOS
					.ToList();

				// Si todos OK
				if (fallidos.Count == 0)
				{
					return Json(new
					{
						Ok = true,
						error = false,
						warn = false,
						msg = "Todas las entregas fueron confirmadas correctamente",
						respuestas = dictRespuestas
					});
				}

				// Si hubo fallos
				return Json(new
				{
					Ok = false,
					error = true,
					warn = false,
					msg = "Errores en la confirmación contable",
					errores = fallidos   
				});
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, msg = ex.Message });
			}
		}

		[HttpPost]
		public IActionResult ObtenerPartialDeValores(string tcf_id, string ins_id, int rend_item, string nro_proceso, int nro_cierre, int nro_rend)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (string.IsNullOrWhiteSpace(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");

				var item = new VtasPVCtlRendDetalleDto();
				if (!string.IsNullOrWhiteSpace(nro_proceso) && !string.IsNullOrEmpty(ins_id))
					item = VtasPVCtlRendDetalleLista.Where(x=>x.ins_id == ins_id && x.caja_nro_proceso == nro_proceso && x.caja_nro_cierre == nro_cierre && x.caja_nro_rend == nro_rend && x.rend_item == rend_item).FirstOrDefault();

				string partialName = $"_partial_modal_{tcf_id}";
				var model = ObtenerModelDesdeTcf_id(tcf_id);
				model.Item = item;
				return PartialView(partialName, model);
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
		public IActionResult ObtenerPlazaPorBanco(string bc_id)
		{
			if (string.IsNullOrWhiteSpace(bc_id))
				return Json(new { ok = false, mensaje = "bc_id inválido" });

			// Aquí obtenés la lista desde tu servicio o repositorio
			List<ABMChequeListaDto> lista = ChequesLista;

			var item = lista.FirstOrDefault(x => x.bc_id == bc_id);

			if (item == null)
				return Json(new { ok = false, mensaje = "Banco no encontrado" });

			return Json(new { ok = true, data = item });
		}

		[HttpPost]
        public JsonResult BuscarClientes(string prefix)
        {
            var top = ClientesLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            var tipos = top.Select(x => new
            {
                Id = x.Cta_Id,
                Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})",
                TipoDesc = x.Tipo_Desc,
                Tipo = x.Tipo
            });
            return Json(tipos);
        }

		[HttpPost]
		public JsonResult ActualizarItemConceptoValorEnDetalleRend(ConceptoValorDesdeCorreccionVtaPVDto detalle, string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, string tcf_id, string ins_id, string ins_detalle, int rend_item)
		{
			var msg = string.Empty;
			try
			{
				if (string.IsNullOrEmpty(caja_nro_proceso))
				{
					msg = "Faltan datos obligatorios: nro_proceso";
					throw new NegocioException(msg);
				}
				if (caja_nro_cierre <= 0)
				{
					msg = "Faltan datos obligatorios: nro_cierre";
					throw new NegocioException(msg);
				}
				if (caja_nro_rend <= 0)
				{
					msg = "Faltan datos obligatorios: caja_nro_rend";
					throw new NegocioException(msg);
				}
				if (VtasPVCtlRendDetalleLista == null || VtasPVCtlRendDetalleLista.Count <= 0)
				{
					msg = $"Error al obtener los datos del detalle de la rendicion seleccionada. Nro. Proceso: {caja_nro_proceso} Cierre: {caja_nro_cierre} Rendición: {caja_nro_rend}";
					throw new NegocioException(msg);
				}

				//var item = new VtasPVCtlRendDetalleDto();
				//var listaTempo = VtasPVCtlRendDetalleLista;
				//var listaFiltrada = listaTempo.Where(x => x.caja_nro_proceso == caja_nro_proceso && x.caja_nro_cierre == caja_nro_cierre && x.caja_nro_rend == caja_nro_rend).ToList();
				//if (rend_item != 0)
				//	item = listaFiltrada.Where(x => x.rend_item == rend_item).First();
				//else
				//	item = listaFiltrada.First();
				//ObtenerConceptoValor(item, detalle);
				var lista = VtasPVCtlRendDetalleLista;

				var item = lista
					.Where(x => x.caja_nro_proceso == caja_nro_proceso &&
								x.caja_nro_cierre == caja_nro_cierre &&
								x.caja_nro_rend == caja_nro_rend)
					.FirstOrDefault(x => rend_item == 0 || x.rend_item == rend_item);

				if (item == null)
					throw new Exception("No se encontró el item para actualizar.");

				ObtenerConceptoValor(item, detalle);

				// Guardar cambios
				VtasPVCtlRendDetalleLista = lista;
				return Json(new { Ok = true, error = false, msg = "", concepto = item.concepto_valor });
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, msg = ex.Message });
			}
		}

		#region Métodos Privados

		private void ObtenerConceptoValor(VtasPVCtlRendDetalleDto source, ConceptoValorDesdeCorreccionVtaPVDto detalle)
		{
			var dato1_valor = string.IsNullOrWhiteSpace(detalle.op_dato1_valor) && string.IsNullOrWhiteSpace(detalle.op_dato1_desc) ? string.Empty : detalle.op_dato1_valor;
			var dato2_valor = string.IsNullOrWhiteSpace(detalle.op_dato2_valor) && string.IsNullOrWhiteSpace(detalle.op_dato2_desc) ? string.Empty : $"{detalle.op_dato2_desc}:{detalle.op_dato2_valor}";
			var dato3_valor = string.IsNullOrWhiteSpace(detalle.op_dato3_valor) && string.IsNullOrWhiteSpace(detalle.op_dato3_desc) ? string.Empty : $"{detalle.op_dato3_desc}:{detalle.op_dato3_valor}";
			var dato_fecha = detalle.op_fecha_valor != null ? $"Fecha Val.:{detalle.op_fecha_valor.Value:dd/MM/yyyy}" : string.Empty;
			source.rend_dato1_valor = detalle.op_dato1_valor;
			source.rend_dato2_valor = detalle.op_dato2_valor;
			source.rend_dato3_valor = detalle.op_dato3_valor;
			source.rend_fecha = detalle.op_fecha_valor.Value;
			source.concepto_valor = $"{dato1_valor} {dato2_valor} {dato3_valor} {dato_fecha}";
			source.rend_importe_ok = detalle.op_importe;
		}
		private IMedioDePago ObtenerModelDesdeTcf_id(string tcf_id)
		{
			switch (tcf_id)
			{
				case "EF":
					var modelEF = new MedioDePagoEFModel();
					return modelEF;
				case "TC":
					var modelTC = new MedioDePagoTCModel
					{
						ListaMediosDePago = ObtenerMediosDePago(tcf_id)
					};
					return modelTC;
				case "TD":
					var modelTD = new MedioDePagoTDModel
					{
						ListaMediosDePago = ObtenerMediosDePago(tcf_id)
					};
					return modelTD;
				case "MU":
					var modelMU = new MedioDePagoMUModel()
					{
						ListaMediosDePago = ObtenerMediosDePago(tcf_id)
					};
					return modelMU;
				case "CH":
					var modelCH = new MedioDePagoCHModel()
					{
						ListaBcoCheqs = ObtenerChequeLista(),
						FechaVto = DateTime.Now.Date
					};
					var Rel01List = new List<ComboGenDto>();
					ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
					return modelCH;
				case "BA":
					var modelBA = new MedioDePagoBAModel()
					{
						ListaMediosDePago = ObtenerMediosDePago(tcf_id)
					};
					return modelBA;
				default:
					var modelNI = new MedioDePagoNIModel();
					return modelNI;
			}
		}
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
				if (string.IsNullOrWhiteSpace(msj))
					return "El mensaje NO es JSON válido.";
				else
					return msj;
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
			if (ClientesLista.Count == 0)
			{
				var lista = _cuentaServicio.ObtenerListaCuentaComercial("%", 'C', TokenCookie).Result;
				ClientesLista = lista;
			}
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerMediosDePago(string tcf_id)
		{
			var medios = _iABMMedioDePagoServicio.ObtenerMediosDePagoLista(tcf_id, TokenCookie).Result;
			if (medios != null && medios.ListaEntidad != null && medios.ListaEntidad.Count > 0)
				return HelperMvc<ComboGenDto>.ListaGenerica(medios.ListaEntidad.Select(x => new ComboGenDto { Id = x.ins_id, Descripcion = x.ins_lista }));
			else
				return HelperMvc<ComboGenDto>.ListaGenerica([]);
		}

		private SelectList ObtenerChequeLista()
		{
			var cheques = _bancoServicio.GetBancoChequeLista(TokenCookie);
			if (cheques != null && cheques.Count > 0)
			{
				ChequesLista = cheques;
				return HelperMvc<ComboGenDto>.ListaGenerica(cheques.Select(x => new ComboGenDto { Id = x.bc_id, Descripcion = x.bc_denominacion }));
			}
			else
			{
				ChequesLista = [];
				return HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
		}
		#endregion
	}

	public class ConceptoValorDesdeCorreccionVtaPVDto : Dto
	{
		public string tcf_id { get; set; } = string.Empty;
		public string op_dato1_valor { get; set; } = string.Empty;
		public string op_dato1_desc { get; set; } = string.Empty;
		public string op_dato2_valor { get; set; } = string.Empty;
		public string op_dato2_desc { get; set; } = string.Empty;
		public string op_dato3_valor { get; set; } = string.Empty;
		public string op_dato3_desc { get; set; } = string.Empty;
		public DateTime? op_fecha_valor { get; set; }
		public decimal op_importe { get; set; } = 0.00M;
		public string? cta_id { get; set; } = string.Empty;
		public string ins_id { get; set; } = string.Empty;
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
