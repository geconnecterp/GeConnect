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
using gc.sitio.Areas.Financieros.Models.PresentacionDeValoresEnCartera;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class PresentacionDeValoresEnCarteraController : PresentacionDeValoresEnCarteraControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.TEC.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private const string param_tipo_medio_pago = "TEND";
		public PresentacionDeValoresEnCarteraController(ITipoCuentaFinServicio tipoCuentaFinServicio, IFinancieroServicio financieroServicio,
														IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
														IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<PresentacionDeValoresEnCarteraController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_financieroServicio = financieroServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: TEC
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new PresDeValEnCartera_Paso1Model();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "TRANSFERENCIAS DE VALORES EN CARTERA";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				CtafIdSelected = string.Empty;
				OPValoresSeleccionados = [];
				FinancieroCarteraLista = [];

				var lista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores(param_tipo_medio_pago, TokenCookie);
				model.ListaTipoMedioDePago = ComboTipoMediosDePago(lista);
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

		public IActionResult SeleccionCuentaFin(string tcf_id)
		{
			var model = new SeleccionCtaFinModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tcf_id, AdministracionId, TokenCookie);
				model.GrillaCtaFin = ObtenerGridCoreSmart<FinancieroDesdeSeleccionDeTipoDto>(lista);
				return PartialView("_seleccionCtaFin", model);
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

		public IActionResult Paso1()
		{
			var model = new PresDeValEnCartera_Paso1Model();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores(param_tipo_medio_pago, TokenCookie);
				model.ListaTipoMedioDePago = ComboTipoMediosDePago(lista);
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

		public IActionResult SeleccionValoresAPresentar(string ctaf_id, string ctaf_desc)
		{
			var model = new SeleccionValoresAPresentarModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var lista = _financieroServicio.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, TokenCookie);
				if (lista != null && lista.Count > 0)
				{
					FinancieroCarteraLista = lista;
					model.GrillaValoresAPresentar = ObtenerGridCoreSmart<FinancieroCarteraDto>(lista);
					var item = lista.First();
					model.ctaf_id = ctaf_id;
					model.ctaf_desc = ctaf_desc;
					model.titulo_col_1 = item.ins_dato1_desc ?? string.Empty;
					model.titulo_col_2 = item.ins_dato2_desc ?? string.Empty;
					model.titulo_col_3 = item.ins_dato3_desc ?? string.Empty;
				}
				return PartialView("_seleccionValoresAPresentar", model);
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

		public IActionResult DetalleDePresentacion(DetalleDePresentacionRequest request)
		{
			var model = new DetalleDePresentacionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var cuenta_al_cobro_lista = _financieroServicio.GetCuentaAlCobroRela(request.ctafIdSelected, TokenCookie);
				if (cuenta_al_cobro_lista == null || cuenta_al_cobro_lista.Count <= 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = $"No se ha encontrado la Cuenta al Cobro de la Cuenta en Cartera {request.ctafIdSelected} - {request.ctafDescSelected}"
					};
					return PartialView("_detalleDePresentacionConError", response);
				}

				var listaTemp = new List<ValoresDesdeObligYCredDto>();
				var lista1 = request.ctafIdLista.Split(',');
				foreach (var item in lista1)
				{
					if (item.Trim() == "")
						continue;
					var subItem = item.Split('|');
					var i = FinancieroCarteraLista.Where(x => x.dia_movi.Equals(subItem[0]) && x.fc_compte.Equals(subItem[1]) && x.fc_item.Equals(Convert.ToInt32(subItem[2]))).First();
					var newValor = new ValoresDesdeObligYCredDto()
					{
						ctaf_id = i.ctaf_id,
						ctaf_denominacion = string.Empty,
						tcf_id = request.tcfIdSelected,
						tipo = " ",
						automatico = ' ',
						op_dato1_valor = i.fc_dato1_valor,
						op_dato1_desc = i.ins_dato1_desc,
						op_dato2_valor = i.fc_dato2_valor,
						op_dato2_desc = i.ins_dato2_desc,
						op_dato3_valor = i.fc_dato3_valor,
						op_dato3_desc = i.ins_dato3_desc,
						op_importe = i.fc_importe,
						op_fecha_valor = i.fc_fecha_valor,
						fc_compte = i.fc_compte,
						fc_item = i.fc_item,
						fc_dia_movi = i.dia_movi,
						fc_cta_id = i.cta_id,
						fc_anombre = " ",
						concepto_valor = i.concepto_valor,
						resultado = 0,
						resultado_msj = " ",
					};
					listaTemp.Add(newValor);
				}
				OPValoresSeleccionados = listaTemp;
				var cuenta_al_cobro = cuenta_al_cobro_lista.First();
				model.concepto = string.Empty;
				model.fecha_acreditacion = DateTime.Today;
				model.cuenta_en_cartera = $"{request.ctafIdSelected} {request.ctafDescSelected}";
				model.saldo_cuenta_en_cartera = request.saldoDeCtaf;
				model.importe_a_presentar_en_cartera = request.totalSeleccionadoEnCartera;
				model.saldo_a_constituir_en_cartera = request.saldoDeCtaf - request.totalSeleccionadoEnCartera;
				model.ctaf_id_cartera = request.ctafIdSelected;
				model.ctaf_desc_cartera = request.ctafDescSelected;

				model.cuenta_al_cobro = $"{cuenta_al_cobro.ctaf_id} {cuenta_al_cobro.ctaf_denominacion}";
				model.importe_a_presentar_al_cobro = request.totalSeleccionadoEnCartera;
				model.saldo_cuenta_al_cobro = cuenta_al_cobro.ctaf_saldo;
				model.saldo_a_constituir_al_cobro = model.importe_a_presentar_al_cobro + model.saldo_cuenta_al_cobro;
				model.ctaf_id_al_cobro = cuenta_al_cobro.ctaf_id;
				model.ctaf_desc_al_cobro = cuenta_al_cobro.ctaf_denominacion;
				return PartialView("_detalleDePresentacion", model);
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

		public JsonResult ConfirmarPresentacionDeValores(ConfirmarPresentacionDeValoresRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = "No se han enviado datos para confirmar." });

				var req = new ConfirmarTransferenciaRequest();
				Console.WriteLine($"ttra_id: PR");
				req.ttra_id = "PR";
				Console.WriteLine($"tra_concepto: {request.concepto}");
				Console.WriteLine($"tra_concepto: {request.concepto}");
				Console.WriteLine($"tra_concepto: {request.concepto}");
				Console.WriteLine($"tra_concepto: {request.concepto}");
				req.tra_concepto = request.concepto;
				Console.WriteLine($"tra_fecha: {request.fecha_acreditacion}");
				req.tra_fecha = request.fecha_acreditacion;
				req.adm_id = AdministracionId;
				Console.WriteLine($"adm_id: {req.adm_id}");
				req.usu_id = UserName;
				Console.WriteLine($"usu_id: {req.usu_id}");
				req.json_o = JsonConvert.SerializeObject(OPValoresSeleccionados, new JsonSerializerSettings());
				Console.WriteLine($"json_o: {req.json_o}");
				var listaTemp = OPValoresSeleccionados;
				listaTemp.ForEach(x => { x.ctaf_id = request.ctaf_id_al_cobro; x.ctaf_denominacion = request.ctaf_desc_al_cobro; });
				OPValoresSeleccionados= listaTemp;
				req.json_d = JsonConvert.SerializeObject(OPValoresSeleccionados, new JsonSerializerSettings());
				Console.WriteLine($"json_d: {req.json_d}");

				var encabezado = new Encabezado();
				var ListaConceptoFacturado = new List<ConceptoFacturadoDto>();
				var ListaOtrosTributos = new List<OtroTributoDto>();

				req.json_concepto = JsonConvert.SerializeObject(ListaConceptoFacturado, new JsonSerializerSettings());
				Console.WriteLine($"json_concepto: {req.json_concepto}");
				req.json_encabezado = JsonConvert.SerializeObject(ListaConceptoFacturado, new JsonSerializerSettings());
				Console.WriteLine($"json_encabezado: {req.json_encabezado}");
				req.json_otro = JsonConvert.SerializeObject(ListaOtrosTributos, new JsonSerializerSettings());
				Console.WriteLine($"json_otro: {req.json_otro}");
				var respuesta = _financieroServicio.FinancieroConfirmarTransferencia(req, TokenCookie);
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

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				CtafIdSelected = string.Empty;
				OPValoresSeleccionados = [];
				FinancieroCarteraLista = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - PRESENTACIONDEVALORESENCARTERA" });
			}
		}

		#region Métodos privados
		protected SelectList ComboTipoMediosDePago(List<TipoCuentaFinDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tcf_id, Descripcion = x.tcf_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
