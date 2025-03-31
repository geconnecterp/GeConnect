using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System;
using System.Security.Cryptography;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDeCompraConsultaController : OrdenDeCompraConsultaControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IAdministracionServicio _adminServicio;
		private readonly IOrdenDeCompraEstadoServicio _ordenDeCompraEstadoServicio;
		private readonly IProductoServicio _productoServicio;
		public OrdenDeCompraConsultaController(ICuentaServicio cuentaServicio, ILogger<OrdenDeCompraConsultaController> logger, IAdministracionServicio adminServicio,
											   IOrdenDeCompraEstadoServicio ordenDeCompraEstadoServicio, IOptions<AppSettings> options, IHttpContextAccessor context,
											   IProductoServicio productoServicio) : base(options, context, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_adminServicio = adminServicio;
			_ordenDeCompraEstadoServicio = ordenDeCompraEstadoServicio;
			_productoServicio = productoServicio;
		}
		public IActionResult Index()
		{
			MetadataGrid metadata;
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				ViewData["Titulo"] = "CONSULTA DE ORDENES DE COMPRA";
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

		public async Task<IActionResult> BuscarOrdenesDeCompra(BuscarOrdenesDeCompraRequest request)
		{
			MetadataGrid metadata;
			GridCore<OrdenDeCompraConsultaDto> grillaDatos;
			ConsultaOCModel model = new();
			try
			{
				request.Registros = _settings.NroRegistrosPagina;
				var productos = _productoServicio.CargarOrdenDeCompraConsultaLista(request, TokenCookie).Result;
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				ListaOrdenDeCompraConsulta = productos.Item1;
				grillaDatos = GenerarGrilla(productos.Item1, request.Sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir);
				model.GrillaOC = grillaDatos;
				model.Importe = ListaOrdenDeCompraConsulta.Count > 0 ? ListaOrdenDeCompraConsulta.Sum(x => x.oc_total) : 0;
				model.ListaAdministraciones = new SelectList(AdministracionesLista, "Adm_id", "Adm_nombre");
				return PartialView("_grillaProductosOC", model);
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
		public async Task<IActionResult> BuscarDetalleDeOrdenDeCompra(string ocCompte)
		{
			ConsultaOCDetalleModel model = new ConsultaOCDetalleModel();
			try
			{
				var detalle = _productoServicio.CargarDetalleDeOC(ocCompte, TokenCookie).Result;
				model.GrillaDetalle = ObtenerGridCore<OrdenDeCompraDetalleDto>(detalle);
				var detalleItem = detalle.First();
				if (detalleItem != null)
				{
					model.FechaEntrega = detalleItem.oc_entrega_fecha;
					model.SucursalEntrega = detalleItem.adm_nombre;
					model.Observaciones = detalleItem.oc_observaciones;
					model.PagoAnticipado = detalleItem.oc_pago_ant == 'S' ? true : false;
					model.PagoPlazo = detalleItem.oc_pago_ant_vto.Value;
					CargarDatosDeConceptosEnTabDetalle(model, detalleItem);
				}
				return PartialView("_grillaDetalleDeOC", model);
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

		/// <summary>
		/// Busca las RPR (Requisiciones de Pedido de Reposición) asociadas a una Orden de Compra (OC) específica.
		/// </summary>
		/// <param name="ocCompte">El identificador de la Orden de Compra.</param>
		/// <returns>Una vista parcial con la grilla de RPR asociadas a la OC.</returns>
		/// <remarks>
		/// Este método carga las RPR asociadas a la OC desde el servicio de productos y las presenta en una grilla.
		/// En caso de error, retorna un mensaje de error en una vista parcial.
		/// </remarks>
		[HttpPost]
		public async Task<IActionResult> BuscarRprAsociadasDeOrdenDeCompra(string ocCompte)
		{
			GridCore<OrdenDeCompraRprAsociadasDto> grilla = new();
			try
			{
				var detalle = _productoServicio.CargarRprAsociadaDeOC(ocCompte, TokenCookie).Result;
				grilla = ObtenerGridCore<OrdenDeCompraRprAsociadasDto>(detalle);
				return PartialView("_grillaRprDeOC", grilla);
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

		//TODO MARCE: Probar este método y verificar que funcione correctamente, ademas probar que levante la lista admin en el primer tab
		/// <summary>
		/// Modifica el estado de una Orden de Compra (OC) específica.
		/// </summary>
		/// <param name="ocCompte">El identificador de la Orden de Compra.</param>
		/// <param name="opt">El nuevo estado a asignar a la Orden de Compra.</param>
		/// <returns>Un objeto JSON con el resultado de la operación, incluyendo el nuevo estado de la OC.</returns>
		/// <remarks>
		/// Este método busca el nuevo estado en la lista de estados de Orden de Compra y actualiza la OC con el nuevo estado.
		/// En caso de error, retorna un mensaje de error en formato JSON.
		/// </remarks>
		[HttpPost]
		public JsonResult ModificarOC(ModificarOCRequest request)
		{
			try
			{
				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				var respuesta = _productoServicio.ModificarOrdenDeCompra(request, TokenCookie).Result;
				if (respuesta == null)
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos de la Orden de Compra. Oc_Compte: {request.oc_compte}" });
				if (respuesta.Entidad == null)
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos de la Orden de Compra. Oc_Compte: {request.oc_compte}" });
				if (respuesta.Entidad.resultado != 0)
					return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos de la Orden de Compra. Oc_Compte: {request.oc_compte} Mensaje: {respuesta.Entidad.resultado_msj}" });
				var obtenerOcPorCompte = _productoServicio.ObtenerOrdenDeCompraPorOcCompte(request.oc_compte, TokenCookie).Result.First();
				//return AnalizarRespuesta(respuesta);
				return Json(new msgRes() { error = false, warn = false, msg = string.Empty, data = new { oc_compte = request.oc_compte, oce_id = obtenerOcPorCompte.Oce_Id, oce_desc = obtenerOcPorCompte.Oce_Desc, adm_id = obtenerOcPorCompte.Adm_Id, adm_nombre = obtenerOcPorCompte.Adm_Nombre } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos de la Orden de Compra. Oc_Compte: {request.oc_compte}" });
			}
		}

		/// <summary>
		/// Busca las sucursales cuyo nombre contiene el prefijo especificado.
		/// </summary>
		/// <param name="prefix">El prefijo a buscar en los nombres de las sucursales.</param>
		/// <returns>Un objeto JSON con la lista de sucursales que coinciden con el prefijo.</returns>
		/// <remarks>
		/// Este método busca en la lista de administraciones cargadas las que contienen el prefijo especificado en su nombre.
		/// Si la lista de administraciones no está cargada, la obtiene del servicio de administraciones.
		/// </remarks>
		[HttpPost]
		public JsonResult BuscarSucursales(string prefix)
		{
			if (AdministracionesLista == null || AdministracionesLista.Count <= 0)
			{
				ObtenerAdministracionesLista(_adminServicio);
			}
			var adms = AdministracionesLista.Where(x => x.Adm_nombre.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return Json(lista);
		}

		/// <summary>
		/// Busca los estados de las Ordenes de Compra (OC) cuyo nombre contiene el prefijo especificado.
		/// </summary>
		/// <param name="prefix">El prefijo a buscar en los nombres de los estados de las OC.</param>
		/// <returns>Un objeto JSON con la lista de estados de OC que coinciden con el prefijo.</returns>
		/// <remarks>
		/// Este método busca en la lista de estados de Orden de Compra cargados los que contienen el prefijo especificado en su nombre.
		/// Si la lista de estados de Orden de Compra no está cargada, la obtiene del servicio de estados de Orden de Compra.
		/// </remarks>
		[HttpPost]
		public JsonResult BuscarEstadosDeOC(string prefix)
		{
			if (OrdenDeCompraEstadoLista == null || OrdenDeCompraEstadoLista.Count <= 0)
			{
				ObtenerOrdenDeCompraEstadoLista(_ordenDeCompraEstadoServicio);
			}
			var ocs = OrdenDeCompraEstadoLista.Where(x => x.oce_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = ocs.Select(x => new ComboGenDto { Id = x.oce_id, Descripcion = x.oce_lista });
			return Json(lista);
		}

		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio);
			}

			if (OrdenDeCompraEstadoLista.Count == 0 || actualizar)
			{
				ObtenerOrdenDeCompraEstadoLista(_ordenDeCompraEstadoServicio);
			}

			if (AdministracionesLista.Count == 0 || actualizar)
			{
				ObtenerAdministracionesLista(_adminServicio);
			}
		}

		private void CargarDatosDeConceptosEnTabDetalle(ConsultaOCDetalleModel model, OrdenDeCompraDetalleDto item)
		{
			var grillaTemp = new List<OrdenDeCompraConceptoDto>();
			var itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 0,
				Concepto = "Subtotal",
				Importe = item.oc_gravado + item.oc_no_gravado + item.oc_exento
			};
			grillaTemp.Add(itemConcepto);
			itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 1,
				Concepto = "II",
				Importe = item.oc_in
			};
			grillaTemp.Add(itemConcepto);
			itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 2,
				Concepto = "Flete",
				Importe = item.oc_flete_importe
			};
			grillaTemp.Add(itemConcepto);
			itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 3,
				Concepto = "IVA",
				Importe = item.oc_iva + item.oc_flete_iva
			};
			grillaTemp.Add(itemConcepto);
			itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 4,
				Concepto = "Percepciones",
				Importe = item.oc_percepciones
			};
			grillaTemp.Add(itemConcepto);
			itemConcepto = new OrdenDeCompraConceptoDto
			{
				Orden = 5,
				Concepto = "TOTAL",
				Importe = grillaTemp.Sum(x => x.Importe)
			};
			grillaTemp.Add(itemConcepto);
			model.ResumenGrilla = ObtenerGridCore<OrdenDeCompraConceptoDto>(grillaTemp);
		}
		#endregion

		#region Clases Privadas

		private class msgRes()
		{
			public bool error { get; set; }
			public bool warn { get; set; }
			public string msg { get; set; } = string.Empty;
			public object data { get; set; }
		}
		#endregion
	}
}
