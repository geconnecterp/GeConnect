using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models.PlanillaDeElaboracionFraccion;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace gc.sitio.Areas.Mstk.Controllers.PlanillaDeElaboracionFraccion
{
	[Area("Mstk")]
	public class PlanillaDeElaboracionFraccionController : PlanillaDeElaboracionFraccionControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IProductoServicio _prodSrv;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo;
		private string APP_MODULO = AppModulos.PLANILLA_DE_ELABORACION_Y_FRACCION.ToString();
		private readonly IDocManagerServicio _docMSv;

		public PlanillaDeElaboracionFraccionController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<PlanillaDeElaboracionFraccionController> logger,
													   IDepositoServicio depositoServicio, IProductoServicio prodSrv,
													   IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_depositoServicio = depositoServicio;
			_prodSrv = prodSrv;

			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new PlanillaDeElaboracionFraccionModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var titulo = "PLANILLA DE ELABORACION Y FRACCIÓN";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);
				#endregion

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

		public async Task<IActionResult> ObtenerBoxesDesdeDeposito(string depoId)
		{
			var model = new ListaBoxesModel();
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;
				if (string.IsNullOrEmpty(depoId))
				{
					var listaNew = new List<ComboGenDto>();
					var hlp = HelperMvc<ComboGenDto>.ListaGenerica(listaNew);
					return PartialView("_listaBoxes", new ListaBoxesModel() { ListaBoxes = hlp });
				}

				if (depoId != "0")
					model.ListaBoxes = CargarComboBoxes(depoId);
				else
				{
					List<DepositoInfoBoxDto> boxes = [];
					model.ListaBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				}
				return PartialView("_listaBoxes", model);
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
		public async Task<JsonResult> ConfirmarPlanilla(ConfirmarPlanillaDeElabYFraccRequest request)
		{
			string msg = "";
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (request == null)
					return Json(new { error = true, msg = "No se recibieron los datos para la confirmación del remito." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				PrintProperties(request);
				var respuesta = _prodSrv.ConfirmarPlanillaElaboracionYFraccion(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La acción se ejecutó correctamente.");
			}
			catch (NegocioException ex)
			{
				return Json(new { error = false, warn = true, msg = ex.Message });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} Error al intentar confirmar");
				msg = $"Hubo algún inconveniente al intentar Confirmar la Planilla de Elaboración y Fracción.";
			}

			return Json(new { error = true, msg });
		}

		#region Metodos Privados
		private void CargarDatosIniciales(PlanillaDeElaboracionFraccionModel model)
		{
			model.Nota = string.Empty;
			var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
			if (depositos != null && depositos.Count > 0)
				model.ListaDepositos = ComboDepositos(depositos);
			else
				model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.ListaBoxes = HelperMvc<ComboGenDto>.ListaGenerica([]);
			model.TablaProductosElaborados = ObtenerGridCoreSmart<ProductoModel>(new List<ProductoModel>());
			model.TablaProductosMateriaPrima = ObtenerGridCoreSmart<ProductoModel>(new List<ProductoModel>());
		}

		private SelectList ComboDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboBoxes(string depoId)
		{
			var adms = _depositoServicio.BuscarBoxPorDeposito(depoId, TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
