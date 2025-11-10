using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class LiqDeEmpConsultaYAnulacionController : LiqDeEmpConsultaYAnulacionControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ImportacionLiquidacionDeEmpleado _importacionLiquidacionDeEmpleado;
		private readonly IFinancieroServicio _financieroServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.DDH.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		public LiqDeEmpConsultaYAnulacionController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<LiqDeEmpConsultaYAnulacionController> logger,
													IFinancieroServicio financieroServicio, IOptions<ImportacionLiquidacionDeEmpleado> options2,
													IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_importacionLiquidacionDeEmpleado = options2.Value;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: ADE
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroLiqDeEmpConsultaYAnulacionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA Y ANULACIÓN DE LIQUIDACIONES DE EMPLEADOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				model.Date1 = DateTime.Now.AddYears(-1);
				model.Date2 = DateTime.Now;
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

		[HttpPost]
		public async Task<IActionResult> BuscarLiquidacionesDeEmpleados(ConsultaLiqDeEmpleadoRequest request, bool buscaNew, string sort = "le_compte", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new LiquidacionDeEmpleadoModel();
			var lista = new List<LiqDeEmpleadoListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<LiqDeEmpleadoListaDto> grillaDatos;
			try
			{
				if (!buscaNew)
				{
					lista = ListaLiqDeEmp.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaLiqDeEmp = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					request.Pagina = pag;

					var res = await _financieroServicio.BuscarLiquidacionesDeEmpleados(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaLiqDeEmp = lista;

				}
				metadata = MetadataLiqDeEmp;
				grillaDatos = GenerarGrillaSmart(ListaLiqDeEmp, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaLiqDeEmp = grillaDatos;
				return PartialView("_gridLiqDeEmp", model);
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
	}
}
