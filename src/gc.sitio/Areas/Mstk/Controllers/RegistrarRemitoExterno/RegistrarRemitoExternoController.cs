using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models.ReporteDeVentas;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.Win32;
using System.Reflection;

namespace gc.sitio.Areas.Mstk.Controllers.RegistrarRemitoExterno
{
	[Area("Mstk")]
	public class RegistrarRemitoExternoController : RegistrarRemitoExternoControladorBase
	{
		private const string _tipoOP = "VE";
		private readonly AppSettings _setting;
		private readonly ITipoComprobanteServicio _tipoCompteServicio;
		private readonly IDepositoServicio _depositoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRemitoServicio _remitoServicio;
		public RegistrarRemitoExternoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<RegistrarRemitoExternoController> logger,
												ITipoComprobanteServicio tipoComprobanteServicio, IDepositoServicio depositoServicio,
												ICuentaServicio cuentaServicio, IRemitoServicio remitoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCompteServicio = tipoComprobanteServicio;
			_depositoServicio = depositoServicio;
			_cuentaServicio = cuentaServicio;
			_remitoServicio = remitoServicio;
		}

		public IActionResult Index()
		{
			var model = new InitCargaRegExt();
			List<DepositoInfoBoxDto> boxes = [];
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REMITOS EXTERNOS";
				ViewData["Titulo"] = titulo;

				InicializarDatosDeSession(model);
				model.TipoComprobantes = ComboTipoComprobante("%", _tipoOP);
				model.ComboDepositos = CargarComboDepositos();
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
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

		public async Task<IActionResult> ObtenerBoxesDesdeDeposito(string depoId)
		{
			var model = new BoxListDto();
			try
			{
				if (depoId != "0")
					model.ComboBoxes = CargarComboBoxes(depoId);
				else
				{
					List<DepositoInfoBoxDto> boxes = [];
					model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				}
				return PartialView("_listaBox", model);
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

		public async Task<JsonResult> VerificarExistenciaDeProductosDesdeComprobantes(RemitoExternoValidaRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (request == null)
					return Json(CrearRespuestaWarning("no se proporcionaron los datos de búsqueda."));
				var lista = await _remitoServicio.CargarProductosDesdeComprobante(request, TokenCookie);
				if (!lista.Ok)
				{
					_logger?.LogError("Se ha producido un error al intentar encontrar los datos del comprobante.");
					return Json(CrearRespuestaError("Se ha producido un error al intentar encontrar los datos del comprobante."));
				}
				if (lista.ListaEntidad == null || lista.ListaEntidad.Count() == 0)
				{
					_logger?.LogInformation("El comprobante no existe o no es un comprobante relacionado a una cotización.");
					return Json(CrearRespuestaWarning("El comprobante no existe o no es un comprobante relacionado a una cotización."));
				}
				lista.ListaEntidad.ForEach(x => { x.box_id = request.box_id; x.depo_id = request.depo_id; x.a_remitir = (x.pre_cantidad - x.pre_cantidad_ent); });
				ListaRemitoExternoValida = lista.ListaEntidad;
				var resultadoDeValidacion = PermiteCargaDeProductosEnRemito(ListaRemitoExternoValida);
				if (resultadoDeValidacion.Resultado)
					return Json(CrearRespuestaOk("Se encontraron los datos de productos desde el comprobante.", true));
				else
					return Json(CrearRespuestaOk(resultadoDeValidacion.Mensaje, false));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaError("Error"));
			}
		}

		public IActionResult CargarProductosDesdeComprobante()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var lista = ListaRemitoExternoValida;

				return PartialView("_partialProdsDelCompte", ObtenerGridCoreSmart<RemitoExternoValidaDto>(lista));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error"));
			}
		}

		public JsonResult LimpiarProductosCargados()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);

				ListaRemitoExternoValida = [];
				return Json(CrearRespuestaOk("Colección de productos eliminada.", false));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaError(ex.Message));
			}
		}

		public JsonResult ValidarExistenciaDeProducto(string pId)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);

				var producto = ListaRemitoExternoValida.Where(x => x.p_id == pId);
				if (producto == null || producto.Count() <= 0)
					return Json(CrearRespuestaOk("Producto inexistente en comprobante.", false));
				if (producto.First().a_remitir > 0)
					return Json(CrearRespuestaOk("Producto ingresado posee cantidad máxima a remitir mayor a 0.", false));
				return Json(CrearRespuestaOk("", true));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaError(ex.Message));
			}
		}

		[HttpPost]
		public async Task<JsonResult> ConfirmarRemitoExterno(ConfirmarRemitoExternoRequest request)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (request == null)
					return Json(new { error = true, msg = "No se recibieron los datos para la confirmación del remito." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				PrintProperties(request);
				var respuesta = _remitoServicio.ConfirmarRemitoExterno(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La acción se ejecutó correctamente.");
			}
			catch (NegocioException ex)
			{
				return Json(new { error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar confirmar");
				msg = $"Hubo algún inconveniente al intentar Confirmar la Autorización.";
			}

			return Json(new { error = true, msg });
		}

		#region Metodos Privados
		private List<ProductoRemitoDto> MapearProductos(List<RemitoExternoValidaDto> origen)
		{
			if (origen == null || origen.Count == 0)
				return [];

			return origen.Select(x => new ProductoRemitoDto
			{
				p_id = x.p_id,
				p_desc = x.p_desc,
				depo_id = x.depo_id,
				box_id = x.box_id,
				up_id = x.up_id,
				unidad_pres = x.unidad_pres,   // ejemplo
				bulto = x.bulto,              // ejemplo
				us = x.us,             // ejemplo
				cantidad = x.pre_cantidad            // ejemplo

			}).ToList();
		}
		private ResultadoValidacionRemito PermiteCargaDeProductosEnRemito(List<RemitoExternoValidaDto> lista)
		{
			if (lista == null || lista.Count == 0)
				return new ResultadoValidacionRemito
				{
					Resultado = false,
					Mensaje = "No existen productos asociados al comprobante."
				};

			var item = lista.First();
			if ((item.pree_id != "F" && item.pree_id != "R") && item.pret_id != "F")
				return new ResultadoValidacionRemito
				{
					Resultado = false,
					Mensaje = "El comprobante no permite carga de productos debido a su estado."
				};

			var fechaLimite = DateTime.Now.AddDays(-60);
			if (item.pre_fecha < fechaLimite)
				return new ResultadoValidacionRemito
				{
					Resultado = false,
					Mensaje = "El comprobante tiene más de 60 días y no permite carga de productos."
				};

			if (lista.Exists(x => x.pre_cantidad_ent < x.pre_cantidad))
				return new ResultadoValidacionRemito
				{
					Resultado = false,
					Mensaje = "Existen ítems con cantidad entregada menor a la cantidad comprada."
				};

			return new ResultadoValidacionRemito
			{
				Resultado = true,
				Mensaje = "Validación correcta. Se permite la carga de productos."
			};
		}


		public class ResultadoValidacionRemito
		{
			public bool Resultado { get; set; }
			public string Mensaje { get; set; } = "";
		}

		private void InicializarDatosDeSession(InitCargaRegExt model)
		{
			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");
		}
		protected SelectList ComboTipoComprobante(string afip_id, string opt_id)
		{
			var listaTemp = _tipoCompteServicio.BuscarTipoComprobanteListaPorTipoAfip(afip_id, opt_id, Token).Result;
			TiposComprobante = listaTemp;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboDepositos()
		{
			var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
			DepositoLista = adms;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboBoxes(string depoId)
		{
			var adms = _depositoServicio.BuscarBoxPorDeposito(depoId, TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
