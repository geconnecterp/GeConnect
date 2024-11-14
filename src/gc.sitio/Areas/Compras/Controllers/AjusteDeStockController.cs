using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Reflection;
using System.Security.Cryptography;
using static gc.sitio.Areas.Compras.Controllers.CompraController;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class AjusteDeStockController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly ILogger<CompraController> _logger;

		public AjusteDeStockController(IProductoServicio productoServicio, IDepositoServicio depositoServicio,
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

		public async Task<IActionResult> AjustesDeStock()
		{
			AjusteDeStockDto model = new AjusteDeStockDto();
			List<DepositoInfoBoxDto> boxes = [];
			List<ProductoAAjustarDto> listaProdAAjustar = [];
			try
			{
				model.ComboDepositos = CargarComboDepositos();
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				model.ComboMotivos = CargarComboTiposDeAjusteDeStock();
				model.ProductosAAjustar = ObtenerGridCore<ProductoAAjustarDto>(listaProdAAjustar);
				AjusteProductosLista = [];
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
				model.ComboBoxes = CargarComboBoxes(depoId);
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

		public async Task<IActionResult> ObtenerBoxesDesdeDepositoDesdeCargaPrevia(string depoId)
		{
			var model = new BoxListDto();
			try
			{
				var boxesAux = AjustePrevioCargadoLista.Where(x => x.depo_id == depoId).Select(x => new { x.box_id, x.box_desc }).Distinct();
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

		public async Task<IActionResult> ObtenerDatosModalCargaPrevia()
		{
			var model = new DatosModalCargaPreviaDto();
			try
			{
				if (AdministracionId == null)
					return ObtenerMensajeDeError("No hay sucural de logueo establecida. Si el problema persiste informe al Administrador.");

				var listaAjustesPrevios = await _productoServicio.ObtenerAJPreviosCargados(AdministracionId, TokenCookie);
				AjustePrevioCargadoLista = listaAjustesPrevios;
				model.ListaProductos = ObtenerGridCore<AjustePrevioCargadoDto>(new List<AjustePrevioCargadoDto>());
				var auxdepositos = listaAjustesPrevios.Select(x => new { x.depo_id, x.depo_nombre }).Distinct();
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

		public async Task<IActionResult> FiltrarProductosModalCargaPrevia(string depoId, string boxId)
		{
			var model = new GridCore<AjustePrevioCargadoDto>();
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
					model = ObtenerGridCore<AjustePrevioCargadoDto>(new List<AjustePrevioCargadoDto>());
					return PartialView("_grillaProductosEnModalCargaPrevia", model);
				}
				var listaAjustesPrevios = AjustePrevioCargadoLista.Where(x => x.depo_id.Equals(depoId) && x.box_id.Equals(boxId)).ToList();
				model = ObtenerGridCore<AjustePrevioCargadoDto>(listaAjustesPrevios);
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

		public async Task<IActionResult> ActaulizarListaProductosDesdeModalCargaPrevia(string depoId, string boxId)
		{
			var model = new GridCore<ProductoAAjustarDto>();
			try
			{
				var listaAjustesPrevios = AjustePrevioCargadoLista.Where(x => x.depo_id.Equals(depoId) && x.box_id.Equals(boxId)).ToList();
				listaAjustesPrevios.RemoveAll(x => AjusteProductosLista.Exists(y => y.p_id.Equals(x.p_id)));
				if (listaAjustesPrevios.Count > 0)
				{
					var productosMapeados = ObtenerProductoAAjustaDesdeAjustePrevio(listaAjustesPrevios);
					var listaTemp = AjusteProductosLista;
					listaTemp.AddRange(productosMapeados);
					AjusteProductosLista = listaTemp;
					model = ObtenerGridCore<ProductoAAjustarDto>(listaTemp);
				}
				else
					model = ObtenerGridCore<ProductoAAjustarDto>(AjusteProductosLista);

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

		public async Task<JsonResult> ValidarNroDeAjusteARevertir(string ajId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(ajId))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar un valor válido para ajuste a revertir." });
				}
				var listaAjustesPrevios = await _productoServicio.ObtenerAJREVERTIDO(ajId, TokenCookie);
				var depo_id = listaAjustesPrevios.First().depo_id;
				if (DepositoLista != null && DepositoLista.Count > 0)
				{
					if (!DepositoLista.Exists(x => x.Depo_Id.Equals(depo_id)))
					{
						return Json(new { error = false, warn = true, msg = $"EL ajuste indicado '{ajId}' no corresponde a las sucursales habilitadas." });
					}
					return Json(new { error = false, warn = false, msg = "" });
				}
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar el ajuste, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> ObtenerProductosDesdeAJRevertido(string ajId)
		{
			var model = new GridCore<ProductoAAjustarDto>();
			try
			{
				if (ajId == null)
					return ObtenerMensajeDeError("No se ha especificado un ID de Ajuste válido. Si el problema persiste informe al Administrador.");

				var listaAjustesPrevios = await _productoServicio.ObtenerAJREVERTIDO(ajId, TokenCookie);
				if (listaAjustesPrevios == null || listaAjustesPrevios.Count == 0)
				{
					model = ObtenerGridCore<ProductoAAjustarDto>(AjusteProductosLista);
					return PartialView("_grillaProductos", model);
				}
				listaAjustesPrevios.RemoveAll(x => AjusteProductosLista.Exists(y => y.p_id.Equals(x.p_id)));
				if (listaAjustesPrevios.Count > 0)
				{
					var productosMapeados = ObtenerProductoAAjustaDesdeListaDeProductoARevertir(listaAjustesPrevios);
					var listaTemp = AjusteProductosLista;
					listaTemp.AddRange(productosMapeados);
					AjusteProductosLista = listaTemp;
					model = ObtenerGridCore<ProductoAAjustarDto>(listaTemp);
				}
				else
				{
					model = ObtenerGridCore<ProductoAAjustarDto>(AjusteProductosLista);
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

		public async Task<IActionResult> QuitarProductoDeLista(string pId)
		{
			var model = new GridCore<ProductoAAjustarDto>();
			try
			{
				if (pId == null)
					return ObtenerMensajeDeError("No se ha especificado un producto válido. Si el problema persiste informe al Administrador.");

				var listaTemp = AjusteProductosLista.Where(x => x.p_id != pId).ToList();
				AjusteProductosLista = listaTemp;
				model = ObtenerGridCore<ProductoAAjustarDto>(listaTemp);
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

		public async Task<IActionResult> AgregarProductoALista(string pId, string boxId, string depoId, string atId, decimal us, int bto, int unidadPres, string upId)
		{
			var model = new GridCore<ProductoAAjustarDto>();
			try
			{
				if (AjusteProductosLista.Where(x => x.p_id.Equals(pId)).Any())
				{
					model = ObtenerGridCore<ProductoAAjustarDto>(AjusteProductosLista);
				}
				else
				{
					var listaTemp = AjusteProductosLista;
					var productoStk = await _productoServicio.InfoProductoStkBoxes(pId, AdministracionId, depoId, TokenCookie, boxId);
					var producto = ObtenerDatosDeProducto(pId);
					var box = await _depositoServicio.ObtenerInfoDeBox(boxId, TokenCookie);
					if (productoStk != null && productoStk.Count > 0 && producto != null)
					{
						var newProduct = new ProductoAAjustarDto()
						{
							tipo = "M",
							as_nro_revierte = null,
							depo_id = depoId,
							box_id = boxId,
							box_desc = box.FirstOrDefault()?.Box_desc,
							ta_id = atId,
							usu_id = UserName,
							p_id = pId,
							p_desc = producto.P_desc,
							p_id_prov = producto.P_id_prov,
							up_id = upId,
							unidad_pres = unidadPres,
							bulto = bto,
							us = (unidadPres * bto) + us,
							as_stock = productoStk.First().Ps_stk,
							as_ajuste = (unidadPres * bto) + us,
							cantidad = productoStk.First().Ps_stk - ((unidadPres * bto) + us),
							as_resultado = productoStk.First().Ps_stk - ((unidadPres * bto) + us),
						};
						listaTemp.Add(newProduct);
						AjusteProductosLista = listaTemp;
						model = ObtenerGridCore<ProductoAAjustarDto>(listaTemp);
					}
					else
					{
						model = ObtenerGridCore<ProductoAAjustarDto>(AjusteProductosLista);
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

		public async Task<JsonResult> ConfirmarAjusteDeStock(string atId, string nota, string atTipo)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(atId) || string.IsNullOrWhiteSpace(atTipo))
				{
					return Json(new { error = true, warn = false, msg = "Debe especificar un 'Tipo' antes de confirmar." });
				}
				if (string.IsNullOrWhiteSpace(nota))
				{
					return Json(new { error = true, warn = false, msg = "Debe especificar una nota antes de confirmar." });
				}
				if (AjusteProductosLista == null || AjusteProductosLista.Count == 0)
				{
					return Json(new { error = true, warn = false, msg = "Debe agregar al menos un producto en el Ajuste de Stock antes de confirmar." });
				}

				var listaTemp = AjusteProductosLista;
				listaTemp.ForEach(x => { x.ta_id = atId; x.nota = nota; });
				if (atId=="")
				AjusteProductosLista = listaTemp;
				var json_string = GenerarJsonDesdeLista();

				return Json(new { error = false, warn = false, msg = json_string });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar el ajuste, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> VerificaExistenciaDeAjusteDeStock()
		{
			try
			{
				if (AjusteProductosLista == null || AjusteProductosLista.Count == 0)
				{
					return Json(new { error = false, warn = true, msg = "No existen Ajustes de Stock cargado por cancelar." });
				}
				else
				{
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, warn = false, msg = "Algo no fue bien al validar el ajuste, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> CancelarAjusteDeStock()
		{
			var model = new GridCore<ProductoAAjustarDto>();
			try
			{
				var listaTemp = AjusteProductosLista;
				listaTemp = [];
				AjusteProductosLista = listaTemp;
				model = ObtenerGridCore<ProductoAAjustarDto>(listaTemp);
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

		#region Métodos Privados
		private string GenerarJsonDesdeLista()
		{
			var jsonstring = JsonConvert.SerializeObject(AjusteProductosLista, new JsonSerializerSettings() { ContractResolver = new IgnorePropertiesResolver(new[] { "Producto" }) });
			return jsonstring;
		}

		private List<ProductoAAjustarDto> ObtenerProductoAAjustaDesdeAjustePrevio(List<AjustePrevioCargadoDto> listaAjustesPrevios)
		{
			if (!listaAjustesPrevios.Any())
				return [];
			var listaMapeada = new List<ProductoAAjustarDto>();
			foreach (var item in listaAjustesPrevios)
			{
				listaMapeada.Add(new ProductoAAjustarDto()
				{
					p_id = item.p_id,
					p_desc = item.p_desc,
					p_id_prov = item.p_id_prov,
					as_ajuste = item.cantidad,
					as_stock = item.ps_stk,
					as_resultado = item.cantidad + item.ps_stk,
					box_id = item.box_id,
					box_desc = item.box_desc,
					depo_id = item.depo_id,
					tipo = "P",
					usu_id = UserName,
					as_nro_revierte = null,
					us = item.us,
					bulto = item.bulto,
					cantidad = item.cantidad + item.ps_stk,
					unidad_pres = item.unidad_pres,
					up_id = item.up_id,
				});
			}
			return listaMapeada;
		}
		private List<ProductoAAjustarDto> ObtenerProductoAAjustaDesdeListaDeProductoARevertir(List<AjusteRevertidoDto> listaAjustesPrevios)
		{
			if (!listaAjustesPrevios.Any())
				return [];
			var listaMapeada = new List<ProductoAAjustarDto>();
			foreach (var item in listaAjustesPrevios)
			{
				listaMapeada.Add(new ProductoAAjustarDto()
				{
					p_id = item.p_id,
					p_desc = item.p_desc,
					p_id_prov = item.p_id_prov,
					as_ajuste = item.as_ajuste * -1,
					as_stock = item.ps_stk,
					as_resultado = (item.as_ajuste * -1) + item.ps_stk,
					box_id = item.box_id,
					box_desc = item.box_desc,
					tipo = "R",
					depo_id = item.depo_id,
					usu_id = UserName,
					us = 0,
					up_id = item.up_id,
					bulto = 0,
					cantidad = (item.as_ajuste * -1) + item.ps_stk,
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
		private SelectList CargarComboTiposDeAjusteDeStock()
		{
			var adms = _productoServicio.ObtenerTipoDeAjusteDeStock(TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = $"{x.at_id}#{x.at_tipo}", Descripcion = x.at_desc });
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
