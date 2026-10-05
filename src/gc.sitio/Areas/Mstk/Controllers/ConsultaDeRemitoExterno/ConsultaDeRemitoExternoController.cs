using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.RemitoExterno;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaDeRemitoExterno
{
	[Area("Mstk")]
	public class ConsultaDeRemitoExternoController : ConsultaDeRemitoExternoControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IRemitoServicio _remitosrv;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.REMITO_NO_FISCAL.ToString();
		private readonly IDocManagerServicio _docMSv;

		public ConsultaDeRemitoExternoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeRemitoExternoController> logger,
												 IDocManagerServicio docManager, IOptions<DocsManager> docsManager, IRemitoServicio remitosrv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_remitosrv = remitosrv;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: CC_NR_NP
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeRemitoExternoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA DE REMITOS EXTERNOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);
				#endregion

				CargarDatosIniciales(model);

				return View(model);

			}
			catch (Exception)
			{

				throw;
			}
		}

		[HttpPost]
		public IActionResult InicializarPantallPrincipal(DateTime f_desde, DateTime f_hasta)
		{
			var model = new PrincipalConsultaDeRemitoExternoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Desde = f_desde;
				model.Hasta = f_hasta;
				return PartialView("_pantallaPrincipal", model);
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
		public async Task<IActionResult> CargarRemitosExternos(DateTime f_desde, DateTime f_hasta, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new RemitosExternosModel();
			var lista = new List<RemitoExternoListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<RemitoExternoListaDto> grillaDatos;

			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (!buscaNew)
				{
					lista = ListaRemitosExternos.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaRemitosExternos = lista;
				}
				else
				{
					var request = new RemitoExternoListaRequest
					{
						FechaDesde = f_desde,
						FechaHasta = f_hasta,
						Sort = sort,
						SortDir = sortDir,
						Registros = _setting.NroRegistrosPagina
					};
					//request.Pagina = pag;

					var res = await _remitosrv.ObtenerRemitosExternosLista(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaRemitosExternos = lista;

				}
				metadata = MetadataListaRemitosExternos;
				grillaDatos = GenerarGrillaSmart(ListaRemitosExternos, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaRemitos = grillaDatos;
				return PartialView("_grillaRemitos", model);

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
		public async Task<IActionResult> ObtenerDetalleRemitoExterno(string rem_compte)
		{
			var model = new RemitoExternoDetalleModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				//List<DevolucionRevertidoDto>
				var lista = await _remitosrv.ObtenerRemitoExternoDetalle(rem_compte, TokenCookie);
				model.GrillaRemitoDetalle = ObtenerGridCoreSmart<RemitoExternoDetalleDto>(lista);
				model.Leyenda = rem_compte;

				return PartialView("_grillaDetalle", model);
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

		#region Métodos Privados
		private void CargarDatosIniciales(ConsultaDeRemitoExternoModel model)
		{
			model.FechaHasta = DateTime.Now;
			model.FechaDesde = DateTime.Now.AddDays(-7);
		}
		#endregion
	}
}
