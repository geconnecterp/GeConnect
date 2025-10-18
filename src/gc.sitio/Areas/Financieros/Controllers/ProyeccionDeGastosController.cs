using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ProyeccionDeGastosController : ProyeccionDeGastosControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public ProyeccionDeGastosController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ProyeccionDeGastosController> logger,
											IFinancieroServicio financieroServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			var model = new ProyeccionDeGastosModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "PROYECCIÓN DE GASTOS";
				ViewData["Titulo"] = titulo;

				var listaProyeccion = _financieroServicio.GetGastosProyLista(TokenCookie);

				model.Fecha = DateTime.Now;
				model.Importe = 0;
				model.Concepto = string.Empty;
				model.GrillaProyeccion = ObtenerGridCoreSmart<ProyeccionDeGastoDto>(MapperProyeccion(listaProyeccion));

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

		#region Métodos Privados
		private List<ProyeccionDeGastoDto> MapperProyeccion(List<GastoProyListaDto> lista)
		{ 
			if (lista == null || lista.Count == 0)
				return [];
			var resultado = new List<ProyeccionDeGastoDto>();
			int orden = 1;
			decimal acumulado = 0;
			foreach (var item in lista)
			{
				acumulado += item.importe;
				var dto = new ProyeccionDeGastoDto
				{
					orden = orden,
					fecha = item.fecha,
					concepto = item.concepto,
					importe = item.importe,
					acumulado = acumulado
				};
				resultado.Add(dto);
				orden++;
			}
			return resultado;
		}
		#endregion
	}
}
