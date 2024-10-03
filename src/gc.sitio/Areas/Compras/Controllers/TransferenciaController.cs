﻿using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Policy;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class TransferenciaController : ControladorBase
	{
		private readonly AppSettings _appSettings;
		private readonly ILogger<CompraController> _logger;
		private readonly IProductoServicio _productoServicio;

		public TransferenciaController(ILogger<CompraController> logger, IOptions<AppSettings> options, IHttpContextAccessor context, IProductoServicio productoServicio) : base(options, context)
		{
			_logger = logger;
			_appSettings = options.Value;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		public async Task<IActionResult> TRAutorizacionesLista()
		{
			var auth = EstaAutenticado;
			if (!auth.Item1 || auth.Item2 < DateTime.Now)
			{
				return RedirectToAction("Login", "Token", new { area = "seguridad" });
			}

			GridCore<TRPendienteDto> grid;
			try
			{
				// Carga por default al iniciar la pantalla
				var items = await _productoServicio.TRObtenerPendientes(AdministracionId, "%", "S", TokenCookie);
				grid = ObtenerGridCore<TRPendienteDto>(items);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener Autorizaciones de transferencias pendientes.");
				TempData["error"] = "Hubo algun problema al intentar obtener las Autorizaciones de transferencias pendientes. Si el problema persiste informe al Administrador";
				grid = new();
			}
			return View(grid);
		}

		public async Task<IActionResult> NuevaTR(string ti)
		{
			var model = new TRCRUDDto();
			Dictionary<string, string> sucursales = [];
			try
			{
				if (TRSucursalesLista != null && TRSucursalesLista.Count > 0)
					model.ListaAutSucursales = ObtenerGridCore<TRAutSucursalesDto>(TRSucursalesLista);
				else
				{
					var itemsAutSucursales = await _productoServicio.TRObtenerAutSucursales(AdministracionId, TokenCookie);
					model.ListaAutSucursales = ObtenerGridCore<TRAutSucursalesDto>(itemsAutSucursales);
					if (model.ListaAutSucursales != null && model.ListaAutSucursales.ListaDatos != null && model.ListaAutSucursales.ListaDatos.Count > 0)
					{

						itemsAutSucursales.ForEach(x => sucursales.Add(x.adm_id, x.adm_nombre));
						PreCargarListaAutPI(sucursales);
						VerificarSiSucursalTieneOrdenes(itemsAutSucursales);
					}
					TRSucursalesLista = itemsAutSucursales;
				}
				model.ListaPedidosSucursal = ObtenerGridCore<TRAutPIDto>([]);
				if (TRAutPedidosIncluidosILista != null)
					model.ListaPedidosIncluidos = ObtenerGridCore<TRAutPIDto>(TRAutPedidosIncluidosILista);
				else
					model.ListaPedidosIncluidos = ObtenerGridCore<TRAutPIDto>([]);
				model.ListaDepositosDeEnvio = ObtenerGridCore<TRAutDepoDto>([]);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener las Sucursales en transferencias pendientes.");
				TempData["error"] = "Hubo algun problema al intentar obtener las Sucursales en transferencias pendientes. Si el problema persiste informe al Administrador";
			}
			return PartialView("TRCrudAutorizacion", model);
		}

		public async Task<IActionResult> CargarPedidosPorSucursal(string admId)
		{
			var model = new GridCore<TRAutPIDto>();
			try
			{
				var itemsAutPI = await _productoServicio.TRObtenerAutPI(AdministracionId, admId, TokenCookie);
				model = ObtenerGridCore<TRAutPIDto>(itemsAutPI);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener los Pedidos Internos por Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener los Pedidos Internos por Sucursal. Si el problema persiste informe al Administrador";
			}
			return PartialView("_trPedidosPorSucursal", model);
		}

		public async Task<IActionResult> CargarDepositosInclPorSucursal(string admId)
		{
			var model = new GridCore<TRAutDepoDto>();
			try
			{
				var itemsAutDepo = await _productoServicio.TRObtenerAutDepositos(admId, TokenCookie);
				model = ObtenerGridCore<TRAutDepoDto>(itemsAutDepo);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener los Depositos Incluidos por Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener los Depositos Incluidos por Sucursal. Si el problema persiste informe al Administrador";
			}
			return PartialView("_trDepositosInclPorSucursal", model);
		}

		public async Task<IActionResult> VerDetallePedidoDeSucursal(string piCompte)
		{
			//TODO: Charlar con carlos para ver si modificamos lo que se muestra en el detalle de PI
			var model = new TRDetallePedidoDto();
			try
			{
				var itemsAutPIDetalle = await _productoServicio.TRObtenerAutPIDetalle(piCompte, TokenCookie);
				model.Detalle = ObtenerGridCore<TRAutPIDetalleDto>(itemsAutPIDetalle);
				model.Titulo = $"Detalle de Pedido {piCompte}";
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener el detalle del Pedido Interno por Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener el detalle del Pedido Interno por Sucursal. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al intentar obtener el detalle del Pedido Interno por Sucursal. Si el problema persiste informe al Administrador.");
			}
			return PartialView("_trDetalleDePedido", model);
		}

		public async Task<IActionResult> AgregarAPedidosIncluidosParaAutTR(string picompte)
		{
			var model = new GridCore<TRAutPIDto>();
			try
			{
				//No existe en la lista de pedidos incluidos, lo agrego
				var listaTemp = TRAutPedidosIncluidosILista;
				if (!listaTemp.Exists(x => x.pi_compte == picompte))
				{
					var itemAAgregar = TRAutPedidosSucursalLista.Where(x => x.pi_compte == picompte).First();
					if (itemAAgregar != null)
					{
						listaTemp.Add(itemAAgregar);
						TRAutPedidosIncluidosILista = listaTemp;
					}
				}
				model = ObtenerGridCore<TRAutPIDto>(listaTemp);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar agregar un pedido a la lista de Pedidos Incluidos.");
				TempData["error"] = "Hubo algun problema al intentar agregar un pedido a la lista de Pedidos Incluidos. Si el problema persiste informe al Administrador";
			}
			return PartialView("_trPedidosInclParaAutoTR", model);
		}

		public async Task<IActionResult> QuitarDePedidosIncluidosParaAutTR(string picompte)
		{
			var model = new GridCore<TRAutPIDto>();
			try
			{
				//Existe en la lista de pedidos incluidos, lo quito
				var listaTemp = TRAutPedidosIncluidosILista;
				if (listaTemp.Exists(x => x.pi_compte == picompte))
				{
					var itemTemp = listaTemp.Where(x => x.pi_compte == picompte).First();
					listaTemp.Remove(itemTemp);
					TRAutPedidosIncluidosILista = listaTemp;
				}
				model = ObtenerGridCore<TRAutPIDto>(listaTemp);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar agregar un pedido a la lista de Pedidos Incluidos.");
				TempData["error"] = "Hubo algun problema al intentar agregar un pedido a la lista de Pedidos Incluidos. Si el problema persiste informe al Administrador";
			}
			return PartialView("_trPedidosInclParaAutoTR", model);
		}

		public async Task<IActionResult> ActualizarInfoEnListaDeSucursalesTR()
		{
			var model = new GridCore<TRAutSucursalesDto>();
			try
			{
				var listaSucursal = TRSucursalesLista;
				foreach (var sucursalItem in listaSucursal)
				{
					if (TRAutPedidosIncluidosILista.Exists(x => x.adm_id.Equals(sucursalItem.adm_id)))
						sucursalItem.tiene_pi = true;
					else
						sucursalItem.tiene_pi = false;
				}
				TRSucursalesLista = listaSucursal;
				model = ObtenerGridCore<TRAutSucursalesDto>(listaSucursal);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar actualizar la información de sucursales.");
				TempData["error"] = "Hubo algun problema al intentar actualizar la información de sucursales. Si el problema persiste informe al Administrador";
			}
			return PartialView("_trListaSucursalesTR", model);
		}

		/// <summary>
		/// Método que hace analisis de parámetros (SPGECO_TR_Aut_Analiza) 
		/// </summary>
		/// <param name="depositos">lista de string concatenadas con '@' e indica depo_id</param>
		/// <param name="stkExistente">booleano</param>
		/// <param name="sustituto">booleano</param>
		/// <param name="maxPallet">intero entre 1 y 80</param>
		/// <returns></returns>
		public async Task<JsonResult> AnalizarParametros(string depositos, bool stkExistente, bool sustituto, int maxPallet)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(depositos))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar al menos un depósito." });
				}
				else if (maxPallet < 1 || maxPallet > 80)
				{
					return Json(new { error = true, warn = false, msg = $"El valor máximo de pallet x autorización no es válido. Min '1' Max '80'." });
				}
				else
				{
					//Obtenemos la lista de Pedidos Incluidos (solo pi_compte)
					TRDepositosSeleccionados = depositos;
					var listaPI = ObtenerStringListDePedidosIncluidos();
					var itemsAutAnaliza = await _productoServicio.TRAutAnaliza(listaPI, depositos, stkExistente, sustituto, maxPallet, TokenCookie);
					TRAutAnaliza = itemsAutAnaliza;
					return Json(new { error = false, warn = false, vacio = false, cantidad = itemsAutAnaliza.Count, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message, vacio = false, cantidad = 0 });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al analizar los parámetros de la transferencia solicitada, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> AbrirVistaEdicionNuevasAutYDetalleTR()
		{
			var model = new TRNuevaAutDto();
			try
			{
				if (TRAutAnaliza == null)
					return ObtenerMensajeDeError("Ocurrio un error al intentar leer los datos para analizar. Intente nevamente desde el principio, si el problema persiste informe al Administrador.");

				var orden = 0;
				var listaSucursales = from i in TRAutAnaliza
									  group i by new { i.adm_id, i.adm_nombre } into x
									  select new TRNuevaAutSucursalDto() { adm_id = x.Key.adm_id, adm_nombre = x.Key.adm_nombre, pallet_aprox = x.Sum(y => y.unidad_palet), orden = orden++ };
				TRNuevaAutSucursalLista = listaSucursales.ToList();
				model.Sucursales = ObtenerGridCore<TRNuevaAutSucursalDto>(listaSucursales.ToList());
				TRNuevaAutDetallelLista = TRAutAnaliza.Select(x => new TRNuevaAutDetalleDto()
				{
					#region Campos
					adm_id = x.adm_id,
					adm_nombre = x.adm_nombre,
					autorrizacion = x.autorizacion,
					a_transferir = x.a_transferir,
					a_transferir_box = x.a_transferir_box,
					box_id = x.box_id,
					depo_id = x.depo_id,
					depo_nombre = x.depo_nombre,
					fv = x.fv,
					nota = x.nota,
					palet = x.palet,
					pedido = x.pedido,
					pi_compte = x.pi_compte,
					p_desc = x.p_desc,
					p_id = x.p_id,
					p_id_sustituto = x.p_id_sustituto,
					p_sustituto = x.p_sustituto,
					stk = x.stk,
					stk_adm = x.stk_adm,
					unidad_palet = x.unidad_palet,
					#endregion
				}).OrderBy(y => y.p_id).ToList();
				model.Detalle = ObtenerGridCore<TRNuevaAutDetalleDto>(TRNuevaAutDetallelLista);
				return View("TRVistaNuevaAutYDetalleDeProductos", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al abrir la vista de edicion de nuevas autorizaciones y detalle de TR.");
				TempData["error"] = "Hubo algun problema al abrir la vista de edicion de nuevas autorizaciones y detalle de TR. Si el problema persiste informe al Administrador";
				return ObtenerMensajeDeError("Hubo algun problema al abrir la vista de edicion de nuevas autorizaciones y detalle de TR. Si el problema persiste informe al Administrador");
			}
		}

		public async Task<IActionResult> FiltrarListaDeProductosPorSucursal(string admId)
		{
			var model = new GridCore<TRNuevaAutDetalleDto>();
			try
			{
				var listaTemp = TRNuevaAutDetallelLista;
				var listaFiltrada = listaTemp.Where(x => x.adm_id == admId).ToList();
				model = ObtenerGridCore<TRNuevaAutDetalleDto>(listaFiltrada);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al filtrar la lista de productos por sucursal.");
				TempData["error"] = "Hubo algun problema al filtrar la lista de productos por sucursal. Si el problema persiste informe al Administrador";
				return ObtenerMensajeDeError("Hubo algun problema al filtrar la lista de productos por sucursal. Si el problema persiste informe al Administrador");
			}
			return PartialView("_trNuevaAutListaProductos", model);
		}

		public async Task<JsonResult> ExisteProductoEnTR(string pId, string admId)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(pId))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar al menos un producto." });
				}
				else if (string.IsNullOrWhiteSpace(admId))
				{
					return Json(new { error = true, warn = false, msg = "Se debe especificar una sucursal válida." });
				}
				else
				{
					if (TRNuevaAutDetallelLista.Exists(x => x.p_id == pId && x.adm_id == admId))
					{
						return Json(new { error = false, warn = true, msg = "El producto ingresado ya existe." });
					}
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al verificar si existe el producto ingresado, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> AgregarNuevoProducto(string idProdDeProdSeleccionado, string idProvDeProdSeleccionado, string pedidoDeProdSeleccionado, string boxDeProdSeleccionado, string stkDeProdSeleccionado, string cantidad, string admSeleccionado, string admSeleccionadoNombre)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(idProdDeProdSeleccionado))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar un producto." });
				}
				else if (string.IsNullOrWhiteSpace(cantidad))
				{
					return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
				}
				else
				{
					if (!decimal.TryParse(stkDeProdSeleccionado, out decimal stk))
					{
						return Json(new { error = true, warn = false, msg = "El valor de stock no es válido." });
					}
					if (!decimal.TryParse(cantidad, out decimal ctd))
					{
						return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
					}
					if (!decimal.TryParse(pedidoDeProdSeleccionado, out decimal pedido))
					{
						pedido = 0;
					}
					var productoBase = ObtenerDatosDeProducto(idProdDeProdSeleccionado);
					var nuevoProducto = new TRNuevaAutDetalleDto
					{
						stk = stk,
						p_id = idProdDeProdSeleccionado,
						p_desc = productoBase.P_desc,
						adm_id = admSeleccionado,
						adm_nombre = admSeleccionadoNombre,
						box_id = boxDeProdSeleccionado,
						pedido = pedido,
						nota = "",
						p_sustituto = false,
						a_transferir = ctd,

					};
					var listaTemp = TRNuevaAutDetallelLista;
					listaTemp.Add(nuevoProducto);
					TRNuevaAutDetallelLista = listaTemp;
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al ingresar el producto a la TR, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> AgregarProductoSustituto(string idProdDeProdSeleccionado, string idProductoSustituto, string idProvDeProdSeleccionado, string pedidoDeProdSeleccionado, string boxDeProdSeleccionado, string stkDeProdSeleccionado, string cantidad, string admSeleccionado, string admSeleccionadoNombre)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(idProdDeProdSeleccionado))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar un producto." });
				}
				else if (string.IsNullOrWhiteSpace(cantidad))
				{
					return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
				}
				else
				{
					if (!decimal.TryParse(stkDeProdSeleccionado, out decimal stk))
					{
						return Json(new { error = true, warn = false, msg = "El valor de stock no es válido." });
					}
					if (!decimal.TryParse(cantidad, out decimal ctd))
					{
						return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
					}
					if (!decimal.TryParse(pedidoDeProdSeleccionado, out decimal pedido))
					{
						pedido = 0;
					}
					var productoBase = ObtenerDatosDeProducto(idProdDeProdSeleccionado);
					var nuevoProducto = new TRNuevaAutDetalleDto
					{
						stk = stk,
						p_id = idProdDeProdSeleccionado,
						p_desc = productoBase.P_desc,
						adm_id = admSeleccionado,
						adm_nombre = admSeleccionadoNombre,
						box_id = boxDeProdSeleccionado,
						pedido = pedido,
						nota = "Sustituto",
						p_sustituto = false,
						a_transferir = ctd,
						p_id_sustituto = idProductoSustituto,
					};
					var listaTemp = TRNuevaAutDetallelLista;
					listaTemp.Add(nuevoProducto);
					TRNuevaAutDetallelLista = listaTemp;
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al ingresar el producto a la TR, intente nuevamente mas tarde." });
			}
		}

		public async Task<JsonResult> EditarCantidadEnProducto(string idProdDeProdSeleccionado, string admSeleccionado, string cantidad)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(idProdDeProdSeleccionado))
				{
					return Json(new { error = true, warn = false, msg = "Se debe indicar un producto." });
				}
				else if (string.IsNullOrWhiteSpace(cantidad))
				{
					return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
				}
				else
				{
					if (!decimal.TryParse(cantidad, out decimal ctd))
					{
						return Json(new { error = true, warn = false, msg = "Se debe especificar un valor válido para cantidad." });
					}
					var listaTemp = TRNuevaAutDetallelLista;
					var producto = listaTemp.Where(x => x.p_id == idProdDeProdSeleccionado && x.adm_id == admSeleccionado).First();
					if (producto != null)
					{ 
						producto.stk=ctd;
						TRNuevaAutDetallelLista = listaTemp;
					}
					return Json(new { error = false, warn = false, msg = "" });
				}
			}
			catch (NegocioException neg)
			{
				return Json(new { error = false, warn = true, msg = neg.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Hubo error en {this.GetType().Name} {MethodBase.GetCurrentMethod().Name}");
				return Json(new { error = true, msg = "Algo no fue bien al ingresar el producto a la TR, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> CargarListaProductoSustituto(string pId, string listaDepo, string admIdDesc, string tipo)
		{
			var model = new TRAgregarProductoDto();
			try
			{
				if (string.IsNullOrWhiteSpace(pId))
					return ObtenerMensajeDeError($"Faltan parámetros: 'p_id'. Si el problema persiste informe al Administrador.");
				if (string.IsNullOrWhiteSpace(tipo))
					return ObtenerMensajeDeError($"Faltan parámetros: 'tipo'. Si el problema persiste informe al Administrador.");

				if (!string.IsNullOrWhiteSpace(TRDepositosSeleccionados))
					listaDepo = TRDepositosSeleccionados; //Lo seteo cuando abro la ventana TRCrudAutorizacion.cshtml
				var itemsSustitutoParaAgregar = await _productoServicio.TRObtenerSustituto(pId, string.IsNullOrWhiteSpace(listaDepo) ? "N" : listaDepo, admIdDesc, tipo, TokenCookie);
				model.Productos = ObtenerGridCore<TRProductoParaAgregar>(itemsSustitutoParaAgregar);
				return PartialView("_trCargarNuevoProducto", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener lista de productos sustitutos.");
				TempData["error"] = "Hubo algun problema al obtener lista de productos sustitutos. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al obtener lista de productos sustitutos. Si el problema persiste informe al Administrador.");
			}
		}

		public async Task<IActionResult> InicializarModalAgregarProductoATR(string admId)
		{
			var model = new TRAgregarProductoDto();
			try
			{
				model.Titulo = $"Detalle de TR {admId} - {TRSucursalesLista.Where(x => x.adm_id == admId).Select(y => y.adm_nombre).First()}";
				var listaTemp = new List<TRProductoParaAgregar>();
				model.Productos = ObtenerGridCore<TRProductoParaAgregar>(listaTemp);
				model.adm_id = admId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al inicializar modal de carga de producto a TR.");
				TempData["error"] = "Hubo algun problema al inicializar modal de carga de producto a TR. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al inicializar modal de carga de producto a TR. Si el problema persiste informe al Administrador.");
			}
			return PartialView("_trCargarNuevoProducto", model);
		}

		public async Task<IActionResult> InicializarModalAgregarProductoSustitutoATR(string admId, string prodSeleccionado, string listaDepo, string tipo)
		{
			var model = new TRAgregarProductoDto();
			try
			{
				model.Titulo = $"Detalle de TR {admId} - {TRSucursalesLista.Where(x => x.adm_id == admId).Select(y => y.adm_nombre).First()}";
				if (!string.IsNullOrWhiteSpace(TRDepositosSeleccionados))
					listaDepo = TRDepositosSeleccionados; //Lo seteo cuando abro la ventana TRCrudAutorizacion.cshtml
				var itemsSustitutoParaAgregar = await _productoServicio.TRObtenerSustituto(prodSeleccionado, string.IsNullOrWhiteSpace(listaDepo) ? "N" : listaDepo, admId, tipo, TokenCookie);
				model.Productos = ObtenerGridCore<TRProductoParaAgregar>(itemsSustitutoParaAgregar);
				model.adm_id = admId;
				return PartialView("_trCargarNuevoProducto", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al inicializar modal de carga de producto a TR.");
				TempData["error"] = "Hubo algun problema al inicializar modal de carga de producto a TR. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al inicializar modal de carga de producto a TR. Si el problema persiste informe al Administrador.");
			}
		}

		public async Task<IActionResult> InicializarModalModificarCantidad(string admId, string pId)
		{
			var model = new TRAgregarProductoDto();
			try
			{
				model.Titulo = $"Detalle de TR {admId} - {TRSucursalesLista.Where(x => x.adm_id == admId).Select(y => y.adm_nombre).First()}";
				var producto = TRNuevaAutDetallelLista.Where(x => x.p_id == pId && x.adm_id == admId).First();
				var listaProdAEditar = new List<TRProductoParaAgregar>();
				listaProdAEditar.Add(new TRProductoParaAgregar()
				{
					p_id=producto.p_id,
					p_desc=producto.p_desc,
					stk_adm=producto.stk_adm,
					box_id=producto.box_id,
					stk=producto.stk,
				});
				model.Productos = ObtenerGridCore<TRProductoParaAgregar>(listaProdAEditar);
				return PartialView("_trCargarNuevoProducto", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al inicializar modal de modificación de cantidad.");
				TempData["error"] = "Hubo algun problema al inicializar modal de modificación de cantidad. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al inicializar modal de modificación de cantidad. Si el problema persiste informe al Administrador.");
			}
		}

		public async Task<IActionResult> EliminarProducto(string pId, string admId)
		{
			var model = new GridCore<TRNuevaAutDetalleDto>();
			try
			{
				if (string.IsNullOrWhiteSpace(pId))
					return ObtenerMensajeDeError($"Faltan parámetros: id de producto. Si el problema persiste informe al Administrador.");
				if (string.IsNullOrWhiteSpace(admId))
					return ObtenerMensajeDeError($"Faltan parámetros: sucursal. Si el problema persiste informe al Administrador.");

				var listaTemp = TRNuevaAutDetallelLista;
				listaTemp.RemoveAll(y => y.p_id == pId && y.adm_id == admId);
				TRNuevaAutDetallelLista = listaTemp;
				model = ObtenerGridCore<TRNuevaAutDetalleDto>(listaTemp);
				return PartialView("_trNuevaAutListaProductos", model);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al eliminar un producto por sucursal.");
				TempData["error"] = "Hubo algun problema al eliminar un producto por sucursal. Si el problema persiste informe al Administrador";
				return ObtenerMensajeDeError("Hubo algun problema al eliminar un producto por sucursal. Si el problema persiste informe al Administrador");
			}
		}

		public async Task<IActionResult> EditarNotaEnSucursal(string admId)
		{
			//TODO: Charlar con carlos para ver si modificamos lo que se muestra en esta vista
			var model = new TRNotaEnSucursalDto();
			try
			{
				if (admId == null)
					return ObtenerMensajeDeError("Debe proporcionar una sucursal válida. Si el problema persiste informe al Administrador.");

				model.Titulo = $"Nota de Sucursal {admId} - {TRSucursalesLista.Where(x => x.adm_id == admId).Select(y => y.adm_nombre).First()}";
				var listaSucursalTemp = TRNuevaAutSucursalLista;
				var sucursalTemp = listaSucursalTemp.Where(x => x.adm_id == admId).First();
				if (sucursalTemp == null)
					return ObtenerMensajeDeError("No se ha encontrado la sucursal seleccionada. Si el problema persiste informe al Administrador.");

				model.Nota = sucursalTemp.nota;
				model.adm_id = admId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener el formlario de Nota de Sucursal.");
				TempData["error"] = "Hubo algun problema al intentar obtener el formlario de Nota de Sucursal. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al intentar obtener el formlario de Nota de Sucursal. Si el problema persiste informe al Administrador.");
			}
			return PartialView("_trNotaEnSucursal", model);
		}

		public async Task<JsonResult> AgregarNotaASucursalNuevaAutTR(string nota, string admId)
		{
			try
			{
				if (string.IsNullOrEmpty(nota))
					return Json(new { error = false, warn = true, vacio = "Debe especificar un valor de nota válido.", msg = "" });
				if (string.IsNullOrEmpty(admId))
					return Json(new { error = false, warn = true, vacio = "Debe seleccionar una sucursal para anexar una nota.", msg = "" });
				var listaSucursalTemp = TRNuevaAutSucursalLista;
				var sucursalTemp = listaSucursalTemp.Where(x => x.adm_id == admId).First();
				if (sucursalTemp != null)
				{
					sucursalTemp.nota = nota;
					TRNuevaAutSucursalLista = listaSucursalTemp;
				}
				else
					return Json(new { error = false, warn = true, vacio = "No se ha encontrado la sucursal seleccionada, solicite soporte.", msg = "" });
				return Json(new { error = false, warn = false, vacio = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar agregar una nota a la sucursal.");
				TempData["error"] = "Hubo algun problema al intentar agregar una nota a la sucursal. Si el problema persiste informe al Administrador";
				return Json(new { error = true, msg = "Algo no fue bien al intentar agregar una nota a la sucursal, intente nuevamente mas tarde." });
			}
		}

		public async Task<IActionResult> EditarNotaEnProducto(string pId)
		{
			//TODO: Charlar con carlos para ver si modificamos lo que se muestra en esta vista
			var model = new TRNotaEnProductoDto();
			try
			{
				if (pId == null)
					return ObtenerMensajeDeError("Debe proporcionar un producto válido. Si el problema persiste informe al Administrador.");

				model.Titulo = $"Nota de Producto {pId} - {TRNuevaAutDetallelLista.Where(x => x.p_id == pId).Select(y => y.p_desc).First()}";
				var listaTemp = TRNuevaAutDetallelLista;
				var itemTemp = listaTemp.Where(x => x.p_id == pId).First();
				if (itemTemp == null)
					return ObtenerMensajeDeError("No se ha encontrado el producto seleccionado. Si el problema persiste informe al Administrador.");

				model.Nota = itemTemp.nota;
				model.p_id = pId;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al obtener el formulario de Nota de Producto.");
				TempData["error"] = "Hubo algun problema al intentar obtener el formulario de Nota de Producto. Si el problema persiste informe al Administrador.";
				return ObtenerMensajeDeError("Hubo algun problema al intentar obtener el formulario de Nota de Producto. Si el problema persiste informe al Administrador.");
			}
			return PartialView("_trNotaEnProducto", model);
		}

		public async Task<JsonResult> AgregarNotaAProductoNuevaAutTR(string nota, string pId)
		{
			try
			{
				if (string.IsNullOrEmpty(nota))
					return Json(new { error = false, warn = true, vacio = "Debe especificar un valor de nota válido.", msg = "" });
				if (string.IsNullOrEmpty(pId))
					return Json(new { error = false, warn = true, vacio = "Debe seleccionar un producto para anexar una nota.", msg = "" });
				var listaProductoTemp = TRNuevaAutDetallelLista;
				var itemTemp = listaProductoTemp.Where(x => x.p_id == pId).First();
				if (itemTemp != null)
				{
					itemTemp.nota = nota;
					TRNuevaAutDetallelLista = listaProductoTemp;
				}
				else
					return Json(new { error = false, warn = true, vacio = "No se ha encontrado el producto seleccionada, solicite soporte.", msg = "" });
				return Json(new { error = false, warn = false, vacio = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar agregar una nota a la sucursal.");
				TempData["error"] = "Hubo algun problema al intentar agregar una nota a la sucursal. Si el problema persiste informe al Administrador";
				return Json(new { error = true, msg = "Algo no fue bien al intentar agregar una nota a la sucursal, intente nuevamente mas tarde." });
			}
		}



		#region métodos privados
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
		private PartialViewResult ObtenerMensajeDeError(string mensaje)
		{
			RespuestaGenerica<EntidadBase> response = new()
			{
				Ok = false,
				EsError = true,
				EsWarn = false,
				Mensaje = mensaje
			};
			return PartialView("_gridMensaje", response);
		}
		private string ObtenerStringListDePedidosIncluidos()
		{
			var lista = TRAutPedidosIncluidosILista.Select(x => x.pi_compte).Distinct().ToList();
			var listaString = string.Join("@", lista);
			return listaString;
		}
		private void PreCargarListaAutPI(Dictionary<string, string> sucursales)
		{
			if (sucursales.Count == 0) return;
			var listaTemp = new List<TRAutPIDto>();
			foreach (var item in sucursales)
			{
				if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value))
				{
					var listaAuxiliar = _productoServicio.TRObtenerAutPI(AdministracionId, item.Key, TokenCookie).Result;
					listaAuxiliar.ForEach(x => { x.adm_id = item.Key; x.adm_desc = item.Value; });
					listaTemp.AddRange(listaAuxiliar);
				}
			}
			if (listaTemp.Count == 0)
			{
				return;
			}
			else
			{
				TRAutPedidosSucursalLista = listaTemp;
			}

		}

		private void VerificarSiSucursalTieneOrdenes(List<TRAutSucursalesDto> lista)
		{
			if (TRAutPedidosSucursalLista == null) return;
			if (TRAutPedidosSucursalLista.Count == 0) return;
			var TRTRAutPIListaTemporal = TRAutPedidosSucursalLista;
			List<string> listaTemp = TRTRAutPIListaTemporal.Select(x => x.adm_id).Distinct().ToList();
			if (listaTemp != null && listaTemp.Count > 0)
			{
				foreach (var item in listaTemp)
				{
					var obj = lista.Where(x => x.adm_id == item).First();
					if (obj != null)
						obj.tiene_ordenes = true;
					else
						obj.tiene_ordenes = false;
				}
				TRAutPedidosSucursalLista = TRTRAutPIListaTemporal;
			}
		}
		#endregion
	}
}
