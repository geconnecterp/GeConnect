using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class TransfBancariaDepDeChequesController : TransfBancariaDepDeChequesControladorBase
	{
		private readonly AppSettings _setting;
		public TransfBancariaDepDeChequesController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<TransfBancariaDepDeChequesController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult TransferenciaBancaria()
		{
			var model = new TransferenciaBancariaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				ViewData["Titulo"] = "TRANSFERENCIAS BANCARIAS Y DE CAJA CHICA O EFECTIVO";
				model.parametro_valores_origen = "TR";
				model.parametro_valores_destino = "TR";
				model.parametro_confirmacion = "TR";
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

		public IActionResult DepositoDeCheques()
		{
			var model = new DepositoDeChequesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				ViewData["Titulo"] = "DEPÓSITOS DE CHEQUES EN CARTERA";
				model.parametro_valores_origen = "DPO";
				model.parametro_valores_destino = "DPD";
				model.parametro_confirmacion = "CH";
				model.ListaIntervalo = ComboIntervalos();
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
		protected SelectList ComboIntervalos()
		{
			var listaTemp = new List<Intervalo>();
			var lista = ObtenerIntervalos().Select(x => new ComboGenDto { Id = x.id, Descripcion = x.descripcion });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		private List<Intervalo> ObtenerIntervalos()
		{
			return [new Intervalo() { id = "1", descripcion = "24hs" }, new Intervalo() { id = "2", descripcion = "48hs" }, new Intervalo() { id = "3", descripcion = "72hs" }, new Intervalo() { id = "4", descripcion = "Otros" }];
		}

		private class Intervalo()
		{
			public string id { get; set; } = string.Empty;
			public string descripcion { get; set; } = string.Empty;
		}
		#endregion
	}
}
