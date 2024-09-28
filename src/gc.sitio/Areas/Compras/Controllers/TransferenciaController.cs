﻿using Azure;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Security.Policy;


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
				var itemsAutSucursales = await _productoServicio.TRObtenerAutSucursales(AdministracionId, TokenCookie);
				model.ListaAutSucursales = ObtenerGridCore<TRAutSucursalesDto>(itemsAutSucursales);
				if (model.ListaAutSucursales != null && model.ListaAutSucursales.ListaDatos != null && model.ListaAutSucursales.ListaDatos.Count > 0)
				{

					itemsAutSucursales.ForEach(x => sucursales.Add(x.adm_id, x.adm_nombre));
					PreCargarListaAutPI(sucursales);
					VerificarSiSucursalTieneOrdenes(itemsAutSucursales);
				}
				TRSucursalesLista = itemsAutSucursales;
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

		public async Task<IActionResult> VerDetallePedidoDeSucursal()
		{
			//TODO: Obtener los datos de pedido a visualizar, charlarlo con Carlos
			return Json(new { error = false, warn = false, vacio = false, msg = "" });
			//return PartialView("<_aca_deberia_ir_un_modal>");
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

		public async Task<JsonResult> AgregarNotaASucursalNuevaAutTR(string nota)
		{
			try
			{
				return Json(new { error = false, warn = false, vacio = false, msg = "" });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar agregar una nota a la sucursal.");
				TempData["error"] = "Hubo algun problema al intentar agregar una nota a la sucursal. Si el problema persiste informe al Administrador";
				return Json(new { error = true, msg = "Algo no fue bien al intentar agregar una nota a la sucursal, intente nuevamente mas tarde." });
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
				}).ToList();
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

		#region métodos privados
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
			}
		}
		#endregion
	}
}
