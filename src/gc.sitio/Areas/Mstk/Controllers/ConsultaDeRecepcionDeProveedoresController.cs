using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDeRecepcionDeProveedoresController : ConsultaDeRecepcionDeProveedoresControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //Consulta de Recepcion de Proveedores
		private AppModulo _modulo_2; //Consulta de Recepcion de Proveedores Detalle
		private string APP_MODULO_1 = AppModulos.REPORTE_RECEPCION_PROVEEDORES.ToString();
		private string APP_MODULO_2 = AppModulos.REPORTE_RECEPCION_PROVEEDORES_DETALLE.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ICuentaServicio _cuentaServicio;
		public ConsultaDeRecepcionDeProveedoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeRecepcionDeProveedoresController> logger,
														  IAdministracionServicio administracionServicio, ICuentaServicio cuentaServicio,
														  IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_cuentaServicio = cuentaServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltrosConsDeReDePrModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE RECEPCIONES DE PROVEEDOR";
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

		public IActionResult AbrirPantallaPrincipal()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				return PartialView("_pantallaPrincipal");
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
					case TipoDeReporte.Recepcion_De_Proveedores:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "RECEPCIÓN DE PROVEEDORES";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.Recepcion_De_Proveedores_Detalle:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "RECEPCIÓN DE PROVEEDORES DETALLE";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
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

		#region Metodos Privados
		private enum TipoDeReporte
		{
			Recepcion_De_Proveedores = 1,
			Recepcion_De_Proveedores_Detalle = 2,
		}

		private void CargarDatosIniciales(FiltrosConsDeReDePrModel model)
		{
			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");

			model.Desde = ObtenerPrimerDiaDelMesActual();
			model.Hasta = DateTime.Now.Date;
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", Token);
			if (sucursales != null && sucursales.Count > 0)
			{
				model.ListaSucursales = ObtenerSucursales(sucursales);
				var suc = sucursales.Where(x => x.Adm_id == AdministracionId).FirstOrDefault();
			}
			else
			{
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			}
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
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

		public static DateTime ObtenerPrimerDiaDelMesActual()
		{
			return new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
		}
		#endregion
	}
}
