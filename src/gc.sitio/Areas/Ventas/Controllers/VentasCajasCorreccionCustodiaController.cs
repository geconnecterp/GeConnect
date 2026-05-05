using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionCustodia;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentasCajasCorreccionCustodiaController : VentasCajasCorreccionCustodiaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IApiVentasServicio _apiVentasServicio;
		private readonly IAdministracionServicio _administracionServicio;

		public VentasCajasCorreccionCustodiaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCorreccionCustodiaController> logger,
													   IApiVentasServicio apiVentasServicio, IAdministracionServicio administracionServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_apiVentasServicio = apiVentasServicio;
			_administracionServicio = administracionServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroCtlCustodiaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CORRECCIÓN DE VALORES ENTREGADOS EN CUSTODIA";
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
		public async Task<IActionResult> CargarDatosDeValores(string admDesc, string admId, string tipo)
		{
			var model = new InitializeViewCusModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(admDesc))
					throw new NegocioException("Faltan datos obligatorios: Sucursal");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlEntregaLista(admId, tipo, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de valores entregados en custodia");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de valores entregados en custodia");
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
				{
					model.GrillaVtasPVCtlEntrega = ObtenerGridCoreSmart<VtasPVCtlEntregaDto>([]);
					VtasPVCtlEntregaLista = [];
				}
				else
				{
					model.GrillaVtasPVCtlEntrega = ObtenerGridCoreSmart<VtasPVCtlEntregaDto>(resultado.ListaEntidad ?? []);
					VtasPVCtlEntregaLista = resultado.ListaEntidad ?? [];
				}
				model.GrillaVtasPVCtlEntregaRend = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>([]);
				model.TipoEntrega = tipo;
				model.Sucursal = admDesc;
				return PartialView("_datos_custodia", model);
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
		public async Task<IActionResult> ObtenerRendDeEntregaSeleccionada(string ent_compte)
		{
			var model = new GridCoreSmart<VtasPVCtlEntregaRendDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(ent_compte))
					throw new NegocioException("Faltan datos obligatorios: Entrega");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlEntregaRendLista(ent_compte, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de rendición de entrega seleccionada");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de rendición de entrega seleccionada");
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
				{
					model = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>([]);
					VtasPVCtlEntregaRendLista = [];
				}
				else
				{
					var item = VtasPVCtlEntregaLista.Where(x => x.ent_compte == ent_compte).FirstOrDefault();
					var listaTempo = resultado.ListaEntidad ?? [];
					listaTempo.ForEach(x => x.ent_estado = item?.ent_estado ?? ' ');
					VtasPVCtlEntregaRendLista = listaTempo;
					model = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>(listaTempo);
				}

				return PartialView("_datos_correccion_VtasPVCtlEntregaRend", model);
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
		public IActionResult ObtenerEntregasParaCambioDeRendicion(string ent_compte)
		{
			var model = new ListaEntregasModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(ent_compte))
					throw new NegocioException("Faltan datos obligatorios: Entrega");
				var listaTempo = VtasPVCtlEntregaLista.Where(x => x.ent_compte != ent_compte).ToList();
				if (listaTempo == null || listaTempo.Count == 0)
					model.ListaEntregas = new SelectList(Enumerable.Empty<ComboGenDto>());
				else
					model.ListaEntregas = ObtenerListaEntregas(listaTempo);
				return PartialView("_partial_entregas_para_cambio_rendicion", model);
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
		public JsonResult ActualizarImporteEnItemDeDetalle(string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, int caja_rend_item, decimal importe)
		{
			try
			{
				var listaTemp = VtasPVCtlEntregaRendLista;
				var item = listaTemp.FirstOrDefault(x => x.caja_nro_proceso == caja_nro_proceso && x.caja_nro_cierre == caja_nro_cierre && x.caja_nro_rend == caja_nro_rend && x.rend_item == caja_rend_item);
				if (item != null)
				{
					item.rend_importe_ok = importe;
					item.editado = true;
					VtasPVCtlEntregaRendLista = listaTemp;
				}
				return Json(new { Ok = true, error = false });
			}
			catch (Exception)
			{
				return Json(new { Ok = false, error = true });
			}
		}

		[HttpPost]
		public JsonResult GuardarCtlDetalle(string tcf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(tcf_id))
					throw new NegocioException("Faltan datos obligatorios: tcf_id");
				var dictRespuestas = new Dictionary<string, RespuestaGenerica<RespuestaDto>>();
				var listaTemp = VtasPVCtlEntregaRendLista.Where(x => x.editado).ToList();
				var lista = MapperVtasPVCtlEntregaRendAux(listaTemp);
				foreach (var itemR in lista)
				{
					string json = JsonConvert.SerializeObject(new List<VtasPVCtlEntregaRendAux>() { itemR });
					var request = new GuardarCtlDetalleRequest()
					{
						caja_nro_proceso = itemR.caja_nro_proceso,
						caja_nro_cierre = itemR.caja_nro_cierre,
						caja_nro_rend = itemR.caja_nro_rend,
						tcf_id = tcf_id,
						adm_id = AdministracionId,
						usu_id = UserName,
						json_rend = json,
						app = "ENTREGA"
					};
					PrintProperties(request);
					var resultado = _apiVentasServicio.GuardarCtlDetalle(request, TokenCookie).Result;
					dictRespuestas[itemR.caja_nro_cierre.ToString()] = resultado;
				}
				// Evaluar fallos
				var fallidos = dictRespuestas
					.Where(x =>
						x.Value == null ||
						!x.Value.Ok ||
						x.Value.EsError ||
						(x.Value.Entidad != null && x.Value.Entidad.resultado != 0)
					)
					.Select(x => new
					{
						ent_compte = x.Key,
						mensaje = x.Value?.Mensaje
								   ?? x.Value?.Entidad?.resultado_msj
								   ?? "Error desconocido"
					})
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
					msg = "Algunas entregas no pudieron confirmarse",
					fallidos,
					respuestas = dictRespuestas
				});
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, warn = false, msg = ex.Message });
			}
		}

		

		[HttpPost]
		public JsonResult MoverCtlDetalle(string ent_compte, string tcf_id, string caja_nro_proceso, int caja_nro_cierre, int caja_nro_rend, int rend_item)
		{
			try
			{
				if (string.IsNullOrEmpty(ent_compte))
					throw new NegocioException("Faltan datos obligatorios: ent_compte");

				var listaTemp = VtasPVCtlEntregaRendLista;
				var listaFiltrada = listaTemp.Where(x => x.caja_nro_proceso == caja_nro_proceso && x.caja_nro_cierre == caja_nro_cierre && x.caja_nro_rend == caja_nro_rend && x.rend_item == rend_item).ToList();
				listaFiltrada.ForEach(x => x.ent_compte = ent_compte);
				string json = JsonConvert.SerializeObject(listaFiltrada);
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
					app = "ENTREGA"
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
		//public JsonResult ConfirmarCtlEntrega(ConfirmarCtlEntregaInput input)
		public JsonResult ConfirmarCtlEntrega(string ent_comptes)
		{
			try
			{
				if (string.IsNullOrEmpty(ent_comptes))
					throw new NegocioException("Faltan datos obligatorios: ent_compte");

				List<string> lista = ent_comptes.Split(';').ToList();
				// Diccionario: ent_compte → respuesta de la API
				var dictRespuestas = new Dictionary<string, RespuestaGenerica<RespuestaDto>>();

				foreach (var item in lista)
				{
					var request = new ConfirmarCtlEntregaRequest()
					{
						adm_id = AdministracionId,
						usu_id = UserName,
						ent_compte = item
					};

					PrintProperties(request);

					var respuesta = _apiVentasServicio
						.ConfirmarCtlEntrega(request, TokenCookie)
						.Result;

					dictRespuestas[item] = respuesta;
				}

				// Evaluar fallos
				var fallidos = dictRespuestas
					.Where(x =>
						x.Value == null ||
						!x.Value.Ok ||
						x.Value.EsError ||
						(x.Value.Entidad != null && x.Value.Entidad.resultado != 0)
					)
					.Select(x => new
					{
						ent_compte = x.Key,
						mensaje = x.Value?.Mensaje
								   ?? x.Value?.Entidad?.resultado_msj
								   ?? "Error desconocido"
					})
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
					msg = "Algunas entregas no pudieron confirmarse",
					fallidos,
					respuestas = dictRespuestas
				});
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, warn = false, msg = ex.Message });
			}
		}


		[HttpPost]
		public JsonResult AnularCtlEntrega(string ent_comptes)
		{
			try
			{
				if (string.IsNullOrEmpty(ent_comptes))
					throw new NegocioException("Faltan datos obligatorios: ent_compte");

				List<string> lista = ent_comptes.Split(';').ToList();
				// Diccionario: ent_compte → respuesta de la API
				var dictRespuestas = new Dictionary<string, RespuestaGenerica<RespuestaDto>>();

				foreach (var item in lista)
				{
					var request = new AnularCtlEntregaRequest()
					{
						adm_id = AdministracionId,
						usu_id = UserName,
						ent_compte = item
					};

					PrintProperties(request);

					var respuesta = _apiVentasServicio
						.AnularCtlEntrega(request, TokenCookie)
						.Result;

					dictRespuestas[item] = respuesta;
				}

				// Evaluar fallos
				var fallidos = dictRespuestas
					.Where(x =>
						x.Value == null ||
						!x.Value.Ok ||
						x.Value.EsError ||
						(x.Value.Entidad != null && x.Value.Entidad.resultado != 0)
					)
					.Select(x => new
					{
						ent_compte = x.Key,
						mensaje = x.Value?.Mensaje
								   ?? x.Value?.Entidad?.resultado_msj
								   ?? "Error desconocido"
					})
					.ToList();

				// Si todos OK
				if (fallidos.Count == 0)
				{
					return Json(new
					{
						Ok = true,
						error = false,
						warn = false,
						msg = "Todas las entregas fueron revertidas correctamente",
						respuestas = dictRespuestas
					});
				}

				// Si hubo fallos
				return Json(new
				{
					Ok = false,
					error = true,
					warn = false,
					msg = "Algunas entregas no pudieron revertirse",
					fallidos,
					respuestas = dictRespuestas
				});
			}
			catch (Exception ex)
			{
				return Json(new { Ok = false, error = true, warn = false, msg = ex.Message });
			}
		}
		#region Metodos Privados
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
		private void CargarDatosIniciales(FiltroCtlCustodiaModel model)
		{
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaEntregas(List<VtasPVCtlEntregaDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.ent_compte, Descripcion = x.ent_compte });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private List<VtasPVCtlEntregaRendAux> MapperVtasPVCtlEntregaRendAux(List<VtasPVCtlEntregaRendDto> lista)
		{
			var listaRet = new List<VtasPVCtlEntregaRendAux>();
			foreach (var item in lista)
			{
				listaRet.Add(new VtasPVCtlEntregaRendAux()
				{
					adm_id=item.adm_id,
					adm_nombre = item.adm_nombre,
					caja_id = item.caja_id,
					caja_nro_cierre=item.caja_nro_cierre,
					caja_nro_proceso = item.caja_nro_proceso,
					caja_nro_rend =item.caja_nro_rend,
					ent_compte=item.ent_compte,
					ins_id=item.ins_id,
					rend_entrega=item.rend_entrega,
					rend_estado= item.rend_estado,
					rend_fecha= item.rend_fecha,
					rend_importe_arq= item.rend_importe_arq,
					rend_importe_ok= item.rend_importe_ok,
					rend_item=item.rend_item,
					rend_tipo=item.rend_tipo,
					usu_apellidoynombre=item.usu_apellidoynombre,
					usu_id=item.usu_id
				});
			}
			return listaRet;
		}
		#endregion

		public class VtasPVCtlEntregaRendAux()
		{
			public string ent_compte { get; set; } = string.Empty;
			public string adm_id { get; set; } = string.Empty;
			public string adm_nombre { get; set; } = string.Empty;
			public string caja_nro_proceso { get; set; } = string.Empty;
			public int caja_nro_cierre { get; set; }
			public int caja_nro_rend { get; set; }
			public int rend_item { get; set; }
			public string caja_id { get; set; } = string.Empty;
			public char rend_tipo { get; set; }
			public DateTime rend_fecha { get; set; }
			public string ins_id { get; set; } = string.Empty;
			public decimal rend_importe_ok { get; set; }
			public decimal rend_importe_arq { get; set; }
			public string usu_id { get; set; } = string.Empty;
			public string usu_apellidoynombre { get; set; } = string.Empty;
			public char rend_estado { get; set; }
			public char rend_entrega { get; set; }
		}
	}
}
