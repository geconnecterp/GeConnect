using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Globalization;
using System.Reflection;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using static gc.sitio.Areas.Compras.Controllers.CompraController;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class DevolucionAProveedorController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly ILogger<CompraController> _logger;
		public DevolucionAProveedorController(IProductoServicio productoServicio, IDepositoServicio depositoServicio,
			ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_depositoServicio = depositoServicio;
			_productoServicio = productoServicio;
		}
		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> DevolucionesAProveedor()
		{
			DevolucionAProveedorDto model = new();
			List<DepositoInfoBoxDto> boxes = [];
			List<ProductoADevolverDto> listaProdAAjustar = [];
			try
			{
				model.ComboDepositos = CargarComboDepositos();
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				model.ProductosADevolver = ObtenerGridCore<ProductoADevolverDto>(listaProdAAjustar);
				DevolucionProductosLista = [];
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
				{
					model.ComboBoxes = CargarComboBoxes(depoId);
				}
				else
				{
					List<DepositoInfoBoxDto> boxes = [];
					model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				}
				return PartialView("~/Areas/Compras/Views/DevolucionAProveedor/_listaBox.cshtml", model);
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

		public async Task<JsonResult> ValidarPertenenciaDeProductoAProveedor(string pId, string ctaId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(pId) || string.IsNullOrWhiteSpace(ctaId))
				{
					return Json(new { error = true, warn = false, msg = "No se puede validar pertenencia de producto, faltan datos." });
				}
				else
				{
					var producto = ObtenerDatosDeProducto(pId);
					if (producto == null)
						return Json(new { error = true, warn = false, msg = "Ha ocurrio un error al intentar obtener el producto solicitado." });
					if (producto.Cta_id != ctaId)
						return Json(new { error = false, warn = true, msg = "El producto ingresado no pertenece al proveedor especificado." });
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar la pertenencia del producto, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> AgregarProductoALista(string pId, string boxId, string ctaId, string depoId, decimal us, int bto, int unidadPres, string upId)
		{
			var model = new GridCore<ProductoADevolverDto>();
			try
			{
				if (DevolucionProductosLista.Where(x => x.p_id.Equals(pId)).Any())
				{
					model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);
				}
				else
				{
					var listaTemp = DevolucionProductosLista;
					var productoStk = await _productoServicio.InfoProductoStkBoxes(pId, AdministracionId, depoId, TokenCookie, boxId);
					var producto = ObtenerDatosDeProducto(pId);
					var box = await _depositoServicio.ObtenerInfoDeBox(boxId, TokenCookie);
					if (producto.Cta_id != ctaId)
					{
						model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);
					}
					if (productoStk != null && productoStk.Count > 0 && producto != null)
					{
						var stkEnteroAux = 0;
						var stkDecimalAux = 0.000M;
						var cantidadAux = 0.000M;
						var unidadPresDecimalAux = Convert.ToDecimal(unidadPres);
						var bultoDecimalAux = Convert.ToDecimal(bto);
						if (producto.Up_id == "07") //Entero
						{
							stkEnteroAux = Int32.Parse(productoStk.First().Ps_stk.ToString(), NumberStyles.AllowThousands, CultureInfo.CurrentCulture);
							var upxbto = unidadPres * bto;
							cantidadAux = stkEnteroAux - ((upxbto < 0 ? upxbto * -1 : upxbto) + us);
						}
						else
						{
							stkDecimalAux = productoStk.First().Ps_stk;
							cantidadAux = stkDecimalAux - ((unidadPresDecimalAux * bultoDecimalAux) * us);
						}

						var newProduct = new ProductoADevolverDto()
						{
							tipo = "M",
							dp_nro_revierte = null,
							depo_id = depoId,
							box_id = boxId,
							box_desc = box.FirstOrDefault()?.Box_desc,
							usu_id = UserName,
							p_id = pId,
							p_desc = producto.P_desc,
							cta_id = producto.Cta_id,
							up_id = upId,
							unidad_pres = unidadPres,
							bulto = bto,
							us = us,
							as_stock = productoStk.First().Ps_stk,
							as_ajuste = (unidadPres * bto) + us,
							cantidad = (unidadPres * bto) + us,
							as_resultado = cantidadAux,
						};
						listaTemp.Add(newProduct);
						DevolucionProductosLista = listaTemp;
						model = ObtenerGridCore<ProductoADevolverDto>(listaTemp);
					}
					else
					{
						model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);
					}
				}
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
			return PartialView("_grillaProductos", model);
		}
		public async Task<IActionResult> FiltrarProductosModalCargaPrevia(string depoId, string boxId)
		{
			var model = new GridCore<DevolucionPrevioCargadoDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(depoId) && string.IsNullOrWhiteSpace(boxId))
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Debe especificar Id de depósito y box."
					};
					return PartialView("_gridMensaje", response);
				}
				//Limpio la grilla
				if (!string.IsNullOrWhiteSpace(depoId) && string.IsNullOrWhiteSpace(boxId))
				{
					model = ObtenerGridCore<DevolucionPrevioCargadoDto>([]);
					return PartialView("_grillaProductosEnModalCargaPrevia", model);
				}
				var listaAjustesPrevios = DevolucionPrevioCargadoLista.Where(x => x.depo_id.Equals(depoId) && x.box_id.Equals(boxId)).ToList();
				model = ObtenerGridCore<DevolucionPrevioCargadoDto>(listaAjustesPrevios);
				return PartialView("_grillaProductosEnModalCargaPrevia", model);
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

		public async Task<JsonResult> ValidarNroDeDevARevertir(string dpId, string ctaId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(dpId))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar un valor válido para devolución a revertir." });
				}
				var listaAjustesPrevios = await _productoServicio.ObtenerDPREVERTIDO(dpId, TokenCookie);
				if (listaAjustesPrevios == null)
					return Json(new { error = true, warn = false, msg = $"La devolución indicada '{dpId}' no existe." });
				if (listaAjustesPrevios.Count == 0)
					return Json(new { error = false, warn = true, msg = $"La devolución indicada '{dpId}' no posee datos." });
				var unProducto = listaAjustesPrevios.First();
				if (unProducto.cta_id != ctaId)
					return Json(new { error = false, warn = true, msg = $"La devolución indicada '{dpId}' no corresponde al proveedor {ctaId}." });
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar el ajuste, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> ObtenerProductosDesdeDPRevertido(string dpId)
		{
			var model = new GridCore<ProductoADevolverDto>();
			try
			{
				if (dpId == null)
					return ObtenerMensajeDeError("No se ha especificado un ID de devolucion válido. Si el problema persiste informe al Administrador.");

				var listaAjustesPrevios = await _productoServicio.ObtenerDPREVERTIDO(dpId, TokenCookie);
				if (listaAjustesPrevios == null || listaAjustesPrevios.Count == 0)
				{
					model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);
					return PartialView("_grillaProductos", model);
				}
				listaAjustesPrevios.RemoveAll(x => DevolucionProductosLista.Exists(y => y.p_id.Equals(x.p_id)));
				if (listaAjustesPrevios.Count > 0)
				{
					var productosMapeados = ObtenerProductoADevolverDesdeListaDeProductoARevertir(listaAjustesPrevios);
					var listaTemp = DevolucionProductosLista;
					listaTemp.AddRange(productosMapeados);
					DevolucionProductosLista = listaTemp;
					model = ObtenerGridCore<ProductoADevolverDto>(listaTemp);
				}
				else
				{
					model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);
				}
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
			return PartialView("_grillaProductos", model);
		}

		public async Task<IActionResult> ObtenerDatosModalCargaPrevia(string ctaId)
		{
			var model = new DatosModalCargaPreviaDPDto();
			try
			{
				if (AdministracionId == null)
					return ObtenerMensajeDeError("No hay sucural de logueo establecida. Si el problema persiste informe al Administrador.");

				var listaDevolucionPrevios = await _productoServicio.ObtenerDPPreviosCargados(AdministracionId, ctaId, TokenCookie);
				DevolucionPrevioCargadoLista = listaDevolucionPrevios;
				model.ListaProductos = ObtenerGridCore<DevolucionPrevioCargadoDto>([]);
				var auxdepositos = listaDevolucionPrevios.Select(x => new { x.depo_id, x.depo_nombre }).Distinct();
				var depositos = auxdepositos.Select(x => new ComboGenDto { Id = x.depo_id, Descripcion = x.depo_nombre });
				model.ComboDepositos = HelperMvc<ComboGenDto>.ListaGenerica(depositos);
				List<DepositoInfoBoxDto> boxes = [];
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
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
			return PartialView("_modalCargaPrevia", model);

		}

		public async Task<IActionResult> ObtenerBoxesDesdeDepositoDesdeCargaPrevia(string depoId)
		{
			var model = new BoxListDto();
			try
			{
				var boxesAux = DevolucionPrevioCargadoLista.Where(x => x.depo_id == depoId).Select(x => new { x.box_id, x.box_desc }).Distinct();
				var boxes = boxesAux.Select(x => new ComboGenDto { Id = x.box_id, Descripcion = $"{x.box_id}__{x.box_desc}" });
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes);
				return PartialView("_listaBoxEnCargaPrevia", model);
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

		public async Task<IActionResult> QuitarProductoDeLista(string pId)
		{
			var model = new GridCore<ProductoADevolverDto>();
			try
			{
				if (pId == null)
					return ObtenerMensajeDeError("No se ha especificado un producto válido. Si el problema persiste informe al Administrador.");

				var listaTemp = DevolucionProductosLista.Where(x => x.p_id != pId).ToList();
				DevolucionProductosLista = listaTemp;
				model = ObtenerGridCore<ProductoADevolverDto>(listaTemp);
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
			return PartialView("_grillaProductos", model);
		}

		public async Task<IActionResult> ActualizarListaProductosDesdeModalCargaPrevia(string depoId, string boxId, string[] ids)
		{
			var model = new GridCore<ProductoADevolverDto>();
			try
			{
				var listaDevolucionPrevios = DevolucionPrevioCargadoLista.Where(x => x.depo_id.Equals(depoId) && x.box_id.Equals(boxId) && ids.Contains(x.p_id)).ToList();
				//Si no existen ya, los agregamos
				listaDevolucionPrevios.RemoveAll(x => DevolucionProductosLista.Exists(y => y.p_id.Equals(x.p_id)));
				if (listaDevolucionPrevios.Count > 0)
				{
					var productosMapeados = ObtenerProductoADevolverDesdeDevolucionPrevia(listaDevolucionPrevios);
					var listaTemp = DevolucionProductosLista;
					listaTemp.AddRange(productosMapeados);
					DevolucionProductosLista = listaTemp;
					model = ObtenerGridCore<ProductoADevolverDto>(listaTemp);
				}
				else
					model = ObtenerGridCore<ProductoADevolverDto>(DevolucionProductosLista);

				return PartialView("_grillaProductos", model);
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

		public async Task<JsonResult> ValidarExistenciaDeProductosCargadosParaDevolucion(bool esConfirmación)
		{
			try
			{
				if (DevolucionProductosLista == null)
					return Json(new { error = true, warn = false, msg = $"No se han cargado productos aún." });
				if (DevolucionProductosLista.Count == 0)
					return Json(new { error = false, warn = true, msg = $"No se han cargado productos aún." });
				if (esConfirmación)
					return Json(new { error = false, warn = false, msg = $"Desea confirmar la carga de devolución a proveedor? Esta acción no se puede revertir." });
				else
					return Json(new { error = false, warn = false, msg = $"Desea cancelar la carga de devolución a proveedor? Esta acción no se puede revertir." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar la existencia de productos a devolver, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> LimpiarDatosCargadosParaDevolucion()
		{
			try
			{
				DevolucionProductosLista = [];
				DevolucionPrevioCargadoLista = [];
				return Json(new { error = false, warn = false, msg = $"" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al lipiar la lista de productos a devolver, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> ConfirmarDevolucion(string nota)
		{
			try
			{
				var listaTemp = DevolucionProductosLista;
				listaTemp.ForEach(x => { x.nota = nota; });
				DevolucionProductosLista = listaTemp;
				var json_string = GenerarJsonDesdeLista();
				var respuesta = await _productoServicio.ConfirmarDP(json_string, AdministracionId, UserName, TokenCookie);
				if (respuesta == null)
					return Json(new { error = true, warn = false, msg = "Algo no fue bien al confirmar la devolución, intente nuevamente mas tarde.", jsonstring = json_string });
				if (respuesta.Count == 0)
					return Json(new { error = true, warn = false, msg = "Algo no fue bien al confirmar la devolución, intente nuevamente mas tarde.", jsonstring = json_string });
				if (respuesta.First().resultado != 0)
					return Json(new { error = false, warn = true, msg = respuesta.First().resultado_msj, jsonstring = json_string });

				DevolucionProductosLista = [];
				return Json(new { error = false, warn = false, msg = respuesta.First().resultado_msj, jsonstring = json_string });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar la existencia de productos a devolver, intente nuevamente mas tarde." });
			}
		}

		#region Métodos Privados
		private string GenerarJsonDesdeLista()
		{
			var jsonstring = JsonConvert.SerializeObject(DevolucionProductosLista, new JsonSerializerSettings() { ContractResolver = new IgnorePropertiesResolver(new[] { "Producto" }) });
			return jsonstring;
		}
		private List<ProductoADevolverDto> ObtenerProductoADevolverDesdeDevolucionPrevia(List<DevolucionPrevioCargadoDto> listaAjustesPrevios)
		{
			if (!listaAjustesPrevios.Any())
				return [];
			var listaMapeada = new List<ProductoADevolverDto>();
			foreach (var item in listaAjustesPrevios)
			{
				listaMapeada.Add(new ProductoADevolverDto()
				{
					p_id = item.p_id,
					p_desc = item.p_desc,
					cta_id = item.cta_id,
					as_ajuste = item.cantidad,
					as_stock = item.ps_stk,
					as_resultado = item.ps_stk - item.cantidad,
					box_id = item.box_id,
					box_desc = item.box_desc,
					depo_id = item.depo_id,
					tipo = "P",
					usu_id = UserName,
					dp_nro_revierte = null,
					us = item.us,
					bulto = item.bulto,
					cantidad = item.ps_stk - item.cantidad,
					unidad_pres = item.unidad_pres,
					up_id = item.up_id,
				});
			}
			return listaMapeada;
		}

		private List<ProductoADevolverDto> ObtenerProductoADevolverDesdeListaDeProductoARevertir(List<DevolucionRevertidoDto> listaAjustesPrevios)
		{
			if (!listaAjustesPrevios.Any())
				return [];
			var listaMapeada = new List<ProductoADevolverDto>();
			foreach (var item in listaAjustesPrevios)
			{


				listaMapeada.Add(new ProductoADevolverDto()
				{
					p_id = item.p_id,
					p_desc = item.p_desc,
					cta_id = item.cta_id,
					as_ajuste = item.as_ajuste * -1,
					as_stock = item.ps_stk,
					as_resultado = item.ps_stk - (item.as_ajuste * -1),
					box_id = item.box_id,
					box_desc = item.box_desc,
					tipo = "R",
					depo_id = item.depo_id,
					usu_id = UserName,
					us = item.ps_stk - (item.as_ajuste * -1),
					up_id = item.up_id,
					bulto = Convert.ToInt32(item.ps_bulto),
					cantidad = item.ps_stk - (item.as_ajuste * -1),
					as_motivo = item.dv_motivo,
				});
			}
			return listaMapeada;
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
		private ProductoBusquedaDto ObtenerDatosDeProducto(string p_id)
		{
			ProductoBusquedaDto producto = new ProductoBusquedaDto { P_id = "0000-0000" };
			BusquedaBase buscar = new()
			{
				Administracion = AdministracionId,
				Busqueda = p_id,
				DescuentoCli = 0,
				ListaPrecio = "",
				TipoOperacion = ""
			};

			return _productoServicio.BusquedaBaseProductos(buscar, TokenCookie).Result;
		}
		#endregion
	}
}
