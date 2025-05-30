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
using Org.BouncyCastle.Ocsp;

namespace gc.sitio.Areas.ControlComun.Controllers
{
	[Area("ControlComun")]
	public class SeleccionDeValoresController : ControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ITipoCuentaFinServicio _tipoCuentaFinServicio;
		private readonly IFinancieroServicio _financieroServicio;

		public SeleccionDeValoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<SeleccionDeValoresController> logger,
											ITipoCuentaFinServicio tipoCuentaFinServicio, IFinancieroServicio financieroServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCuentaFinServicio = tipoCuentaFinServicio;
			_financieroServicio = financieroServicio;
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
				var finLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tcf_id, TokenCookie);
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
							GrillaValoresEnCartera = new GridCoreSmart<ValoresEnCarteraDto>()
						};
						return View("~/areas/ControlComun/views/SeleccionDeValores/_edicion_tipo_terceros_en_cartera.cshtml", modelCH);
					case "EC":
						var modelEC = new EdicionTipoEmisionChequesModel();
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
		public JsonResult AgregarItemAColeccionDeValores([FromBody] AgregarItemModel req)
		{
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				var resultado = ValidarItemParaAgregarAColeccion(req);
				response.Ok = true;
				response.Mensaje = "Valor cargado correctamente.";
				return Json(response);
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
				if (req.Data == null)
					return new Resultado() { Exito = false, Mensaje = "Objeto origen vacío." };

				var newItem = new ValoresDesdeObligYCredDto();
				switch (req.DataType)
				{
					case "BA": //Transferencias Bancarias 
						var objBA = (EdicionTipoTransferenciaBancariaModel)req.Data;

						break;
					case "CH": //Valores de Terceros en Cartera 
						var objCH = (EdicionTipoValoresDeTercerosEnCarteraModel)req.Data;
						break;
					case "EC": //Emisión de Cheques 
						var objEC = (EdicionTipoEmisionChequesModel)req.Data;
						break;
					case "EF": //Efectivos o Cajas 
						var objEF = (EdicionTipoEfectivoCajasModel)req.Data;
						break;
					default:
						return new Resultado() { Exito = false, Mensaje = "Variable no configurada." };
				}
				return new Resultado() { Exito = false, Mensaje = "Objeto origen vacío." };
			}
			catch (Exception)
			{
				return new Resultado() { Exito = false, Mensaje = "Ha ocurrido un error al intentar validar y agregar el valor a la colección." };
			}

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
