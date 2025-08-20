using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ChequePagaAcaController : ChequePagaAcaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly string tipoCF = "CH";

		public ChequePagaAcaController(IFinancieroServicio financieroServicio,
									   IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ChequePagaAcaController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
		}

		public IActionResult Index()
		{
			var model = new PasoUnoModel();
			try
			{
				var titulo = "CHEQUE PAGA ACÁ y CAMBIO DE FECHA DE PRESENTACIÓN";
				ViewData["Titulo"] = titulo;

				var listaCuentaValores = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores(tipoCF, AdministracionId, TokenCookie);
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

		#region Métodos privados
		protected SelectList ComboCuentaValoresEnCartera(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
