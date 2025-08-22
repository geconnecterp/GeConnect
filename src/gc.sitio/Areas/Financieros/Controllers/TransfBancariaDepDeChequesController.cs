using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.EntidadesComunes;
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
using Org.BouncyCastle.Ocsp;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class TransfBancariaDepDeChequesController : TransfBancariaDepDeChequesControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.TEC.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public TransfBancariaDepDeChequesController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<TransfBancariaDepDeChequesController> logger,
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
			return View();
		}

		public IActionResult TransferenciaBancaria()
		{
			var model = new TransferenciaBancariaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				OPValoresOrigen = [];
				OPValoresDestino = [];

				var titulo = "TRANSFERENCIAS BANCARIAS Y DE CAJA CHICA O EFECTIVO";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				model.parametro_valores_origen = "TR";
				model.parametro_valores_destino = "TR";
				model.parametro_confirmacion = "TR";
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

		public IActionResult DepositoDeCheques()
		{
			var model = new DepositoDeChequesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				OPValoresOrigen = [];
				OPValoresDestino = [];

				var titulo = "DEPÓSITOS DE CHEQUES EN CARTERA";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				model.parametro_valores_origen = "DPO";
				model.parametro_valores_destino = "DPD";
				model.parametro_confirmacion = "CH";
				model.ListaIntervalo = ComboIntervalos();
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

		public JsonResult ActualizarTotales()
		{
			try
			{
				var totalOrigen = OPValoresOrigen.Sum(x => x.op_importe);
				var totalDestino = OPValoresDestino.Sum(x => x.op_importe);
				return Json(new { error = false, warn = false, msg = "", totalOrigen, totalDestino });
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

		public IActionResult CargarValores(string source, string sourceSeleccionado)
		{
			var model = new ValoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				//if (OPValoresDesdeObligYCredLista == null || OPValoresDesdeObligYCredLista.Count <= 0) {
				//	model.Grilla = new GridCoreSmart<ValoresDesdeObligYCredDto>();
				//	if (sourceSeleccionado.Equals("1"))
				//		return PartialView("_grillaValoresOrigen", model);
				//	else
				//		return PartialView("_grillaValoresDestino", model);
				//}
				var orden = 1;
				if (sourceSeleccionado.Equals("1"))
				{
					if (OPValoresOrigen == null)
						OPValoresOrigen = [];
					var listaTemp = OPValoresOrigen;
					listaTemp.AddRange(OPValoresDesdeObligYCredLista);
					OPValoresDesdeObligYCredLista = [];
					listaTemp.ForEach(x => x.orden = orden++);
					OPValoresOrigen = listaTemp;
					model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresOrigen);
					return PartialView("_grillaValoresOrigen", model);
				}
				else 
				{
					if (OPValoresDestino == null)
						OPValoresDestino = [];
					var listaTemp = OPValoresDestino;
					listaTemp.AddRange(OPValoresDesdeObligYCredLista);
					OPValoresDesdeObligYCredLista = [];
					listaTemp.ForEach(x => x.orden = orden++);
					OPValoresDestino = listaTemp;
					model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDestino);
					return PartialView("_grillaValoresDestino", model);
				}
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

		public JsonResult InicializarDatosEnSesion() 
		{
			try
			{
				OPValoresOrigen = [];
				OPValoresDestino = [];
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

		public IActionResult RecargarGrillaOrigen()
		{
			var model = new ValoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresOrigen);
				return PartialView("_grillaValoresOrigen", model);
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

		public IActionResult RecargarGrillaDestino()
		{
			var model = new ValoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDestino);
				return PartialView("_grillaValoresDestino", model);
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
		public IActionResult ActualizarGrillaValores(int orden, string sourceSeleccionado)
		{
			var model = new ValoresModel();
			try
			{
				if (orden > 0)
				{
					if (sourceSeleccionado.Equals("tbListaOrigen"))
					{
						var listaAux = OPValoresOrigen;
						listaAux = [.. listaAux.Where(x => x.orden != orden)];
						OPValoresOrigen = listaAux;
						model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresOrigen);
						return PartialView("_grillaValoresOrigen", model);
					}
					else
					{
						var listaAux = OPValoresDestino;
						listaAux = [.. listaAux.Where(x => x.orden != orden)];
						OPValoresDestino = listaAux;
						model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDestino);
						return PartialView("_grillaValoresDestino", model);
					}
				}
				else
				{
					if (sourceSeleccionado.Equals("tbListaOrigen"))
					{
						model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresOrigen);
						return PartialView("_grillaValoresOrigen", model);
					}
					else
					{
						model.Grilla = ObtenerGridCoreSmart<ValoresDesdeObligYCredDto>(OPValoresDestino);
						return PartialView("_grillaValoresDestino", model);
					}
				}
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
		public JsonResult ValidarAntesDeGuardar()
		{
			try
			{
				if (OPValoresOrigen != null && OPValoresOrigen.Count > 0 && OPValoresDestino != null && OPValoresDestino.Count > 0)
				{
					return Json(new { error = false, warn = false, msg = "" });
				}
				else
				{
					return Json(new { error = true, warn = false, msg = "Se deben especificar datos para Origen y Destino." });
				}
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

		[HttpPost]
		public JsonResult ConfirmarTransferencia(ConfirmarTransferenciaRequest request)
		{
			try
			{
				if (request==null)
					return Json(new { error = true, warn = false, msg = "No se han enviado datos para confirmar." });


				Console.WriteLine($"ttra_id: {request.ttra_id}");
				Console.WriteLine($"tra_concepto: {request.tra_concepto}");
				Console.WriteLine($"ttra_id: {request.ttra_id}");
				Console.WriteLine($"tra_concepto: {request.tra_concepto}");
				Console.WriteLine($"ttra_id: {request.ttra_id}");
				Console.WriteLine($"tra_concepto: {request.tra_concepto}");
				Console.WriteLine($"ttra_id: {request.ttra_id}");
				Console.WriteLine($"tra_concepto: {request.tra_concepto}");
				Console.WriteLine($"tra_fecha: {request.tra_fecha}");
				request.adm_id = AdministracionId;
				Console.WriteLine($"adm_id: {request.adm_id}");
				request.usu_id = UserName;
				Console.WriteLine($"usu_id: {request.usu_id}");
				request.json_o = JsonConvert.SerializeObject(OPValoresOrigen, new JsonSerializerSettings());
				Console.WriteLine($"json_o: {request.json_o}");
				request.json_d = JsonConvert.SerializeObject(OPValoresDestino, new JsonSerializerSettings());
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
				return AnalizarRespuesta(respuesta, "La Transferencia se confirmó con Éxito");
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

		#region Métodos Privados
		protected SelectList ComboIntervalos()
		{
			var listaTemp = new List<Intervalo>();
			var lista = ObtenerIntervalos().Select(x => new ComboGenDto { Id = x.id, Descripcion = x.descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private List<Intervalo> ObtenerIntervalos()
		{
			return [new Intervalo() { id = "1", descripcion = "24hs" }, new Intervalo() { id = "2", descripcion = "48hs" }, new Intervalo() { id = "3", descripcion = "72hs" }, new Intervalo() { id = "4", descripcion = "Otros" }];
		}

		private class Intervalo()
		{
			public string id { get; set; } = string.Empty;
			public string descripcion { get; set; } = string.Empty;
		}
		#endregion
	}
}
