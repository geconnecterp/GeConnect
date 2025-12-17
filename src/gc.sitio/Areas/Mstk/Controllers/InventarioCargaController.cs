using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Tipos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class InventarioCargaController : InventarioCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IInventarioServicio _inventarioServicio;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IInventarioEstadoServicio _inventarioEstadoServicio;
		private readonly ISectorServicio _sectorServicio;
		public InventarioCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioCargaController> logger,
										 IInventarioServicio inventarioServicio, IDepositoServicio depositoServicio, IInventarioEstadoServicio inventarioEstadoServicio,
										 ISectorServicio sectorServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
			_depositoServicio = depositoServicio;
			_inventarioEstadoServicio = inventarioEstadoServicio;
			_sectorServicio = sectorServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "INVENTARIOS";
				ViewData["Titulo"] = titulo;

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

		public IActionResult InicializarPantallPrincipal()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_inventarioCargaPantallaPrincipal");
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult BuscarInventarioLista(GetInventarioListaRequest request)
		{
			var model = new InventarioCargaGrillaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				request.inve_id = "%";
				var lista = _inventarioServicio.GetInventarioLista(request, Token);
				model.GrillaInventario = ObtenerGridCoreSmart<InventarioListaDto>(lista);
				return PartialView("_gridInventarioCarga", model);
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

		public IActionResult CargarCamposDatosInventario()
		{
			var model = new InventarioCargaDatosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				var estados = _inventarioEstadoServicio.GetInventarioEstadoLista(TokenCookie);
				if (estados != null && estados.Count > 0)
					model.ListaEstado = ObtenerListaEstados(estados);
				else
					model.ListaEstado = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
				if (depositos != null && depositos.Count > 0)
					model.ListaDepositos = ObtenerListaDepositos(depositos);
				else
					model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
				var conteos = ObtenerListaConteos();
				model.ListaConteos = conteos;
				model.AperturaDesde = DateTime.Now.AddDays(-1);
				model.AperturaHasta = DateTime.Now;
				model.Descripcion = string.Empty;

				return PartialView("_inventarioDatos", model);
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

		public IActionResult CargarDatosAdicionalesInicial()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_grillasAdicionales");
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		public IActionResult CargarGrillaRubrosEnSeccionDatosAdicionales(string inv_nro)
		{
			var model = new InventarioCargaGrillaRubrosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				
				var lista = _inventarioServicio.GetRubrosEnInventario(inv_nro, TokenCookie, AdministracionId);
				model.GrillaRubros = ObtenerGridCoreSmart<RubroEnInventarioDto>(lista);
				return PartialView("_grillasAdicionalesRubros", model);
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

		public IActionResult CargarListaSectoresEnSeccionDatosAdicionales()
		{
			var model = new ListaSectorModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _sectorServicio.GetSectoresLista(TokenCookie);
				model.ListaSectores = ObtenerListaSectores(lista);
				return PartialView("_listaSectores", model);
			}
			catch (Exception ex)
			{
				RespuestaGenerica<EntidadBase> response = new()
				{
					Ok = false,
					EsError = true,
					Mensaje = ex.Message
				};
				return PartialView("_gridMensaje", response);
			}
		}

		#region Metodos privados
		private SelectList ObtenerListaConteos()
		{ 
			var lista = new List<ComboGenDto>
			{
				new ComboGenDto { Id = "1", Descripcion = "Conteo Simple" },
				new ComboGenDto { Id = "2", Descripcion = "Conteo Doble" },
				new ComboGenDto { Id = "3", Descripcion = "Conteo por Box" }
			};
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaEstados(List<InventarioEstadoDto> estados)
		{
			var lista = estados.Select(x => new ComboGenDto { Id = x.inve_id, Descripcion = x.inve_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaSectores(List<SectorDto> sectores)
		{
			var lista = sectores.Select(x => new ComboGenDto { Id = x.Sec_Id, Descripcion = x.Sec_Desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
