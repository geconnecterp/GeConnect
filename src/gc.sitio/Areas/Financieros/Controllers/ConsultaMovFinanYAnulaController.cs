using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ConsultaMovFinanYAnulaController : ConsultaMovFinanYAnulaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoTransferenciaServicio _tipoTransferenciaServicio;
		public ConsultaMovFinanYAnulaController(IFinancieroServicio financieroServicio, ITipoTransferenciaServicio tipoTransferenciaServicio,
												IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ConsultaMovFinanYAnulaController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_tipoTransferenciaServicio = tipoTransferenciaServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaMovFinanYAnulaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA MOVIMIENTOS FINANCIEROS";
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
		public IActionResult ActualizarListaDeUsuarios(DateTime desde, DateTime hasta)
		{
			var model = new ListaUsuariosModel();
			try
			{
				var usuLista = _financieroServicio.GetFinancieroTraUsu(desde, hasta, TokenCookie);
				var listaUsu = usuLista.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
				model.ListaUsu = HelperMvc<ComboGenDto>.ListaGenerica(listaUsu);

				var usuList = new List<ComboGenDto>();
				ViewBag.UsuList = HelperMvc<ComboGenDto>.ListaGenerica(usuList);

				return PartialView("_listaUsuarios", model);
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
		private void CargarDatosIniciales(ConsultaMovFinanYAnulaModel model)
		{
			model.Date1 = DateTime.Today.AddMonths(-1);
			model.Date2 = DateTime.Today;
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.ListaCFO = ComboCTF(ctfLista);
			model.ListaCFD = ComboCTF(ctfLista);
			var tipoTransfeLista = _tipoTransferenciaServicio.GetTipoTransferenciaLista(TokenCookie);
			var listaTT = tipoTransfeLista.Select(x => new ComboGenDto { Id = x.ttra_id, Descripcion = x.ttra_lista });
			model.ListaTT = HelperMvc<ComboGenDto>.ListaGenerica(listaTT);
			var usuLista = _financieroServicio.GetFinancieroTraUsu(model.Date1, model.Date2, TokenCookie);
			var listaUsu = usuLista.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
			model.ListaUsu = HelperMvc<ComboGenDto>.ListaGenerica(listaUsu);

			var cFOList = new List<ComboGenDto>();
			ViewBag.CFOList = HelperMvc<ComboGenDto>.ListaGenerica(cFOList);
			var cFDList = new List<ComboGenDto>();
			ViewBag.CFDList = HelperMvc<ComboGenDto>.ListaGenerica(cFDList);
			var tTList = new List<ComboGenDto>();
			ViewBag.TTList = HelperMvc<ComboGenDto>.ListaGenerica(tTList);
			var usuList = new List<ComboGenDto>();
			ViewBag.UsuList = HelperMvc<ComboGenDto>.ListaGenerica(usuList);
		}

		protected SelectList ComboCTF(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
