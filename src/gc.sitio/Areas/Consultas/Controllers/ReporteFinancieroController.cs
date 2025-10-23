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
					model.DocumentosACobrarVencidos = item.acobrar_mes_ant;
					model.ProyeccionDeVentasDiarias = item.proy_vtas;
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

		public IActionResult BuscarSaldoDeCuentas(BuscarSaldoDeCuentasRequest request) 
		{
			var model = new SaldoDeCuentaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var res = _financieroServicio.GetSaldoDeCuentas(request, TokenCookie);
				if (res == null || res.Count <= 0)
				{
					return PartialView("_gridSaldoCuenta", model);
				}
				else
				{
					model.GrillaSaldoDeCuenta = ObtenerGridCoreSmart<SaldoDeCuentaDto>(res);
					return PartialView("_gridSaldoCuenta", model);
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

		public IActionResult BuscarFlujoDeIngreso(BuscarFlujoDeIngresoRequest request)
		{
			var model = new FlujoDeIngresoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				request.adm_id = AdministracionId;
				var res = _financieroServicio.GetFlujoDeIngreso(request, TokenCookie);
				if (res == null || res.Count <= 0)
				{
					return PartialView("_gridFlujoIngr", model);
				}
				else
				{
					model.GrillaProyFinan = ObtenerGridCoreSmart<FlujoDeIngresoDto>(res);
					return PartialView("_gridFlujoIngr", model);
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

		public IActionResult BuscarPoyeccionEgresoGroup()
		{
			var model = new ProyEgrGroupModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaProyeccion = _financieroServicio.GetGastosProyLista(TokenCookie);
				ListaProyeccionDeEgresos = listaProyeccion;
				var lista = AgruparGastosPorFechaConAcumulado(listaProyeccion);
				model.GrillaProyEgrGroup = ObtenerGridCoreSmart<ProyEgrGroupDto>(lista);
				return PartialView("_gridProyEgrGroup", model);
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

		public IActionResult BuscarPoyeccionEgresoDetail(DateTime fecha)
		{
			var model = new ProyEgrDetailModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var listaFiltrada = ListaProyeccionDeEgresos.Where(x => x.fecha.Date == fecha.Date).ToList();
				model.GrillaProyEgrDetail = ObtenerGridCoreSmart<ProyEgrDetailDto>(MapperProyEgrDetal(listaFiltrada));
				model.fecha = fecha.ToString("dd/MM/yyyy");
				return PartialView("_gridProyEgrDetail", model);
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
		private List<ProyEgrDetailDto> MapperProyEgrDetal(List<GastoProyListaDto> lstProyEgre)
		{
			if (lstProyEgre == null || lstProyEgre.Count <= 0)
				return [];
			var resultado = lstProyEgre
				.Select(g => new ProyEgrDetailDto
				{
					fecha = g.fecha,
					concepto = g.concepto,
					importe = g.importe
				})
				.ToList();
			return resultado;
		}
		private List<ProyEgrGroupDto> AgruparGastosPorFechaConAcumulado(List<GastoProyListaDto> gastos)
		{
			var agrupados = gastos
				.GroupBy(g => g.fecha.Date)
				.OrderBy(g => g.Key)
				.Select(g => new
				{
					Fecha = g.Key,
					Importe = g.Sum(x => x.importe)
				})
				.ToList();

			var resultado = new List<ProyEgrGroupDto>();
			decimal acumulado = 0;

			foreach (var grupo in agrupados)
			{
				acumulado += grupo.Importe;
				resultado.Add(new ProyEgrGroupDto
				{
					fecha = grupo.Fecha,
					prevision_egresos = grupo.Importe,
					prevision_acumulada = acumulado
				});
			}

			return resultado;
		}

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
