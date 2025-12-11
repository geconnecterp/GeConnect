using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.ControlComun.Models;
using gc.sitio.Areas.ControlComun.Models.InfoAdicionalDeProd.Model;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Diagnostics.Metrics;
using NDeCYPI = gc.infraestructura.Dtos.Almacen.Tr.NDeCYPI;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class InfoAdicionalDeProdController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IProductoServicio _productoServicio;
		public InfoAdicionalDeProdController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InfoAdicionalDeProdController> logger,
											 IAdministracionServicio administracionServicio, IProductoServicio productoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AbrirComponente(AbrirComponenteRequest request)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var model = new AbrirComponenteModel();
				model.ComboSucursales = ComboSucursales();
				return View("~/areas/ControlComun/views/InfoAdicionalDeProd/_index.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		#region Buscar InfoProd

		public async Task<IActionResult> BuscarInfoProd(string pId)
		{
			var model = new GridCoreSmart<NDeCYPI.InfoProductoDto>();
			try
			{
				var info = await _productoServicio.InfoProd(pId, TokenCookie);
				model = ObtenerGridCoreSmart<NDeCYPI.InfoProductoDto>(info);
				return PartialView("_infoProd", model);
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

		public async Task<IActionResult> BuscarInfoProdStkA(string pId, string admId)
		{
			var model = new GridCoreSmart<InfoProdStkA>();
			try
			{
				var info = await _productoServicio.InfoProductoStkA(pId, AdministracionId, TokenCookie);
				model = ObtenerGridCoreSmart<InfoProdStkA>(info);
				return PartialView("_infoProdStockA", model);
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

		public async Task<IActionResult> BuscarInfoProdStkD(string pId, string admId)
		{
			var model = new BuscarInfoProdStkDModel();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProductoStkD(pId, admId, TokenCookie);
				model.GrillaInfoProdStkD = ObtenerGridCoreSmart<InfoProdStkD>(info);
				model.ComboSucursales = ComboSucursales();
				model.selectedValue = admId;
				return PartialView("_infoProdStockD", model);
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

		public async Task<IActionResult> BuscarInfoProdStkBox(string pId, string admId)
		{
			var model = new BuscarInfoProdStkBoxModel();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				var info = await _productoServicio.InfoProductoStkBoxes(pId, admId, "%", TokenCookie);
				model.GrillaInfoProdStkBox = ObtenerGridCoreSmart<InfoProdStkBox>(info);
				model.ComboSucursales = ComboSucursales();
				model.selectedValue = admId;
				return PartialView("_infoProdStockBox", model);
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

		public async Task<IActionResult> BuscarInfoProdMovMens(string pId, string admId, int meses)
		{
			var model = new BuscarInfoProdMovMensModel();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				if (meses == 0)
					model.Meses = 12;
				else
					model.Meses = meses;
				var info = await _productoServicio.InfoProdIExMes(admId, pId, model.Meses, TokenCookie);
				model.GrillaInfoProdMovMens = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExMesDto>(info);
				model.ComboSucursales = ComboSucursales();
				model.selectedValue = admId;
				
				return PartialView("_infoProdMovMens", model);
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

		public async Task<IActionResult> BuscarInfoProdMovSem(string pId, string admId, int semanas)
		{
			var model = new BuscarInfoProdMovSemModel();
			try
			{
				if (string.IsNullOrWhiteSpace(admId))
					admId = AdministracionId;
				if (semanas == 0)
					model.Semanas = 4;
				else
					model.Semanas = semanas;

				var info = await _productoServicio.InfoProdIExSemana(admId, pId, model.Semanas, TokenCookie);
				model.GrillaInfoProdMovSem = ObtenerGridCoreSmart<NDeCYPI.InfoProdIExSemanaDto>(info);
				model.ComboSucursales = ComboSucursales();
				model.selectedValue = admId;
				return PartialView("_infoProdMovSem", model);
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



		#endregion

		#region Métodos privados
		private SelectList ComboSucursales()
		{
			var adms = _administracionServicio.GetAdministracionLogin();
			var lista = adms.Select(x => new ComboGenDto { Id = x.Id, Descripcion = x.Descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
