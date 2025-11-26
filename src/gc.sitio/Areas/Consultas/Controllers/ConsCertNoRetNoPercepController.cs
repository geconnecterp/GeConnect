using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Consultas.ConsCertNoRetNoPercep;
using gc.infraestructura.Dtos.Consultas.ConsVencTipoCtaTipoCompte;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ConsCertNoRetNoPercepController : ConsCertNoRetNoPercepControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoImpuestoServicio _tipoImpuestoServicio;
		private readonly IConsultasServicio _consultaServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.CC_NR_NP.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		public ConsCertNoRetNoPercepController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsCertNoRetNoPercepController> logger,
											   ITipoImpuestoServicio tipoImpuestoServicio, IConsultasServicio consultaServicio,
											   IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoImpuestoServicio = tipoImpuestoServicio;
			_consultaServicio = consultaServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: CC_NR_NP
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsCertNoRetNoPercepModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CERTIFICADOS DE NO RETENCIÓN NO PERCEPCIÓN";
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

		public async Task<IActionResult> BuscarCertificados(ConsultarCertificadosRequest request, bool buscaNew, string sort = "grupo", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new CertificadoListaModel();
			var lista = new List<CertificadoListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<CertificadoListaDto> grillaDatos;

			try
			{
				if (!buscaNew)
				{
					lista = ListaCertificados.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaCertificados = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					request.Pagina = pag;

					var res = await _consultaServicio.ConsultarCertificados(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaCertificados = lista;

				}
				metadata = MetadataCertificados;
				grillaDatos = GenerarGrillaSmart(ListaCertificados, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaCertificados = grillaDatos;
				return PartialView("_gridCertificados", model);
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

		#region CargarDatosIniciales
		private void CargarDatosIniciales(ConsCertNoRetNoPercepModel model)
		{
			if (TipoImpuestoLista.Count == 0)
			{
				ObtenerTiposDeImpuestos(_tipoImpuestoServicio);
			}
			model.ListaTipoImpuesto = ComboTipoImpuestos();
			model.NoVencidos = false;
			model.Vencidos = false;
			model.CertNoPercepcion = false;
			model.CertNoRetencion = false;

			var tImpuestosList = new List<ComboGenDto>();
			ViewBag.TipoImpuestosList = HelperMvc<ComboGenDto>.ListaGenerica(tImpuestosList);
		}
		#endregion
	}
}
