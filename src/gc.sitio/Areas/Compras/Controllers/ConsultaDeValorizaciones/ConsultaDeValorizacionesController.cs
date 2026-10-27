using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Ocsp;

namespace gc.sitio.Areas.Compras.Controllers.ConsultaDeValorizaciones
{
	[Area("Compras")]
	public class ConsultaDeValorizacionesController : ConsultaDeValorizacionesControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private const string valorizacion_class_blue = "badge bg-label-info-strong me-1";
		private const string valorizacion_class_red = "badge bg-label-danger-strong me-1";
		public ConsultaDeValorizacionesController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDeValorizacionesController> logger,
												  ICuentaServicio cuentaServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDeValorizacionesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var titulo = "CONSULTA DE VALORIZACIONES REGISTRADAS";
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

		public IActionResult InitPantallaPrincipal(string cta_id, string cta_desc, DateTime desde, DateTime hasta)
		{
			var model = new InitConsultaDeValorizacionesModel
			{
				Proveedor = $"{cta_desc}",
				RangoDeFechas = $"{desde:dd/MM/yyyy} - {hasta:dd/MM/yyyy}"
			};
			return PartialView("_partial_Principal", model);
		}

		public IActionResult CargarListaValorizaciones(ObtenerValorizacionesRegistradasRequest request)
		{
			var model = new ListaValorizacionesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var valorizaciones = _cuentaServicio.ObtenerValorizacionesRegistradas(request, TokenCookie);
				model.Grilla = ObtenerGridCoreSmart<ConsultaValorizacionesRegistradasDto>(valorizaciones);
				return PartialView("_partial_ListaValorizaciones", model);
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

		public IActionResult CargarListaComprobantesDeValorizacion(ObtenerComptesDeValorizacionRequest request)
		{
			var model = new ListaComprobantesDeValorizacionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (request.cta_id == "" && request.cm_compte == "")
				{
					model.Grilla = ObtenerGridCoreSmart<ComptesDeValorizacionDto>(new List<ComptesDeValorizacionDto>());
					return PartialView("_partial_ListaComptesDeValorizacion", model);
				}
				var valorizaciones = _cuentaServicio.ObtenerComptesDeValorizacion(request, TokenCookie);
				model.Grilla = ObtenerGridCoreSmart<ComptesDeValorizacionDto>(valorizaciones);
				return PartialView("_partial_ListaComptesDeValorizacion", model);
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

		public IActionResult CargarListaDetalleRPR(CompteValorizaRprDtosRequest request)
		{
			var model = new ListaDetalleRprModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				if (request.cta_id == "" && request.cm_compte == "")
				{
					model.leyenda = "Sin datos";
					model.Grilla = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(new List<CompteValorizaDetalleRprListaDto>());
					return PartialView("_partial_DetalleRpr", model);
				}
				var responseRpr = _cuentaServicio.ObtenerComprobantesDetalleRpr(request, TokenCookie);
				RecalcularCostos(responseRpr); //Por pedido de CR pide recalcular los costos por las dudas vengan con errores desde la DB (?)
				CalcularValorizacionDC_DP(responseRpr);
				model.Grilla = ObtenerGridCoreSmart<CompteValorizaDetalleRprListaDto>(responseRpr);
				model.leyenda = request.leyenda;
				return PartialView("_partial_DetalleRpr", model);
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
		private void CargarDatosIniciales(ConsultaDeValorizacionesModel model)
		{
			if (ProveedoresLista.Count == 0)
			{
				ObtenerProveedores(_cuentaServicio, "BI");
			}
			model.FechaDesde = DateTime.Now.AddMonths(-1);
			model.FechaHasta = DateTime.Now;
		}
		private void CalcularValorizacionDC_DP(List<CompteValorizaDetalleRprListaDto> lista)
		{
			if (lista.Count > 0)
			{
				foreach (var item in lista)
				{
					var boni = 0.00M;
					//if (!string.IsNullOrWhiteSpace(item.rpd_boni))
					//	boni = CalcularBoni2(item.rpd_boni ?? "", item.rpd_cantidad_compte);
					var result = item.rpd_cantidad - item.rpd_cantidad_compte;
					if (result == 0)
						item.valorizacion_mostrar_dc = false;
					else
					{
						item.valorizacion_mostrar_dc = true;
						if (result > 0)
						{
							item.valorizacion_class_dc = valorizacion_class_blue;
							item.valorizacion_value_dc = "+";
						}
						else
						{
							item.valorizacion_class_dc = valorizacion_class_red;
							item.valorizacion_value_dc = "-";
						}
					}

					var result2 = Math.Round(item.ocd_pcosto, 2) - Math.Round(item.rpd_pcosto, 2);
					if (result2 == 0)
						item.valorizacion_mostrar_dp = false;
					else
					{
						item.valorizacion_mostrar_dp = true;
						if (result2 > 0)
						{
							item.valorizacion_class_dp = valorizacion_class_blue;
							item.valorizacion_value_dp = "+";
						}
						else
						{
							item.valorizacion_class_dp = valorizacion_class_red;
							item.valorizacion_value_dp = "-";
						}
					}
				}
			}
		}
		private void RecalcularCostos(List<CompteValorizaDetalleRprListaDto> lista)
		{
			try
			{
				if (lista.Count > 0)
				{
					foreach (var item in lista)
					{
						Console.WriteLine($"ocd_pcosto anterior: {item.ocd_pcosto:N2}");
						item.ocd_pcosto = CalcularPCosto(item.ocd_plista, item.ocd_dto1, item.ocd_dto2, item.ocd_dto3, item.ocd_dto4, item.ocd_dto_pa, item.ocd_boni, 0, item.rpd_cantidad);
						Console.WriteLine($"ocd_pcosto posterior: {item.ocd_pcosto:N2}");
						Console.WriteLine($"///////////////////////////////////////////////////////////////");
						Console.WriteLine($"rpd_pcosto anterior: {item.rpd_pcosto:N2}");
						item.rpd_pcosto = CalcularPCosto(item.rpd_plista, item.rpd_dto1, item.rpd_dto2, item.rpd_dto3, item.rpd_dto4, item.rpd_dto_pa, item.rpd_boni, 0, item.rpd_cantidad_compte);
						Console.WriteLine($"rpd_pcosto posterior: {item.rpd_pcosto:N2}");
						Console.WriteLine($"///////////////////////////////////////////////////////////////");
					}
				}
			}
			catch (Exception)
			{
				throw;
			}

		}
		private static decimal CalcularPCosto(decimal p_plista, decimal p_d1, decimal p_d2, decimal p_d3, decimal p_d4, decimal p_dpa, string p_boni, decimal flete, decimal cantidad = 0)
		{
			var boni = CalcularBoni(p_boni, cantidad);
			var boni2 = CalcularBoni2(p_boni, cantidad);
			return p_plista * ((100 - p_d1) / 100) * ((100 - p_d2) / 100) * ((100 - p_d3) / 100) * ((100 - p_d4) / 100) * ((100 - p_dpa) / 100) * boni2 * ((100 + flete) / 100);
		}
		/// <summary>
		/// Funcion que calcula la bonificacion a agregar a las cantidades
		/// </summary>
		/// <param name="val">valor de tipo string en formato NNN/NNN, Ej: 110/100 -> 110 unidades entregadas por cada 100 compradas, la bonificacion serían los 10</param>
		/// <param name="cant">cantidad de productos comprados</param>
		/// <returns></returns>
		private static int CalcularBoni(string val, decimal cant)
		{
			var boni = 1;
			if (string.IsNullOrWhiteSpace(val))
			{
				return boni;
			}
			var arr = val.Split('/');
			if (arr.Length < 2)
			{
				return boni;
			}
			if (!int.TryParse(arr[0], out int num))
			{
				return boni;
			}
			if (!int.TryParse(arr[1], out int den))
			{
				return boni;
			}
			if (num > den)
			{
				return boni;
			}
			var res = den - num; //En la bonificacion viene NNN/MMM donde sería "cada NNN, lleva MMM", siendo MMM mayor a NNN. La diferencia es el valor adicional que se suma al pedido.
			if (num != 0)
			{
				var multiplo = cant / num;
				if (multiplo > 1)
				{
					boni = (res * (int)multiplo);
				}
			}
			else
				boni = 1;


			return boni;
		}

		private static decimal CalcularBoni2(string val, decimal cant)
		{
			var boni = 1;
			if (string.IsNullOrWhiteSpace(val))
			{
				return boni;
			}
			var arr = val.Split('/');
			if (arr.Length < 2)
			{
				return boni;
			}
			if (!int.TryParse(arr[0], out int num))
			{
				return boni;
			}
			if (!int.TryParse(arr[1], out int den))
			{
				return boni;
			}
			//if (num > den)
			//{
			//	return boni;
			//}
			//return Decimal.Divide(den, num);
			if (den == 0)
				return 1;
			return Decimal.Divide(num, den);
		}
		#endregion
	}
}
