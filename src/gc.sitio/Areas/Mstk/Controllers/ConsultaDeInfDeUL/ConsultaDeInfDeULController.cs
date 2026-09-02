using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.ConsultaDeInfDeUL
{
	[Area("Mstk")]
	public class ConsultaDeInfDeULController : ConsultaDeInfDeULControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IProducto2Servicio _producto2Servicio;
		public ConsultaDeInfDeULController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeInfDeULController> logger,
										   IProducto2Servicio producto2Servicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_producto2Servicio = producto2Servicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeInfDeULModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "INFORMACIÓN DE UNIDADES DE LECTURA";
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
		public IActionResult InicializarPantallPrincipal(string radioText, DateTime fechadesde, DateTime fechahasta)
		{
			var model = new PrincipalConsultaDeInfDeULModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.UL_Tipo = radioText;
				model.FechaDesde = fechadesde;
				model.FechaHasta = fechahasta;
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
		public IActionResult BuscarUnidadesDeLectura(string tipo, DateTime desde, DateTime hasta)
		{
			GridCoreSmart<ConsULDto> grillaDatos;
			RespuestaGenerica<EntidadBase> response = new();
			try
			{
				if (desde > hasta)
				{
					throw new NegocioException("Verifique el Periodo de fechas, ya que es incorrecta.");
				}
				var res = _producto2Servicio.ConsultaUL(tipo, desde, hasta, AdministracionId, TokenCookie).Result;
				if (res.Ok)
				{
					if (res.ListaEntidad?.Count > 0)
					{
						foreach (var item in res.ListaEntidad)
						{
							//generando imagen png en base 64 con el code 3of9
							item.ImgB64 = HelperGen.GeneraIdEnCodeBar3of9WithText(item.UL_id);
						}

						grillaDatos = ObtenerGridCoreSmart<ConsULDto>(res.ListaEntidad);
						return PartialView("_gridListadoUL", grillaDatos);
					}
					else
					{
						if (tipo == "F")
						{
							response.Mensaje = $"No se encontraron datos para el periodo {desde.ToShortDateString()} - {hasta.ToShortDateString()}";
						}
						else
						{
							response.Mensaje = $"No se encontraron UL sin almacenar.";
						}
						response.Ok = true;
						response.EsWarn = false;
						response.EsError = false;
						return PartialView("_gridMensaje", response);
					}
				}
				else
				{
					throw new NegocioException(res.Mensaje ?? "Hubo un problema para consultar la UL");
				}
			}
			catch (NegocioException ex)
			{
				_logger.LogError(ex, "Error al consultar las ULs");
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{
				string msg = "Error al Intentar consultar las ULs. Verifique.";
				_logger.LogError(ex, "Error al consultar las ULs");
				response.Mensaje = msg;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
		}

		#region Metodos Privados
		private void CargarDatosIniciales(ConsultaDeInfDeULModel model)
		{
			model.FechaHasta = DateTime.Now;
			model.FechaDesde = DateTime.Now.AddMonths(-1);
			model.UL_Por_Fecha = true;
			model.UL_Sin_Almacen = false;
		}
		#endregion
	}
}
