using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Inventario;
using gc.infraestructura.Dtos.Inventario.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Tipos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class InventarioReportesController : InventarioReportesControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //INV_REPO_STK_VS_CONTEO
		private AppModulo _modulo_2; //INV_REPO_VAL_X_SEC
		private AppModulo _modulo_3; //INV_REPO_VAL_X_RUB
		private AppModulo _modulo_4; //INV_REPO_VAL_DETALLE
		private AppModulo _modulo_5; //INV_REPO_CONTEO_X_USU
		private string APP_MODULO_1 = AppModulos.INV_REPO_STK_VS_CONTEO.ToString();
		private string APP_MODULO_2 = AppModulos.INV_REPO_VAL_X_SEC.ToString();
		private string APP_MODULO_3 = AppModulos.INV_REPO_VAL_X_RUB.ToString();
		private string APP_MODULO_4 = AppModulos.INV_REPO_VAL_DETALLE.ToString();
		private string APP_MODULO_5 = AppModulos.INV_REPO_CONTEO_X_USU.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		private readonly AppSettings _setting;
		private readonly IInventarioServicio _inventarioServicio;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly IInventarioEstadoServicio _inventarioEstadoServicio;
		public InventarioReportesController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InventarioReportesController> logger,
											IInventarioServicio inventarioServicio, IAdministracionServicio administracionServicio,
											IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
											IInventarioEstadoServicio inventarioEstadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_inventarioServicio = inventarioServicio;
			_administracionServicio = administracionServicio;
			_inventarioEstadoServicio = inventarioEstadoServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_modulo_3 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_3);
			_modulo_4 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_4);
			_modulo_5 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_5);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new InventarioReporteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTES DE INVENTARIOS";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(model);

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

		public IActionResult InicializarPantallPrincipal()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				return PartialView("_inventarioReportePantallaPrincipal");
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
				request.usu_id = "%";
				var lista = _inventarioServicio.GetInventarioLista(request, TokenCookie);
				model.GrillaInventario = ObtenerGridCoreSmart<InventarioListaDto>(lista);
				ListaInventarioEnReporte = lista;
				return PartialView("_gridInventario", model);
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

		public IActionResult InicializarTabRepoValorPorSec(ReporteInventarioRequest request)
		{ 
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				request.inv_nro = request.inv_nro == null ? "%" : request.inv_nro;
				var lista = _inventarioServicio.GetReporteValorizacionPorSector(request, TokenCookie);
				var model = new RepoValPorSecModel
				{
					GrillaRepoValPorSec = ObtenerGridCoreSmart<InvRepoValPorSecDto>(lista)
				};
				return PartialView("_gridRepoValPorSec", model);
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

		public IActionResult InicializarTabRepoValorPorRub(ReporteInventarioRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				request.inv_nro = request.inv_nro == null ? "%" : request.inv_nro;
				var lista = _inventarioServicio.GetReporteValorizacionPorRubro(request, TokenCookie);
				var model = new RepoValPorRubModel
				{
					GrillaRepoValPorRub = ObtenerGridCoreSmart<InvRepoValPorRubDto>(lista)
				};
				return PartialView("_gridRepoValPorRub", model);
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

		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.RepoStkVsConteo:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Registro de Stock vs Conteo";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.RepoValPorSec:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Valorizado por Sectores";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					case TipoDeReporte.RepoValPorRub:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Valorizado por Rubros";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_3);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_3);
						#endregion
						break;
					case TipoDeReporte.RepoValorDetalle:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Valorizado Detalle";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					case TipoDeReporte.RepoConteoPorUsu:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "Planilla por Usuarios";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_5);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_5);
						#endregion
						break;
					default:
						break;
				}

				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		#region Métodos Privados
		private void CargarDatosIniciales(InventarioReporteModel model)
		{
			model.Desde = DateTime.Now.Date.AddMonths(-1);
			model.Hasta = DateTime.Now.Date;
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", Token);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursales = ObtenerSucursales(sucursales);
				var suc = sucursales.Where(x => x.Adm_id == AdministracionId).FirstOrDefault();
				if (suc != null && suc.Adm_central == 'S')
					model.HabilitarCambioDeSucursalSeleccionada = false;
				else
					model.HabilitarCambioDeSucursalSeleccionada = true;
			}
			else
			{
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.HabilitarCambioDeSucursalSeleccionada = false;
			}
			model.SucursalSeleccionada = AdministracionId;
			var estados = _inventarioEstadoServicio.GetInventarioEstadoLista(TokenCookie);
			if (estados != null && estados.Count > 0)
				model.ListaEstados = ObtenerEstadosDeInventario(estados);
			else
				model.ListaEstados = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}

		private SelectList ObtenerSucursales(List<AdministracionDto> administraciones)
		{
			var lista = administraciones.Select(a => new ComboGenDto
			{
				Id = a.Adm_id,
				Descripcion = a.Adm_nombre
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private SelectList ObtenerEstadosDeInventario(List<InventarioEstadoDto> estados)
		{
			var lista = estados.Select(e => new ComboGenDto
			{
				Id = e.inve_id,
				Descripcion = e.inve_desc
			}).ToList();
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		enum TipoDeReporte
		{
			RepoStkVsConteo = 1,
			RepoValPorSec = 2,
			RepoValPorRub = 3,
			RepoValorDetalle = 4,
			RepoConteoPorUsu = 5
		}
		#endregion
	}
}
