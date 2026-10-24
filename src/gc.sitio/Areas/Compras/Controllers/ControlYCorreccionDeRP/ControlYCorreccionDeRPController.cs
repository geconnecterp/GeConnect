using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.ControlYCorreccionDeRP;
using gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;

namespace gc.sitio.Areas.Compras.Controllers.ControlYCorreccionDeRP
{
	[Area("Compras")]
	public class ControlYCorreccionDeRPController : ControlYCorreccionDeRPControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IProductoServicio _prdSrv;
		public ControlYCorreccionDeRPController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ControlYCorreccionDeRPController> logger,
												ICuentaServicio cuentaServicio, IProductoServicio prdSrv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_prdSrv = prdSrv;
		}

		public IActionResult Index()
		{
			var model = new InitModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var titulo = "CONTROL DE RECEPCIÓN DE PROVEEDORES";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(true);
				model.ListaRecepciones = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.RecepcionSeleccionada = string.Empty;
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

		public IActionResult CargarRecepcionesDelProveedor(string cta_id)
		{
			var model = new InitRecepcionesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var recepciones = _prdSrv.ObtenerRprCtlPendiente(cta_id, TokenCookie).Result;
				RprCtlPendienteLista = recepciones;
				model.ListaRecepciones = ComboComptesPendientes();
				return PartialView("_listaRecepciones", model);
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

		public IActionResult ObtenerDetalleDeRecepcion(string rp_compte, string prov, string rp_compte_desc)
		{
			var model = new InitDetalleDeRecepcionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var detalle = _prdSrv.ObtenerRprCtlDetalle(rp_compte, TokenCookie).Result;
				RprCtlDetalleLista = detalle;
				model.GrillaDetalle = ObtenerGridCoreSmart<RprCtlItemDetalleDto>(detalle);
				model.Recepcion = rp_compte_desc;
				model.Proveedor = prov;
				return PartialView("_partial_Principal", model);
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

		public IActionResult ObtenerDatosModalAgregarProducto(string rp_compte, string cta_id, string dia_movi, string tco_id, string tco_desc, string cm_compte)
		{
			var model = new AgregarProductoModel();
			try
			{
				var listaTemp = new List<ComboGenDto>();
				listaTemp.Add(new ComboGenDto() { Id = rp_compte, Descripcion = $"{tco_desc} ({cm_compte})" });
				model.ComboRP = HelperMvc<ComboGenDto>.ListaGenerica(listaTemp);

				return PartialView("_modal_agregar_producto", model);
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

		public JsonResult ObtenerDatosDeRecepcionSeleccionada(string rp_compte)
		{
			try
			{
				var detalle = RprCtlPendienteLista.FirstOrDefault(x => x.rp_compte == rp_compte);
				if (detalle != null)
				{
					return Json(new { ok = true, data = detalle });
				}
				else
				{
					return Json(new { ok = false, mensaje = "No se encontró la recepción seleccionada." });
				}
			}
			catch (Exception ex)
			{
				return Json(new { ok = false, mensaje = ex.Message });
			}
		}


		[HttpPost]
		public JsonResult ConfirmarRprCtl(ConfirmarRprCtlRequest request)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (request == null)
					return Json(new { error = true, msg = "No se recibieron los datos para la confirmación del RPR." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				request.json_p = ObtenerJsonProductosParaRprCtl();
				PrintProperties(request);
				var respuesta = _prdSrv.ConfirmarRprCtl(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La acción se ejecutó correctamente.");
			}
			catch (NegocioException ex)
			{
				return Json(new { error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar confirmar");
				msg = $"Hubo algún inconveniente al intentar Confirmar la RPR.";
			}

			return Json(new { error = true, msg });
		}

		[HttpPost]
		public IActionResult ActualizarValorEnDetalle([FromBody] ActualizarDetalleRecepcionDto dto)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (dto == null)
					return Json(new { ok = false, error = true, msg = "No se recibieron los datos para la actualizacion." });

				var lista = RprCtlDetalleLista;
				var item = lista.FirstOrDefault(x => x.p_id == dto.p_id);

				if (item == null)
					return Json(new { ok = false });

				switch (dto.Campo)
				{
					case nameof(RprCtlItemDetalleDto.rpd_bulto_compte):
						item.rpd_bulto_compte = Convert.ToInt32(dto.Valor);
						break;

					case nameof(RprCtlItemDetalleDto.rpd_unidad_suelta_compte):
						item.rpd_unidad_suelta_compte = dto.Valor;
						break;

					case nameof(RprCtlItemDetalleDto.rpd_bulto_recibidos):
						item.rpd_bulto_recibidos = Convert.ToInt32(dto.Valor);
						break;

					case nameof(RprCtlItemDetalleDto.rpd_unidad_suelta):
						item.rpd_unidad_suelta = dto.Valor;
						break;
				}

				RprCtlDetalleLista = lista;

				return Json(new { ok = true });
			}
			catch (NegocioException ex)
			{
				return Json(new { ok = false, error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar actualizar");
				msg = $"Error al intentar actualizar.";
				return Json(new { ok = false, error = false, warn = true, msg = ex.Message });
			}
		}

		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio, "BI");
			}
		}
		protected SelectList ComboComptesPendientes()
		{
			var lista = RprCtlPendienteLista.Select(x => new ComboGenDto { Id = x.rp_compte.ToString(), Descripcion = $"RP N° {x.rp_compte} - {x.tco_desc_rp} ({x.tco_id_rp}) - {x.cm_compte_rp} - {x.cm_fecha_rp.ToShortDateString()} $ {x.cm_importe_rp}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private string ObtenerJsonProductosParaRprCtl()
		{
			var listaBase = RprCtlDetalleLista;
			var listaDestino = new List<ProductoParaRprCtlDto>();
			listaBase.ForEach(x => listaDestino.Add(new ProductoParaRprCtlDto() {
				oc_compte =x.oc_compte,
				p_desc = x.p_desc,
				p_id = x.p_id,
				rpd_bulto_compte = x.rpd_bulto_compte,
				rpd_bulto_recibidos = x.rpd_bulto_recibidos,
				rpd_cantidad = x.rpd_cantidad,
				rpd_cantidad_compte = x.rpd_cantidad_compte,
				rpd_cantidad_ori = x.rpd_cantidad_ori,
				rpd_item = x.rpd_item,
				rpd_unidad_pres = x.rpd_unidad_pres,
				rpd_unidad_suelta = x.rpd_unidad_suelta,
				rpd_unidad_suelta_compte = x.rpd_unidad_suelta_compte,
				rp_compte = x.rp_compte,
				up_id = x.up_id,
				up_tipo = x.up_tipo
			}));
			return JsonConvert.SerializeObject(listaDestino, new JsonSerializerSettings());
		}
		#endregion

		#region Clases
		public class ActualizarDetalleRecepcionDto
		{
			public string p_id { get; set; } = string.Empty;
			public string Campo { get; set; } = string.Empty;
			public decimal Valor { get; set; }
		}

		public class ProductoParaRprCtlDto
		{
			public string rp_compte { get; set; } = string.Empty;
			public int rpd_item { get; set; }
			public string p_id { get; set; } = string.Empty;
			public string p_desc { get; set; } = string.Empty;
			public string up_id { get; set; } = string.Empty;
			public string up_tipo { get; set; } = string.Empty;
			public string oc_compte { get; set; } = string.Empty;
			public int rpd_unidad_pres { get; set; }
			public int rpd_bulto_recibidos { get; set; }
			public decimal rpd_unidad_suelta { get; set; }
			public decimal rpd_cantidad { get; set; }
			public decimal rpd_cantidad_ori { get; set; }
			public int rpd_bulto_compte { get; set; }
			public decimal rpd_unidad_suelta_compte { get; set; }
			public decimal rpd_cantidad_compte { get; set; }
		}
		#endregion
	}
}
