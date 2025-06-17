using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Dtos;
using gc.infraestructura.ViewModels;
using gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Model;
using gc.sitio.Areas.ControlComun.Models.SeleccionDeValores.Request;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class SeleccionDeValoresController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;

		public SeleccionDeValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<SeleccionDeValoresController> logger,
											ICuentaServicio cuentaServicio,ITipoCuentaFinServicio tipoCuentaFinServicio, IFinancieroServicio financieroServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
		}

		public string CtaValoresANombre
		{
			get
			{
				var txt = _context.HttpContext?.Session.GetString("CtaValoresANombre");
				if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
				{
					return string.Empty;
				}
				return JsonConvert.DeserializeObject<string>(txt) ?? string.Empty;
			}
			set
			{
				var valor = JsonConvert.SerializeObject(value);
				_context.HttpContext?.Session.SetString("CtaValoresANombre", valor);
			}

		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public IActionResult AbrirComponente(AbrirComponenteSeleccionDeValoresRequest req)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				CtaValoresANombre = req.valor_a_nombre_de;
				var tipoCuentaFinLista = _tipoCuentaFinServicio.GetTipoCuentaFinParaSeleccionDeValores(req.app, TokenCookie);
				var model = new SeleccionDeValoresViewModel()
				{
					GrillaTipoCuentaFin = ObtenerGridCoreSmart<TipoCuentaFinDto>(tipoCuentaFinLista)
				};
				return View("~/areas/ControlComun/views/SeleccionDeValores/_index.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult CargarCtaFinParaSeleccionDeValores(string tcf_id)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var finLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tcf_id, AdministracionId, TokenCookie);
				var model = ObtenerGridCoreSmart<FinancieroDesdeSeleccionDeTipoDto>(finLista);
				return View("~/areas/ControlComun/views/SeleccionDeValores/_grillaFinancieros.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult CargarGrillaFinancieroCarteraEnSeleccionDeValores(string ctaf_id)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var model = new EdicionTipoValoresDeTercerosEnCarteraModel();
				var finCarLista = _financieroServicio.GetFinancieroCarteraParaSeleccionDeValores(ctaf_id, TokenCookie);
				model.GrillaValoresEnCartera = ObtenerGridCoreSmart<FinancieroCarteraDto>(finCarLista);
				return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_terceros_en_cartera.cshtml", model);
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public IActionResult CargarSeccionEdicionEnSeleccionDeValores(string tcf_id)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				switch (tcf_id)
				{
					case "BA":
						var modelBA = new EdicionTipoTransferenciaBancariaModel();
						return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_transferencia_bancaria.cshtml", modelBA);
					case "CH":
						var modelCH = new EdicionTipoValoresDeTercerosEnCarteraModel()
						{
							GrillaValoresEnCartera = new GridCoreSmart<FinancieroCarteraDto>()
						};
						return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_terceros_en_cartera.cshtml", modelCH);
					case "EC":
						var modelEC = new EdicionTipoEmisionChequesModel() { ANombreDe = CtaValoresANombre };
						return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_emision_cheques.cshtml", modelEC);
					case "EF":
						var modelEF = new EdicionTipoEfectivoCajasModel();
						return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_efectivo_cajas.cshtml", modelEF);
					default:
						response.Mensaje = "Variante de Tipo no configurada!";
						response.Ok = false;
						response.EsWarn = false;
						response.EsError = true;
						return PartialView("_gridMensaje", response);
				}
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{

				string msg = "Error en la obtención de la configuración para el Gestor Documental.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		[HttpPost]
		public JsonResult AgregarItemAColeccionDeValores(AgregarItemModel req)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var resultado = ValidarItemParaAgregarAColeccion(req);
				return Json(new { error = !resultado.Exito, warn = false, msg = resultado.Mensaje });
			}
			catch (NegocioException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return Json(response);
			}
			catch (Exception ex)
			{
				string msg = "Error al devolver el valor cargado.";
				_logger?.LogError(ex, msg);
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return Json(response);
			}
		}

		#region Métodos Privados
		private Resultado ValidarItemParaAgregarAColeccion(AgregarItemModel req)
		{
			try
			{
				if (req == null)
					return new Resultado() { Exito = false, Mensaje = "Request vacío." };
				if (string.IsNullOrEmpty(req.DataType))
					return new Resultado() { Exito = false, Mensaje = "No se ha especificado el tipo." };
				if (req.DataObject == null)
					return new Resultado() { Exito = false, Mensaje = "Objeto origen vacío." };
				if (req.DataObject.Count <= 0)
					return new Resultado() { Exito = false, Mensaje = "Objeto origen vacío." };
				if (!NoExisteElItem(req))
					return new Resultado() { Exito = false, Mensaje = "El/Los elemento/s que esta intentando agregar ya existe/n." };

				var listaAux = new List<ValoresDesdeObligYCredDto>();

				foreach (var item in req.DataObject)
				{
					var objAux = item;
					ObtenerConceptoValor(objAux);
					listaAux.Add(objAux);
				}

				var listaTemp = OPValoresDesdeObligYCredLista;
				listaTemp.AddRange(listaAux);
				OPValoresDesdeObligYCredLista = listaTemp;
				return new Resultado() { Exito = true, Mensaje = "Valores agregados con éxito." };
			}
			catch (Exception)
			{
				return new Resultado() { Exito = false, Mensaje = "Ha ocurrido un error al intentar validar y agregar el valor a la colección." };
			}
		}

		private bool NoExisteElItem(AgregarItemModel req)
		{
			var result = true;
			switch (req.DataType)
			{
				case "BA": //Transferencias Bancarias 
						   //Controlo que el numero de la transferencia y la entidad no esten ya cargadas.
					if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0 && OPValoresDesdeObligYCredLista.Any(x => x.tcf_id.Equals("BA")))
					{
						foreach (var item in req.DataObject)
						{
							if (OPValoresDesdeObligYCredLista.Where(x => x.tcf_id.Equals("BA"))
															 .Any(x => x.op_dato2_valor.Equals(item.op_dato2_valor) && x.ctaf_id.Equals(item.ctaf_id)))
							{
								result = false;
								break;
							}
						}
					}
					break;
				case "CH": //Valores de Terceros en Cartera 
					if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0)
					{
						if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0 && OPValoresDesdeObligYCredLista.Any(x => x.tcf_id.Equals("CH")))
						{
							foreach (var item in req.DataObject)
							{
								if (OPValoresDesdeObligYCredLista.Where(x => x.tcf_id.Equals("CH"))
													 .Any(x => x.op_dato1_valor.Equals(item.op_dato1_valor) && x.op_dato2_valor.Equals(item.op_dato2_valor)))
								{
									result = false;
									break;
								}
							}
						}
					}
					break;
				case "EC": //Emisión de Cheques 
					if (req.DataObject.First().automatico.Equals('N')) //Si es manual, controlar que el numero de cheque y la entidad no esten cargados ya
					{
						if (OPValoresDesdeObligYCredLista != null && OPValoresDesdeObligYCredLista.Count > 0 && OPValoresDesdeObligYCredLista.Any(x => x.tcf_id.Equals("EC")))
						{
							foreach (var item in req.DataObject)
							{
								if (OPValoresDesdeObligYCredLista.Where(x => x.tcf_id.Equals("EC"))
													 .Any(x => x.op_dato2_valor.Equals(item.op_dato2_valor) && x.ctaf_id.Equals(item.ctaf_id)))
								{
									result = false;
									break;
								}
							}
						}

					}
					break;
				case "EF": //Efectivos o Cajas 
					break;
				default:
					break;
			}
			return result;
		}

		private void ObtenerConceptoValor(ValoresDesdeObligYCredDto source)
		{
			var dato1_valor = string.IsNullOrWhiteSpace(source.op_dato1_valor) && string.IsNullOrWhiteSpace(source.op_dato1_desc) ? string.Empty : source.op_dato1_valor;
			var dato2_valor = string.IsNullOrWhiteSpace(source.op_dato2_valor) && string.IsNullOrWhiteSpace(source.op_dato2_desc) ? string.Empty : $"{source.op_dato2_desc}:{source.op_dato2_valor}";
			var dato3_valor = string.IsNullOrWhiteSpace(source.op_dato3_valor) && string.IsNullOrWhiteSpace(source.op_dato3_desc) ? string.Empty : $"{source.op_dato3_desc}:{source.op_dato3_valor}";
			var dato_fecha = source.op_fecha_valor != null ? $"Fecha Val.:{source.op_fecha_valor.Value:dd/MM/yyyy}" : string.Empty;
			source.concepto_valor = $"{dato1_valor} {dato2_valor} {dato3_valor} {dato_fecha}";
		}
		#endregion

		#region Clases
		private class Resultado
		{
			public bool Exito { get; set; } = false;
			public string Mensaje { get; set; } = string.Empty;
		}
		#endregion
	}
}
