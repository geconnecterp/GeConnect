using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using gc.sitio.Areas.Compras.Models;
using System.Security.Claims;
using Azure.Core;
using System.Security.Cryptography;
using System;
using System.Drawing;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDeCompraController : OrdenDeCompraControladorBase
	{
		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		public OrdenDeCompraController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio, ILogger<OrdenDeCompraController> logger,
									   IOptions<AppSettings> options, IHttpContextAccessor context) : base(options, context, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
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

				ViewData["Titulo"] = "ORDEN DE COMPRA";
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

		public async Task<IActionResult> BuscarProductos(NCPICargarListaDeProductos2Request request)
		{
			MetadataGrid metadata;
			GridCore<ProductoNCPIDto> grillaDatos;
			try
			{
				if (request.Rel01 == null || request.Rel01.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Se debe proporcionar una cuenta."
					};
					return PartialView("_gridMensaje", response);
				}

				request.Registros = _settings.NroRegistrosPagina;
				request.Adm_Id = AdministracionId;
				request.Usu_Id = UserName;
				var productos = _productoServicio.NCPICargarListaDeProductosPag2(request, TokenCookie).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				ListaProductos = productos.Item1;
				grillaDatos = GenerarGrilla(productos.Item1, request.Sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir);
				return PartialView("_grillaProductos", grillaDatos);
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

		public async Task<IActionResult> BuscarProductosTabOC(string ctaId, string ocCompte)
		{
			ProductoParaOcModel model = new();
			GridCore<ProductoParaOcDto> grillaDatos;
			try
			{
				CtaIdSelected = ctaId;
				CargarProductoParaOcRequest request = new()
				{
					Adm_Id = AdministracionId,
					Usu_Id = UserName,
					Cta_Id = ctaId,
					Nueva = string.IsNullOrEmpty(ocCompte),
					Oc_Compte = ocCompte
				};
				var productos = _productoServicio.CargarProductosDeOC(request, TokenCookie).Result;
				grillaDatos = ObtenerGridCore<ProductoParaOcDto>(productos);
				ListaProductosOC = productos;
				model.ListaOC = grillaDatos;
				CalcularTotalesParaOC(model, productos);
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
		public IActionResult AgregarProductoEnOC(string pId)
		{
			try
			{
				ProductoParaOcModel model = new();
				GridCore<ProductoParaOcDto> grillaDatos;

				if (ListaProductos != null && ListaProductos.Count > 0)
				{
					var producto = ListaProductos.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						var productos = ListaProductosOC;
						if (productos == null)
						{
							productos = [];
						}
						productos.Add(new ProductoParaOcDto(producto));
						grillaDatos = ObtenerGridCore<ProductoParaOcDto>(productos);
						ListaProductosOC = productos;
						model.ListaOC = grillaDatos;
						CalcularTotalesParaOC(model, productos);
						return PartialView("_grillaProductosOC", model);
					}
				}

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
		public IActionResult QuitarProductoEnOc(string pId)
		{
			try
			{
				ProductoParaOcModel model = new();
				GridCore<ProductoParaOcDto> grillaDatos;
				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					var producto = ListaProductosOC.FirstOrDefault(x => x.P_Id == pId);
					if (producto != null)
					{
						var productos = ListaProductosOC.Where(x => x.P_Id != pId).ToList();
						grillaDatos = ObtenerGridCore<ProductoParaOcDto>(productos);
						ListaProductosOC = productos;
						model.ListaOC = grillaDatos;
						CalcularTotalesParaOC(model, productos);
						return PartialView("_grillaProductosOC", model);
					}
				}

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

		/// <summary>
		/// Funcion que actualiza los valores de un producto seleccionado en la Grilla de OC (Segundo Tab)
		/// Los valores a actualizar son Pedido +Boni, Precio Costo, Total Costo, Total Pallet
		/// </summary>
		/// <param name="pId">ID del producto seleccionado</param>
		/// <param name="field">Campo que se ha editado, los cuales pueden ser: Pedido Bultos, Precio de Lista, Dto1, Dto2, Dto3, Dto4, DtoPa, Bonificacion</param>
		/// <param name="val">Valor correspondiente al campo editado</param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult ActualizarProductoEnOc(string pId, string field, string val)
		{
			ProductoParaOcModel model = new();
			List<ProductoParaOcDto> productos = new();
			try
			{
				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					productos = ListaProductosOC;
				}
				if (productos.Count > 0)
				{
					var producto = productos.FirstOrDefault(x => x.P_Id == pId);
					if (producto != null)
					{
						switch (field)
						{
							case "P_Dto1":
								producto.P_Dto1 = Convert.ToDecimal(val);
								break;
							case "P_Dto2":
								producto.P_Dto2 = Convert.ToDecimal(val);
								break;
							case "P_Dto3":
								producto.P_Dto3 = Convert.ToDecimal(val);
								break;
							case "P_Dto4":
								producto.P_Dto4 = Convert.ToDecimal(val);
								break;
							case "P_Dto_Pa":
								producto.P_Dto_Pa = Convert.ToDecimal(val);
								break;
							case "P_Plista":
								producto.P_Plista = Convert.ToDecimal(val);
								break;
							case "P_Boni":
								producto.P_Boni = val;
								var arr = val.Split('/');
								var res = Convert.ToInt32(arr[1]) - Convert.ToInt32(arr[0]); //En la bonificacion viene NNN/MMM donde sería "cada NNN, lleva MMM", siendo MMM mayor a NNN. La diferencia es el valor adicional que se suma al pedido.
								producto.Pedido_Mas_Boni = res + producto.Cantidad;
								break;
							case "Bultos":
								producto.Bultos = Convert.ToInt32(val);
								producto.Cantidad = producto.Bultos * producto.P_Unidad_Pres;
								break;
							default:
								break;
						}
						producto.P_Pcosto = ProductoParaOcDto.CalcularPCosto(producto.P_Plista, producto.P_Dto1, producto.P_Dto2, producto.P_Dto3, producto.P_Dto4, producto.P_Dto_Pa, producto.P_Boni, producto.P_Porc_Flete);
						producto.P_Pcosto_Total = producto.P_Pcosto * producto.Pedido_Mas_Boni;
						//MARCE TODO: Consultar con CR como era el tema del calculo de Total Pallet (me olvide)
					}
					ListaProductosOC = productos; //Actualizo la lista en memoria
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						data = new DatosDeProductoActualizado()
						{
							PedidoCantidad = producto.Cantidad,
							Pedido_Mas_Boni = producto.Pedido_Mas_Boni,
							P_Pcosto = producto.P_Pcosto,
							P_Pcosto_Total = producto.P_Pcosto_Total,
							Paletizado = producto.Paletizado,
							Total_Costo = productos.Sum(x => x.P_Pcosto_Total),
							Total_Pallet = productos.Sum(x => x.Paletizado)
						}
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en la OC" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {pId}" });
			}
		}

		//MARCE TODO: Seguir con este metodo
		[HttpPost]
		public IActionResult UpdateMasivoEnOc(ActualizacionMasivaRequest request)
		{
			ProductoParaOcModel model = new();
			List<ProductoParaOcDto> productos = new();
			try
			{
				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					model.ListaOC = ObtenerGridCore<ProductoParaOcDto>(ListaProductosOC);
					CalcularTotalesParaOC(model, ListaProductosOC);
					productos = ListaProductosOC;
				}
				if (request.pIds.Length > 0 && productos.Count > 0)
				{
					foreach (var pId in request.pIds)
					{
						var producto = productos.FirstOrDefault(x => x.P_Id == pId);
						if (producto != null)
						{
							producto.P_Dto1 = request.dto1;
							producto.P_Dto2 = request.dto2;
							producto.P_Dto3 = request.dto3;
							producto.P_Dto4 = request.dto4;
							producto.P_Dto_Pa = request.dpa;
							if (request.boolFlete) producto.P_Porc_Flete = request.flete;
						}
					}
					ListaProductosOC = productos;
					model.ListaOC = ObtenerGridCore<ProductoParaOcDto>(productos);
					CalcularTotalesParaOC(model, productos);
				}
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
		public JsonResult ObtenerTopesDeOc()
		{
			try
			{
				var listaTopes = _productoServicio.CargarTopesDeOC(AdministracionId, TokenCookie).Result;
				if (listaTopes == null)
					return Json(new msgRes() { error = false, warn = true, msg = "Sin datos de tope de OC.", data = new TopeOC() });
				if (listaTopes.Count == 0)
					return Json(new msgRes() { error = false, warn = true, msg = "Sin datos de tope de OC.", data = new TopeOC() });

				var tope = listaTopes.First();
				//return Json(new msgRes() { error = false, warn = false, msg = string.Empty, data = new TopeOC() { oc_limite_semanal = tope.oc_limite_semanal + Convert.ToDecimal(0.01), oc_emitidas = tope.oc_emitidas, oc_tope = tope.oc_tope } });
				return Json(new msgRes() { error = false, warn = false, msg = string.Empty, data = new TopeOC() { oc_limite_semanal = tope.oc_limite_semanal, oc_emitidas = tope.oc_emitidas, oc_tope = tope.oc_tope } });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los topes de OC. AdmId: {AdministracionId}" });
			}
		}



		//Invocar cuando se haya seleccionado solo un proveedor desde el filtro base.
		[HttpPost]
		public JsonResult BuscarFamiliaDesdeProveedorSeleccionado(string ctaId)
		{
			try
			{
				CtaIdSelected = ctaId;
				CargarProveedoresFamiliaLista(ctaId, _cuentaServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de la familia de productos del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public JsonResult CargarOCDesdeProveedorSeleccionado(string ctaId)
		{
			try
			{
				CtaIdSelected = ctaId;
				CargarOrdenesDeCompraLista(ctaId, _productoServicio);
				return Json(new { error = false, warn = false, msg = string.Empty });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de las OC del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public JsonResult BuscarFlias(string prefix)
		{
			if ((ProveedorFamiliaLista == null || ProveedorFamiliaLista.Count <= 0) && (!string.IsNullOrEmpty(CtaIdSelected)))
			{
				BuscarFamiliaDesdeProveedorSeleccionado(CtaIdSelected);
			}
			var rub = ProveedorFamiliaLista.Where(x => x.pg_desc.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var rubros = rub.Select(x => new ComboGenDto { Id = x.pg_id, Descripcion = x.pg_lista });
			return Json(rubros);
		}

		[HttpPost]
		public JsonResult BuscarOCPendientes(string prefix)
		{
			var oc = OrdenDeCompraLista.Where(x => x.oc_compte.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var ocs = oc.Select(x => new ComboGenDto { Id = x.oc_compte, Descripcion = x.oc_compte });
			return Json(ocs);
		}

		#region Métodos privados


		private void CalcularTotalesParaOC(ProductoParaOcModel model, List<ProductoParaOcDto> productos)
		{
			if (productos == null || productos.Count == 0)
			{
				model.Total_Costo = "0.00";
				model.Total_Pallet = "0.00";
			}
			else
			{
				model.Total_Costo = productos.Sum(x => x.P_Pcosto_Total).ToString("0.##");
				model.Total_Pallet = productos.Sum(x => x.Paletizado).ToString("0.##");
			}
		}
		private static void ObtenerColor(ref List<ProductoNCPIDto> listaProd)
		{
			foreach (var item in listaProd)
			{
				if (item.p_activo == "D") //Discontinuo
					item.Row_color = "#fc4641";
			}
		}
		protected void CargarProveedoresFamiliaLista(string ctaId, ICuentaServicio _cuentaServicio, string? fam = null)
		{
			var adms = _cuentaServicio.ObtenerListaProveedoresFamilia(ctaId, TokenCookie);
			ProveedorFamiliaLista = adms;
		}
		protected void CargarOrdenesDeCompraLista(string ctaId, IProductoServicio _productoServicio)
		{
			var adms = _productoServicio.CargarOrdenesDeCompraList(ctaId, AdministracionId, UserName, TokenCookie);
			OrdenDeCompraLista = adms;
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio);
			}

			if (RubroLista.Count == 0 || actualizar)
			{
				ObtenerRubros(_rubroServicio);
			}
		}
		#endregion

		#region Clases locales
		private class DatosDeProductoActualizado()
		{
			public decimal PedidoCantidad { get; set; }
			public decimal Pedido_Mas_Boni { get; set; }
			public decimal P_Pcosto { get; set; }
			public decimal P_Pcosto_Total { get; set; }
			public decimal Paletizado { get; set; }
			public decimal Total_Costo { get; set; }
			public decimal Total_Pallet { get; set; }
		}

		private class msgRes()
		{
			public bool error { get; set; }
			public bool warn { get; set; }
			public string msg { get; set; } = string.Empty;
			public object data { get; set; } = new TopeOC();
		}

		private class TopeOC()
		{
			public decimal oc_limite_semanal { get; set; } = 0.00M;
			public decimal oc_emitidas { get; set; } = 0.00M;
			public decimal oc_tope { get; set; } = 0.00M;
		}

		public class ActualizacionMasivaRequest()
		{
			public string[] pIds { get; set; }
			public decimal dto1 { get; set; }
			public decimal dto2 { get; set; }
			public decimal dto3 { get; set; }
			public decimal dto4 { get; set; }
			public decimal dpa { get; set; }
			public bool boolFlete { get; set; }
			public decimal flete { get; set; }
		}
		#endregion
	}
}
