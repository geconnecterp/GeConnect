using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using log4net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDeCompraController : OrdenDeCompraControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.OC.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _settings;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRubroServicio _rubroServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly IAdministracionServicio _adminServicio;
		public OrdenDeCompraController(ICuentaServicio cuentaServicio, IRubroServicio rubroServicio, IProductoServicio productoServicio, ILogger<OrdenDeCompraController> logger,
									   IAdministracionServicio adminServicio, IOptions<AppSettings> options, IHttpContextAccessor context, IOptions<DocsManager> docsManager,
									   IDocManagerServicio docManager) : base(options, context, logger)
		{
			_settings = options.Value;
			_cuentaServicio = cuentaServicio;
			_rubroServicio = rubroServicio;
			_productoServicio = productoServicio;
			_adminServicio = adminServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: COP
			_docMSv = docManager; //instancio el servicio de impresión
		}
		public IActionResult Index()
		{
			var model = new OrdenDeCompraCargaDto();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				string titulo = "ORDEN DE COMPRA";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				CargarDatosIniciales(true);
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

		#region Laboratorio para abrir desde NCPI
		[HttpPost]
		[ActionName("IndexConParametros")]
		public IActionResult Index(string pId, string ctaId, string ctaDeno)
		{
			var model = new OrdenDeCompraCargaDto();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				string titulo = "ORDEN DE COMPRA";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

				CargarDatosIniciales(true);

				model.p_id = pId;
				model.cta_id = ctaId;
				model.cta_denominacion = ctaDeno;
				return View("Index", model);
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
		#endregion

		public IActionResult ObtenerProveedoresFamilia(string ctaId)
		{
			var model = new NDeCYPI.ProveedoresFamiliaDto();
			try
			{
				model.ComboProveedoresFamilia = ComboProveedoresFamilia(ctaId, _cuentaServicio);
				return PartialView("_listaProveedoresFamilia", model);
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

		public IActionResult ObtenerRubros()
		{
			var model = new ListaRubroModel();
			try
			{
				model.ListaRubros = ComboRubros();
				return PartialView("_listaRubros", model);
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

		public IActionResult BuscarProductos(NCPICargarListaDeProductos2Request request)
		{
			MetadataGrid metadata;
			GridCoreSmart<ProductoNCPIDto> grillaDatos;
			CargarOcBuscarProductoModel model = new CargarOcBuscarProductoModel();
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
				request.Adm_id = AdministracionId;
				request.Usu_id = UserName;
				var productos = _productoServicio.NCPICargarListaDeProductosPag2(request, TokenCookie).Result;
				ObtenerColor(ref productos.Item1);
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				ListaProductos = productos.Item1;

				var nnCol = request.Sort ?? string.Empty;
				var sortDir = request.SortDir ?? "ASC";

				grillaDatos = GenerarGrillaSmart(productos.Item1, nnCol, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, sortDir);
				model.grillaDatos = grillaDatos;
				model.ComboSucursales = ComboSucursales();
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

		//public async Task<IActionResult> BuscarProductosTabOC(string ctaId, string ocCompte)
		public IActionResult BuscarProductosTabOC(string ctaId, string ocCompte)
		{
			ProductoParaOcModel model = new();
			GridCoreSmart<ProductoParaOcDto> grillaDatos;
			try
			{
				CtaIdSelected = ctaId;
				CargarProductoParaOcRequest request = new()
				{
					adm_id = AdministracionId,
					usu_id = UserName,
					Cta_Id = ctaId,
					Nueva = string.IsNullOrEmpty(ocCompte),
					Oc_Compte = ocCompte
				};
				var productos = _productoServicio.CargarProductosDeOC(request, TokenCookie).Result;
				CalcularPedidoMasBoniMasivo(productos);
				grillaDatos = ObtenerGridCoreSmart<ProductoParaOcDto>(productos);
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
				GridCoreSmart<ProductoParaOcDto> grillaDatos;

				if (ListaProductos != null && ListaProductos.Count > 0)
				{
					var producto = ListaProductos.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						var productos = ListaProductosOC;
						if (productos == null)
						{
							productos = [];
							productos.Add(new ProductoParaOcDto(producto));
						}
						else
						{
							if (!productos.Where(x => x.p_id == pId).Any())
							{
								productos.Add(new ProductoParaOcDto(producto));
							}
						}
						grillaDatos = ObtenerGridCoreSmart<ProductoParaOcDto>(productos);
						ListaProductosOC = productos;
						model.ListaOC = grillaDatos;
						CalcularTotalesParaOC(model, productos);

						return PartialView("_grillaProductosOC", model);
					}
				}

				Console.WriteLine("Session ID: " + HttpContext.Session.Id);
				Console.WriteLine("Productos en sesión: " + ListaProductosOC?.Count);

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
				GridCoreSmart<ProductoParaOcDto> grillaDatos;
				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					var producto = ListaProductosOC.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						var productos = ListaProductosOC.Where(x => x.p_id != pId).ToList();
						grillaDatos = ObtenerGridCoreSmart<ProductoParaOcDto>(productos);
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
			List<ProductoParaOcDto> productos = new();
			try
			{
				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					productos = ListaProductosOC;
				}
				if (productos.Count > 0)
				{
					var datosDelProductoParaEnviar = new DatosDeProductoActualizado();
					var producto = productos.FirstOrDefault(x => x.p_id == pId);
					if (producto != null)
					{
						if (field.Contains("p_dto1"))
						{
							val = val.Replace(",", ".");
							producto.p_dto1 = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_dto2"))
						{
							val = val.Replace(",", ".");
							producto.p_dto2 = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_dto3"))
						{
							val = val.Replace(",", ".");
							producto.p_dto3 = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_dto4"))
						{
							val = val.Replace(",", ".");
							producto.p_dto4 = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_dto_pa"))
						{
							val = val.Replace(",", ".");
							producto.p_dto_pa = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_plista"))
						{
							val = val.Replace(",", "");
							producto.p_plista = Convert.ToDecimal(val);
						}
						else if (field.Contains("p_boni"))
						{
							producto.p_boni = val;
						}
						else if (field.Contains("bultos"))
						{
							val = val.Replace(",", "");
							producto.bultos = Convert.ToInt32(val);
							producto.cantidad = producto.bultos * producto.p_unidad_pres;
						}

						producto.pedido_mas_boni = Math.Round(CalcularPedidoMasBoni(producto.p_boni, producto), 1);
						producto.p_pcosto = Math.Round(ProductoParaOcDto.CalcularPCosto(producto.p_plista, producto.p_dto1, producto.p_dto2, producto.p_dto3, producto.p_dto4, producto.p_dto_pa, producto.p_boni, producto.p_porc_flete), 2);
						producto.p_pcosto_total = Math.Round(producto.p_pcosto * (producto.pedido_mas_boni == 0.0M ? 1.0M : producto.pedido_mas_boni), 2);
						producto.paletizado = Math.Round((producto.pedido_mas_boni == 0.0M ? 1.0M : producto.pedido_mas_boni) / producto.p_unidad_palet, 1);
						producto.cantidad_total = producto.cantidad + producto.bonificados;

						datosDelProductoParaEnviar = new DatosDeProductoActualizado()
						{
							PedidoCantidad = producto.cantidad,
							Pedido_Mas_Boni = producto.pedido_mas_boni,
							P_Pcosto = producto.p_pcosto,
							P_Pcosto_Total = producto.p_pcosto_total,
							Paletizado = producto.paletizado,
							Total_Costo = productos.Sum(x => x.p_pcosto_total),
							Total_Pallet = productos.Sum(x => x.paletizado),
							PermiteDecimales = producto.up_tipo != "N"
						};
					}
					ListaProductosOC = productos; //Actualizo la lista en memoria
					return Json(new msgRes()
					{
						error = false,
						warn = false,
						msg = string.Empty,
						data = datosDelProductoParaEnviar
					});
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen productos cargados en la OC" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos del producto recientemente editado. Id de Producto: {pId}" });
			}
		}

		[HttpPost]
		public IActionResult UpdateMasivoEnOc(ActualizacionMasivaRequest request)
		{
			ProductoParaOcModel model = new();
			List<ProductoParaOcDto> productos = new();
			try
			{
				_logger.LogInformation("Session ID: " + HttpContext.Session.Id);
				_logger.LogInformation("Productos en sesión: " + ListaProductosOC?.Count);


				if (ListaProductosOC != null && ListaProductosOC.Count > 0)
				{
					model.ListaOC = ObtenerGridCoreSmart<ProductoParaOcDto>(ListaProductosOC);
					CalcularTotalesParaOC(model, ListaProductosOC);
					productos = ListaProductosOC;
				}
				if (request.pIds.Length > 0 && productos.Count > 0)
				{
					foreach (var pId in request.pIds)
					{
						var producto = productos.FirstOrDefault(x => x.p_id == pId);
						if (producto != null)
						{
							producto.p_dto1 = request.dto1;
							producto.p_dto2 = request.dto2;
							producto.p_dto3 = request.dto3;
							producto.p_dto4 = request.dto4;
							producto.p_dto_pa = request.dpa;
							if (request.boolFlete) producto.p_porc_flete = request.flete;
							producto.p_pcosto = Math.Round(ProductoParaOcDto.CalcularPCosto(producto.p_plista, producto.p_dto1, producto.p_dto2, producto.p_dto3, producto.p_dto4, producto.p_dto_pa, producto.p_boni, producto.p_porc_flete), 2);
							producto.p_pcosto_total = Math.Round(producto.p_pcosto * ((producto.pedido_mas_boni == 0.0M ? 1.0M : producto.pedido_mas_boni) + producto.cantidad), 2);
							producto.paletizado = Math.Round((producto.cantidad + (producto.pedido_mas_boni == 0.0M ? 1.0M : producto.pedido_mas_boni)) / producto.p_unidad_palet, 1);
						}
					}
					ListaProductosOC = productos;
					model.ListaOC = ObtenerGridCoreSmart<ProductoParaOcDto>(productos);
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
		public IActionResult CargarResumenDeOc(string oc_compte)
		{
			try
			{
				//CtaIdSelected
				//AdministracionId
				//UserName
				var jsonstring = JsonConvert.SerializeObject(ListaProductosOC, new JsonSerializerSettings());
				var resumen = _productoServicio.CargarResumenDeOC(new CargarResumenDeOCRequest
				{
					Cta_Id = CtaIdSelected,
					Adm_Id = AdministracionId,
					Usu_Id = UserName,
					Nueva = string.IsNullOrEmpty(oc_compte),
					Oc_Compte = oc_compte,
					Entrega_Fecha = DateTime.Now,
					Entrega_Adm = AdministracionId,
					Pago_Anticipado = 'N',
					Pago_Fecha = DateTime.Now.AddDays(1),
					Observaciones = string.Empty,
					Oce_Id = 'P',
					Json = jsonstring
				}, TokenCookie).Result;

				var model = new ResumenOCModel
				{
					SucursalEntrega = ObtenerComboAdministraciones(_adminServicio.ObtenerAdministraciones("S", TokenCookie)),
					AdmId = AdministracionId,
					FechaEntrega = DateTime.Now,
					PagoAnticipado = false,
					PagoPlazo = DateTime.Now.AddDays(1),
					Obs = string.Empty,
					DejarOCActiva = false,
					ResumenGrilla = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(resumen)
				};
				return PartialView("_resumen", model);
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
		public IActionResult ObtenerConcepto(ActualizarConceptosRequest request)
		{
			try
			{
				var model = new ConceptoModel();
				var jsonstring = JsonConvert.SerializeObject(ListaProductosOC, new JsonSerializerSettings());
				var resumen = _productoServicio.CargarResumenDeOC(new CargarResumenDeOCRequest
				{
					Cta_Id = CtaIdSelected,
					Adm_Id = AdministracionId,
					Usu_Id = UserName,
					Nueva = string.IsNullOrEmpty(request.Oc_Compte),
					Oc_Compte = request.Oc_Compte,
					Entrega_Fecha = request.Entrega_Fecha,
					Entrega_Adm = request.Entrega_Adm,
					Pago_Anticipado = request.Pago_Anticipado,
					Pago_Fecha = request.Pago_Fecha,
					Observaciones = request.Observaciones,
					Oce_Id = request.Oce_Id,
					Json = jsonstring
				}, TokenCookie).Result;
				model.ResumenGrilla = ObtenerGridCoreSmart<OrdenDeCompraConceptoDto>(resumen);
				return PartialView("_gridConceptos", model);
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
		public IActionResult CargarSucursalesParInfoAdicional()
		{
			ListaSucursalesModel model = new()
			{
				ComboSucursales = ComboSucursales()
			};
			return PartialView("_listaSucursales", model);
		}

		[HttpPost]
		public JsonResult ConfirmarOrdenDeCompra(ConfirmarOCRequest request)
		{
			try
			{
				if (ListaProductosOC == null || ListaProductosOC.Count <= 0) return Json(new { error = true, warn = false, msg = $"No existen productos cargados en la OC" });
				if (string.IsNullOrEmpty(CtaIdSelected)) return Json(new { error = true, warn = false, msg = $"Se ha producido un error al selecciona la cuenta." });
				request.Adm_Id = AdministracionId;
				request.Usu_Id = UserName;
				request.Cta_Id = CtaIdSelected;
				request.Nueva = string.IsNullOrEmpty(request.Oc_Compte);
				request.Json = JsonConvert.SerializeObject(ListaProductosOC, new JsonSerializerSettings());
				PrintProperties(request);
				var respuesta = _productoServicio.ConfirmarOrdenDeCompra(request, TokenCookie).Result;
				return AnalizarRespuesta(respuesta, "La Orden de Compra se Confirmo con Éxito");
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar confirmar los datos de la orden de compra" });
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
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los topes de OC. AdmId: {AdministracionId}" });
			}
		}

		[HttpPost]
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				CtaIdSelected = "";
				ListaProductos = [];
				ListaProductosOC = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - ORDENDECOMPRA" });
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
			catch (Exception)
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
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar obtener los datos de las OC del proveedor: {ctaId}" });
			}

		}

		[HttpPost]
		public IActionResult BuscarOCDesdeCtaIdSeleccionado(string ctaId)
		{
			var model = new ListaOcPendienteModel();
			try
			{
				CtaIdSelected = ctaId;
				CargarOrdenesDeCompraLista(ctaId, _productoServicio);
				model.LstOcPendiente = ComboOcPendientes();
				//if (OrdenDeCompraLista !=null && OrdenDeCompraLista.Count>0)
				//	model.oc_compte = OrdenDeCompraLista.First().oc_compte;
				//else
				model.oc_compte = string.Empty;
				return PartialView("_listaOcPendientes", model);
			}
			catch (Exception)
			{
				return PartialView("_empty_view");
			}

		}

		protected SelectList ComboOcPendientes()
		{
			var lista = OrdenDeCompraLista.Select(x => new ComboGenDto { Id = x.oc_compte.ToString(), Descripcion = $"{x.oc_compte} {x.oc_fecha.ToShortDateString()}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		[HttpPost]
		public JsonResult BuscarFlias(string prefix)
		{
			if ((ProveedorFamiliaLista == null || ProveedorFamiliaLista.Count <= 0) && (!string.IsNullOrEmpty(CtaIdSelected)))
			{
				BuscarFamiliaDesdeProveedorSeleccionado(CtaIdSelected);
			}
			ProveedorFamiliaLista ??= [];
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

		#region Información Adicional
		public async Task<IActionResult> BuscarInfoProdIExMeses(string pId, string admId, int meses)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProdIExMesDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProdIExMes(admId, pId, meses, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExMesDto>(info);
				return PartialView("_infoProdIExMeses", model);
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

		public async Task<IActionResult> BuscarInfoProdIExSemanas(string pId, string admId, int semanas)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProdIExSemanaDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProdIExSemana(admId, pId, semanas, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExSemanaDto>(info);
				return PartialView("_infoProdIExSemanas", model);
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

		public async Task<IActionResult> BuscarInfoProdStkDeposito(string pId, string admId)
		{
			var model = new GridCoreSmart<InfoProdStkD>();
			try
			{
				var info = await _productoServicio.InfoProductoStkD(pId, AdministracionId, TokenCookie);
				model = ObtenerGridCoreSmart<InfoProdStkD>(info);
				return PartialView("_infoProdPorDeposito", model);
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

		public async Task<IActionResult> BuscarInfoProdStkSucursal(string pId, string admId)
		{
			var model = new GridCoreSmart<InfoProdStkA>();
			try
			{
				var info = await _productoServicio.InfoProductoStkA(pId, AdministracionId, TokenCookie);
				model = ObtenerGridCoreSmart<InfoProdStkA>(info);
				return PartialView("_infoProdPorSucursal", model);
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

		public async Task<IActionResult> BuscarInfoProdSustituto(string pId, string tipo, bool soloProv)
		{
			var model = new GridCoreSmart<ProductoNCPISustitutoDto>();
			try
			{
				var info = await _productoServicio.InfoProdSustituto(pId, tipo, AdministracionId, soloProv, TokenCookie);
				model = ObtenerGridCoreSmart<ProductoNCPISustitutoDto>(info);
				return PartialView("_infoProdSustituto", model);
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

		public async Task<IActionResult> BuscarInfoProd(string pId)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProductoDto>();
			try
			{
				var info = await _productoServicio.InfoProd(pId, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProductoDto>(info);
				return PartialView("_infoProducto", model);
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
		#endregion

		#region Métodos privados
		private SelectList ComboRubros()
		{
			var adms = _rubroServicio.ObtenerListaRubros("", TokenCookie);
			var lista = adms.Select(x => new ComboGenDto { Id = x.Rub_Id, Descripcion = x.Rub_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboSucursales()
		{
			var adms = _adminServicio.GetAdministracionLogin();
			var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CalcularPedidoMasBoniMasivo(List<ProductoParaOcDto> lista)
		{
			foreach (var item in lista)
			{
				CalcularPedidoMasBoni(item);
			}
		}
		private void CalcularPedidoMasBoni(ProductoParaOcDto producto)
		{
			if (string.IsNullOrWhiteSpace(producto.p_boni))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return;
			}

			var arr = producto.p_boni.Split('/');
			if (!int.TryParse(arr[0], out int num))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return;
			}

			if (!int.TryParse(arr[1], out int den))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return;
			}

			if (num > den)
			{
				producto.pedido_mas_boni = producto.cantidad;
				return;
			}

			var res = den - num; //En la bonificacion viene NNN/MMM donde sería "cada NNN, lleva MMM", siendo MMM mayor a NNN. La diferencia es el valor adicional que se suma al pedido.
			var multiplo = producto.cantidad / num;
			if (multiplo > 0)
			{
				producto.bonificados = (res * (int)multiplo);
				producto.pedido_mas_boni = producto.bonificados + producto.cantidad;
			}
			else
			{
				producto.bonificados = 0;
				producto.pedido_mas_boni = producto.cantidad;
			}
			return;
		}
		private decimal CalcularPedidoMasBoni(string val, ProductoParaOcDto producto)
		{
			if (string.IsNullOrWhiteSpace(val))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return producto.pedido_mas_boni;
			}
			var arr = val.Split('/');
			if (!int.TryParse(arr[0], out int num))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return producto.pedido_mas_boni;
			}
			if (!int.TryParse(arr[1], out int den))
			{
				producto.pedido_mas_boni = producto.cantidad;
				return producto.pedido_mas_boni;
			}
			if (num > den)
			{
				producto.pedido_mas_boni = producto.cantidad;
				return producto.pedido_mas_boni;
			}
			var res = den - num; //En la bonificacion viene NNN/MMM donde sería "cada NNN, lleva MMM", siendo MMM mayor a NNN. La diferencia es el valor adicional que se suma al pedido.
			var multiplo = producto.cantidad / num;
			if (multiplo > 0)
			{
				producto.bonificados = (res * (int)multiplo);
				producto.pedido_mas_boni = producto.bonificados + producto.cantidad;
			}
			else
			{
				producto.bonificados = 0;
				producto.pedido_mas_boni = producto.cantidad;
			}
			return producto.pedido_mas_boni;
		}
		private static SelectList ObtenerComboAdministraciones(List<AdministracionDto> lista)
		{
			return HelperMvc<ComboGenDto>.ListaGenerica(lista.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre }));
		}

		private void CalcularTotalesParaOC(ProductoParaOcModel model, List<ProductoParaOcDto> productos)
		{
			if (productos == null || productos.Count == 0)
			{
				model.Total_Costo = "0.00";
				model.Total_Pallet = "0.00";
				model.Precio_Costo = "0.00";
				model.Cant_Items = "0";
			}
			else
			{
				model.Total_Costo = productos.Sum(x => x.p_pcosto_total).ToString("0.##");
				model.Total_Pallet = productos.Sum(x => x.paletizado).ToString("0.##");
				model.Cant_Items = productos.Sum(x => x.pedido_mas_boni).ToString("0.##");
				model.Precio_Costo = productos.Sum(x => x.p_pcosto).ToString("0.##");
			}
		}
		private static void ObtenerColor(ref List<ProductoNCPIDto> listaProd)
		{
			/*
			 * ROJO (INACTIVO[N]) -> #ff0000
			 * VERDE (ACTIVO[S]) -> #33ff33
			 * AZUL (DISCONTINUO[D]) -> #0066cc
			 */
			foreach (var item in listaProd)
			{
				if (item.p_activo == "D") //Discontinuo
					item.Row_color = "#0066cc";
				//item.Row_color = "#fc4641";
				if (item.p_activo == "S") //Activo
					item.Row_color = "#33ff33";
				if (item.p_activo == "N") //Inactivo
					item.Row_color = "#ff0000";
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
				ObtenerProveedores(_cuentaServicio, "BI");
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
			public string P_Id { get; set; } = string.Empty;
			public bool PermiteDecimales { get; set; }
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
			public required string[] pIds { get; set; }
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
