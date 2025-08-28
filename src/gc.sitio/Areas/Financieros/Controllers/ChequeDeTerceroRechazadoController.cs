using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ChequeDeTerceroRechazadoController : ChequeDeTerceroRechazadoControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.TEC.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly string tipoCTAF = "BA";
		public ChequeDeTerceroRechazadoController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ChequeDeTerceroRechazadoController> logger,
												  IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
												  IFinancieroServicio financieroServicio) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: TEC
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new ChequeRechazadoPasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CHEQUES DEPOSITADOS RECHAZADOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				var listaCuentasBancos = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCTAF, AdministracionId, TokenCookie);
				ListaCuentaBancos = listaCuentasBancos;
				model.ListaCuentasBancarias = ComboCuentaBancos(ListaCuentaBancos);

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

		public IActionResult BuscarChequesDepositados(string ctaf_id, DateTime fechaDesde, DateTime fechaHasta)
		{ 
			var model = new ChequesDepositadosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaDeCheques = _financieroServicio.GetFinancieroChequeDepositado(ctaf_id, fechaDesde, fechaHasta, TokenCookie);
				model.GrillaChequesDepositados = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(listaDeCheques);
				ListaCheques = listaDeCheques;
				model.FechaRechazado = DateTime.Today;

				return PartialView("_chequesDepositados", model);
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

		public IActionResult VolverPasoUno()
		{
			var model = new ChequeRechazadoPasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaCuentasBancos = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCTAF, AdministracionId, TokenCookie);
				ListaCuentaBancos = listaCuentasBancos;
				model.ListaCuentasBancarias = ComboCuentaBancos(ListaCuentaBancos);

				return PartialView("_pasoUno", model);
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

		public JsonResult ConfirmarRechazoDeValor(string tra_compte_selected, string fc_dia_movi_selected, string fc_compte_selected, string fc_item_selected, DateTime fechaRechazo)
		{
			try
			{
				if (string.IsNullOrEmpty(tra_compte_selected) || string.IsNullOrEmpty(fc_dia_movi_selected) || string.IsNullOrEmpty(fc_compte_selected) || string.IsNullOrEmpty(fc_item_selected))
					return Json(new { error = true, warn = false, msg = $"Faltan especificar algunos datos. tra_compte: {tra_compte_selected} - fc_dia_movi: {fc_dia_movi_selected} - fc_compte: {fc_compte_selected} - fc_item: {fc_item_selected}" });

				var itemSeleccionado = ListaCheques.Where(x => x.tra_compte == tra_compte_selected && x.fc_dia_movi == fc_dia_movi_selected && x.fc_compte == fc_compte_selected && x.fc_item == Convert.ToInt32(fc_item_selected)).FirstOrDefault();
				if (itemSeleccionado == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener el ítem. tra_compte: {tra_compte_selected} - fc_dia_movi: {fc_dia_movi_selected} - fc_compte: {fc_compte_selected} - fc_item: {fc_item_selected}" });

				var request = new ConfirmarTransferenciaRequest
				{
					ttra_id = "VR",
					usu_id = UserName,
					adm_id = AdministracionId,
					tra_concepto = "",
					tra_fecha = fechaRechazo
				};

				Console.WriteLine($"ttra_id: {request.ttra_id}");
				Console.WriteLine($"usu_id: {request.usu_id}");
				Console.WriteLine($"adm_id: {request.adm_id}");
				Console.WriteLine($"tra_concepto: {request.tra_concepto}");
				Console.WriteLine($"tra_fecha: {request.tra_fecha}");

				var newValor = new ValoresDesdeObligYCredDto()
				{
					ctaf_id = itemSeleccionado.ctaf_id,
					ctaf_denominacion = itemSeleccionado.ctaf_denominacion,
					tcf_id = "BA",
					tipo = " ",
					automatico = 'N',
					op_dato1_valor = itemSeleccionado.fc_dato1_valor,
					op_dato1_desc = " ",
					op_dato2_valor = itemSeleccionado.fc_dato2_valor,
					op_dato2_desc = " ",
					op_dato3_valor = itemSeleccionado.fc_dato3_valor,
					op_dato3_desc = " ",
					op_importe = itemSeleccionado.fc_importe,
					op_fecha_valor = itemSeleccionado.fc_fecha_valor,
					fc_compte = itemSeleccionado.fc_compte,
					fc_item = itemSeleccionado.fc_item,
					fc_dia_movi = itemSeleccionado.fc_dia_movi,
					fc_cta_id = itemSeleccionado.fc_cta_id,
					fc_anombre = " ",
					concepto_valor = itemSeleccionado.tra_compte,
					resultado = 0,
					resultado_msj = " ",
				};
				var listaAux = new List<ValoresDesdeObligYCredDto>
				{
					newValor
				};
				request.json_o = JsonConvert.SerializeObject(listaAux, new JsonSerializerSettings());
				Console.WriteLine($"json_o: {request.json_o}");
				listaAux = [];
				request.json_d = JsonConvert.SerializeObject(listaAux, new JsonSerializerSettings());
				Console.WriteLine($"json_d: {request.json_d}");
				var encabezado = new Encabezado();
				var ListaConceptoFacturado = new List<ConceptoFacturadoDto>();
				var ListaOtrosTributos = new List<OtroTributoDto>();

				request.json_concepto = JsonConvert.SerializeObject(ListaConceptoFacturado, new JsonSerializerSettings());
				Console.WriteLine($"json_concepto: {request.json_concepto}");
				request.json_encabezado = JsonConvert.SerializeObject(ListaConceptoFacturado, new JsonSerializerSettings());
				Console.WriteLine($"json_encabezado: {request.json_encabezado}");
				request.json_otro = JsonConvert.SerializeObject(ListaOtrosTributos, new JsonSerializerSettings());
				Console.WriteLine($"json_otro: {request.json_otro}");
				var respuesta = _financieroServicio.FinancieroConfirmarTransferencia(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "La confirmación del rechazo del valor se ha realizado con éxito.");
				//return Json(new { error = false, warn = false, msg = "[MOCK] La confirmación del rechazo del valor se ha realizado con éxito." });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaCheques = [];
				ListaCuentaBancos = [];
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public IActionResult Paso1()
		{
			var model = new ChequeRechazadoPasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaCuentasBancos = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCTAF, AdministracionId, TokenCookie);
				ListaCuentaBancos = listaCuentasBancos;
				model.ListaCuentasBancarias = ComboCuentaBancos(ListaCuentaBancos);

				return PartialView("_paso1", model);
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
		protected SelectList ComboCuentaBancos(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
