using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero;
using gc.infraestructura.Dtos.Consultas.ReporteFinanciero.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Consultas.Models;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Consultas.Controllers
{
	[Area("Consultas")]
	public class ReporteFinancieroController : ReporteFinancieroControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public ReporteFinancieroController(IFinancieroServicio financieroServicio,
										   IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ReporteFinancieroController> logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTES FINANCIEROS";
				ViewData["Titulo"] = titulo;

				return View();
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

		public IActionResult BuscarProyeccionFinanciera(BuscarProyFinanRequest request)
		{
			var model = new ProyFinanModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var res = _financieroServicio.GetProyeccionFinanciera(request, TokenCookie);
				if (res == null || res.Count <= 0)
				{
					return PartialView("_gridProyFinan", model);
				}
				else
				{
					var item = res.First();
					model.SaldoBancarioDisponible = item.saldo_bco;
					model.SaldoBancarioEnDescubierto = item.saldo_bco_rojo;
					model.ValoresAlCobroNoAcreditados = item.valores_alcobro_v;
					model.DocumentosACobrarVencidos = item.valores_alcobro_ven;
					model.ProyeccionDeVentasDiarias = 0;
					AsignarLeyendaSemana(res);
					model.GrillaProyFinan = ObtenerGridCoreSmart<ProyFinanDto>(res);
					return PartialView("_gridProyFinan", model);
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

		#region Métodos privados
		private void AsignarLeyendaSemana(List<ProyFinanDto> lista)
		{
			if (lista == null || lista.Count == 0) return;

			// Agrupar por semana
			var gruposPorSemana = lista
				.GroupBy(x => x.semana)
				.OrderBy(g => g.Key);

			foreach (var grupo in gruposPorSemana)
			{
				// Obtener rango de fechas
				var fechaInicio = grupo.Min(x => x.desde).Date;
				var fechaFin = grupo.Max(x => x.hasta).Date;

				string leyenda = $"Semana del {fechaInicio:yyyy-MM-dd} al {fechaFin:yyyy-MM-dd}";

				// Asignar leyenda a cada elemento del grupo
				foreach (var item in grupo)
				{
					item.leyendaSemana = leyenda;
				}
			}
		}


		#endregion
	}
}
