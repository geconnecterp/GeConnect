using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ChequePagaAcaController : ChequePagaAcaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly string tipoCF = "CH";

		public ChequePagaAcaController(IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio,
									   IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ChequePagaAcaController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			var model = new PasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CHEQUE PAGA ACÁ y CAMBIO DE FECHA DE PRESENTACIÓN";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(true);

				var listaCuentaValores = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCF, AdministracionId, TokenCookie);
				ListaFinancieroDesdeSeleccionDeTipo = listaCuentaValores;
				model.ListaCuentaValoresEnCartera = ComboCuentaValoresEnCartera(listaCuentaValores);

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

		public IActionResult CargarChequesDeTercerosEnCartera(string ctaf_id, string cta_id, bool mostrarFecha, bool docEnCuenta)
		{
			var model = new CargarChequesDeTercerosEnCarteraModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var cheques = _financieroServicio.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, TokenCookie, cta_id);

				if (cheques == null || cheques.Count() == 0)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "No se han encontrado Cheques en Cartera"
					};
					return PartialView("_gridMensaje", response);
				}

				CambioDeFechaDePresentacion = mostrarFecha;
				DocumentoEnCuenta = docEnCuenta;

				ListaFinancieroCartera = cheques;
				var item = cheques.First();
				model.titulo_col_1 = item.ins_dato1_desc;
				model.titulo_col_2 = item.ins_dato2_desc;
				model.titulo_col_3 = item.ins_dato3_desc;
				model.GrillaChequesEnCartera = ObtenerGridCoreSmart<FinancieroCarteraDto>(cheques);
				model.mostrar_fecha = mostrarFecha;
				model.fecha_valor = DateTime.Today;
				return PartialView("_chequesDeTercerosEnCartera", model);
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

		public IActionResult PasoUno()
		{
			var model = new PasoUnoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaCuentaValores = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCF, AdministracionId, TokenCookie);
				model.ListaCuentaValoresEnCartera = ComboCuentaValoresEnCartera(listaCuentaValores);

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

		public JsonResult ConfirmarCargaDeChequeDeTerceroEnCartera(string dia_movi, string fc_compte, string fc_item, DateTime fecha_valor)
		{
			try
			{
				if (string.IsNullOrEmpty(dia_movi) || string.IsNullOrEmpty(fc_compte) || string.IsNullOrEmpty(fc_item))
					return Json(new { error = true, warn = false, msg = $"Faltan especificar algunos datos. dia_movi: {dia_movi} - fc_compte: {fc_compte} - fc_item: {fc_item}" });

				var itemEnListaFinancieroCartera = ListaFinancieroCartera.Where(x => x.dia_movi.Equals(dia_movi) && x.fc_item.Equals(Convert.ToInt32(fc_item)) && x.fc_compte == fc_compte);
				if (itemEnListaFinancieroCartera == null || itemEnListaFinancieroCartera.Count() <= 0)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener el ítem. dia_movi: {dia_movi} - fc_compte: {fc_compte} - fc_item: {fc_item}" });


				var request = new ConfirmarTransferenciaRequest();
				request.ttra_id = DocumentoEnCuenta ? "CQ" : "CF";
				Console.WriteLine($"ttra_id: {request.ttra_id}");
				request.usu_id = UserName;
				Console.WriteLine($"usu_id : {request.usu_id}");
				request.adm_id = AdministracionId;
				Console.WriteLine($"adm_id: {request.adm_id}");
				request.tra_concepto = string.Empty;
				Console.WriteLine($"tra_concepto : {request.tra_concepto}");
				request.tra_fecha = CambioDeFechaDePresentacion ? fecha_valor : null;
				Console.WriteLine($"tra_fecha: {request.tra_fecha}");

				var ctafDenominacion = string.Empty;
				var i = itemEnListaFinancieroCartera.First();
				var itemCtaf = ListaFinancieroDesdeSeleccionDeTipo.Where(x => x.ctaf_id.Equals(i.ctaf_id));
				if (itemCtaf.Any())
					ctafDenominacion = itemCtaf.First().ctaf_denominacion;

				var newValor = new ValoresDesdeObligYCredDto()
				{
					ctaf_id = i.ctaf_id,
					ctaf_denominacion = ctafDenominacion,
					tcf_id = i.tcf_id,
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
				return AnalizarRespuesta(respuesta, "La Transferencia se confirmó con Éxito");
				//return Json(new { error = false, warn = false, msg = "Anulación de comprobante correctamente." });
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
				CambioDeFechaDePresentacion = false;
				DocumentoEnCuenta = false;
				ListaFinancieroCartera = [];
				ListaFinancieroDesdeSeleccionDeTipo = [];
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

		#region Métodos privados
		protected SelectList ComboCuentaValoresEnCartera(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private void CargarDatosIniciales(bool actualizar)
		{
			if (CuentasLista.Count == 0 || actualizar)
				ObtenerCuentas(_cuentaServicio, 'C', "%");

		}
		#endregion
	}
}
