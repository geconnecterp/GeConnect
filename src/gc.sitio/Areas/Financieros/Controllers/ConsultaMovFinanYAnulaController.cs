using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ConsultaMovFinanYAnulaController : ConsultaMovFinanYAnulaControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la impresión del comprobante
		private AppModulo _modulo_2; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.TEC.ToString();
		private string APP_MODULO_2 = AppModulos.CMF.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoTransferenciaServicio _tipoTransferenciaServicio;
		public ConsultaMovFinanYAnulaController(IFinancieroServicio financieroServicio, ITipoTransferenciaServicio tipoTransferenciaServicio,
												IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
												IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ConsultaMovFinanYAnulaController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_tipoTransferenciaServicio = tipoTransferenciaServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: TEC
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2); //identifico los datos del modulo que necesito: COP 
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ConsultaMovFinanYAnulaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA MOVIMIENTOS FINANCIEROS";
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

		[HttpPost]
		public IActionResult ActualizarListaDeUsuarios(DateTime desde, DateTime hasta)
		{
			var model = new ListaUsuariosModel();
			try
			{
				var usuLista = _financieroServicio.GetFinancieroTraUsu(desde, hasta, TokenCookie);
				var listaUsu = usuLista.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
				model.ListaUsu = HelperMvc<ComboGenDto>.ListaGenerica(listaUsu);

				var usuList = new List<ComboGenDto>();
				ViewBag.UsuList = HelperMvc<ComboGenDto>.ListaGenerica(usuList);

				return PartialView("_listaUsuarios", model);
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
		public async Task<IActionResult> BuscarMovimientosFinancieros(ConsultaMovFinancierosRequest request, bool buscaNew, string sort = "cta_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new MovimientoFinancieroModel();
			var lista = new List<MovimientoFinancieroListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<MovimientoFinancieroListaDto> grillaDatos;
			try
			{
				if (!buscaNew)
				{
					lista = ListaMovimientoFinanciero.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaMovimientoFinanciero = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					request.Pagina = pag;

					var res = await _financieroServicio.BuscarMovimientoFinanciero(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaMovimientoFinanciero = lista;

					var resTotal = _financieroServicio.BuscarMovimientoFinancieroReporte(request, TokenCookie);
					model.Totales = resTotal?.Sum(x => x.tra_importe) ?? 0.00M;
				}
				metadata = MetadataMovimientoFinanciero;
				grillaDatos = GenerarGrillaSmart(ListaMovimientoFinanciero, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaMovimientoFinanciero = grillaDatos;
				return PartialView("_movimientoFinanciero", model);
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
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaMovimientoFinanciero = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - ORDENDECOMPRA" });
			}
		}

		public JsonResult ActualizarTotales()
		{
			try
			{
				var tot = (decimal)0.00;
				if (ListaMovimientoFinanciero != null && ListaMovimientoFinanciero.Count > 0)
					tot += ListaMovimientoFinanciero.Sum(x => x.tra_importe);
				return Json(new { error = false, warn = false, msg = string.Empty, totales = tot });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar calcular los totales. {ex}" });
			}
		}

		/// <summary>
		/// Establece el tipo de reporte seleccionado por el usuario para la consulta de órdenes de pago.
		/// Inicializa el gestor de impresión y carga los documentos disponibles según el tipo de reporte.
		/// </summary>
		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });


				//Seteo el tipo de reporte en la sesion
				if (tipoReporte == 1)
				{
					string titulo = "IMPRIME ACUSE DE MOVIMIENTO";
					#region Gestor Impresion - Inicializacion de variables
					//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
					DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
					// en este mismo acto se cargan los posibles documentos
					//que se pueden imprimir, exportar, enviar por email o whatsapp
					ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

					#endregion
				}
				else
				{
					string titulo = "CONSULTA DE MOVIMIENTO FINANCIERO";
					#region Gestor Impresion - Inicializacion de variables
					//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
					DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
					// en este mismo acto se cargan los posibles documentos
					//que se pueden imprimir, exportar, enviar por email o whatsapp
					ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);

					#endregion
				}
				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		public JsonResult AnularMovimientoFinanciero(string tra_compte)
		{
			try
			{
				if (string.IsNullOrEmpty(tra_compte))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un movimiento financiero para anular." });
				var request = new MovimientoFinancieroAnularRequest()
				{
					tra_compte = tra_compte,
					adm_id = AdministracionId,
					usu_id = UserName
				};
				var respuesta = _financieroServicio.MovimientoFinancieroAnular(request, TokenCookie);
				if (respuesta == null || respuesta.Entidad == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar anular el movimiento financiero seleccionado ({tra_compte})." });
				if (respuesta.Entidad.resultado > 0)
					return Json(new { error = true, warn = false, msg = $"{respuesta.Entidad.resultado_msj} ({tra_compte})" });
				return Json(new { error = false, warn = false, msg = $"El movimiento financiero ({tra_compte}) se ha anulado con éxito." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar anular el movimiento financiero: {ex.Message}" });
			}
		}

		public IActionResult CargarDetalleDeMovimientoFinanciero(string tra_compte)
		{
			var model = new DetalleMovFinanModel();
			try
			{
				if (string.IsNullOrEmpty(tra_compte))
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Debe seleccionar un movimiento financiero para ver su detalle."
					};
					return PartialView("_gridMensaje", response);
				}
				var detalle = _financieroServicio.GetFinancieroTraRepoDDto(tra_compte, TokenCookie);
				var detalleCtag = _financieroServicio.GetFinancieroTraRepoCtag(tra_compte, TokenCookie);
				if (detalle == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"No se encontró el detalle del movimiento financiero seleccionado ({tra_compte})."
					};
					return PartialView("_gridMensaje", response);
				}

				var gridOrigen = detalle.Where(x => x.grupo.Equals(1)).ToList();
				var gridDestino = detalle.Where(x => x.grupo.Equals(2)).ToList();

				model.GrillaOrigen = ObtenerGridCoreSmart<FinancieroTraRepoDDto>(gridOrigen);
				model.TotalOrigen = gridOrigen.Sum(x => x.fc_importe);
				model.MostrarSeccionGrillaOrigen = gridOrigen != null && gridOrigen.Count > 0;
				model.GrillaDestino = ObtenerGridCoreSmart<FinancieroTraRepoDDto>(gridDestino);
				model.TotalDestino = gridDestino.Sum(x => x.fc_importe);
				model.MostrarSeccionGrillaDestino = gridDestino != null && gridDestino.Count > 0;
				model.GrillaCtag = ObtenerGridCoreSmart<FinancieroTraRepoCtagDto>(detalleCtag);
				model.TotalCtag = detalleCtag.Sum(x => x.cm_importe);
				model.MostrarSeccionGrillaCtag = detalleCtag != null && detalleCtag.Count > 0;

				return PartialView("_detalleMovimientoFinanciero", model);
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

		#region Métodos privados
		private void CargarDatosIniciales(ConsultaMovFinanYAnulaModel model)
		{
			model.Date1 = DateTime.Today.AddMonths(-1);
			model.Date2 = DateTime.Today;
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.ListaCFO = ComboCTF(ctfLista);
			model.ListaCFD = ComboCTF(ctfLista);
			var tipoTransfeLista = _tipoTransferenciaServicio.GetTipoTransferenciaLista(TokenCookie);
			var listaTT = tipoTransfeLista.Select(x => new ComboGenDto { Id = x.ttra_id, Descripcion = x.ttra_lista });
			model.ListaTT = HelperMvc<ComboGenDto>.ListaGenerica(listaTT);
			var usuLista = _financieroServicio.GetFinancieroTraUsu(model.Date1, model.Date2, TokenCookie);
			var listaUsu = usuLista.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
			model.ListaUsu = HelperMvc<ComboGenDto>.ListaGenerica(listaUsu);

			var cFOList = new List<ComboGenDto>();
			ViewBag.CFOList = HelperMvc<ComboGenDto>.ListaGenerica(cFOList);
			var cFDList = new List<ComboGenDto>();
			ViewBag.CFDList = HelperMvc<ComboGenDto>.ListaGenerica(cFDList);
			var tTList = new List<ComboGenDto>();
			ViewBag.TTList = HelperMvc<ComboGenDto>.ListaGenerica(tTList);
			var usuList = new List<ComboGenDto>();
			ViewBag.UsuList = HelperMvc<ComboGenDto>.ListaGenerica(usuList);
		}

		protected SelectList ComboCTF(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
