using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers.InformacionDeBox
{
	[Area("Mstk")]
	public class InformacionDeBoxController : InformacionDeBoxControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IDepositoServicio _depositoServicio;
		private readonly IConsultasServicio _consultaServicio;
		private readonly IProductoServicio _productoServicio;
		private readonly IProducto2Servicio _producto2Servicio;
		private readonly ITipoMovStkServicio _tipoMovStkServicio;
		public InformacionDeBoxController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<InformacionDeBoxController> logger,
										  IDepositoServicio depositoServicio, IConsultasServicio consultaServicio, IProductoServicio productoServicio, 
										  IProducto2Servicio producto2Servicio, ITipoMovStkServicio tipoMovStkServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_depositoServicio = depositoServicio;
			_consultaServicio = consultaServicio;
			_productoServicio = productoServicio;
			_producto2Servicio = producto2Servicio;
			_tipoMovStkServicio = tipoMovStkServicio;
		}

		public IActionResult Index()
		{
			var model = new InformacionDeBoxModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "INFORMACIÓN DE BOX";
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

		public IActionResult BuscarInfoBoxes(InformacionDeBoxesListaRequest request)
		{
			var model = new BoxesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var info = _productoServicio.InformacionDeBoxesLista(request, TokenCookie).Result;
				model.GrillaBoxes = ObtenerGridCoreSmart<BoxInfoExtendedDto>(info);
				model.LeyendaDeposito = ConstruirLeyenda("Depósito", request.depo_desc);
				model.LeyendaGondola = ConstruirLeyenda("Góndola", request.box_gondola);
				model.LeyendaNivel = ConstruirLeyenda("Nivel", request.box_nivel);
				model.LeyendaRack = ConstruirLeyenda("Rack", request.box_rack);
				model.LeyendaZona = ConstruirLeyenda("Zona", request.box_zona);
				model.LeyendaSoloLibres = ConstruirLeyendaLibres("Solo Libres", request.boxe_id);
				model.ListaTipoMovimientos = ComboTipoMovStk();
				// Leyenda final
				var partesLeyenda = new List<string>();

				if (!string.IsNullOrWhiteSpace(model.LeyendaDeposito))
					partesLeyenda.Add(model.LeyendaDeposito);

				if (!string.IsNullOrWhiteSpace(model.LeyendaGondola))
					partesLeyenda.Add(model.LeyendaGondola);

				if (!string.IsNullOrWhiteSpace(model.LeyendaNivel))
					partesLeyenda.Add(model.LeyendaNivel);

				if (!string.IsNullOrWhiteSpace(model.LeyendaRack))
					partesLeyenda.Add(model.LeyendaRack);

				if (!string.IsNullOrWhiteSpace(model.LeyendaZona))
					partesLeyenda.Add(model.LeyendaZona);

				if (!string.IsNullOrWhiteSpace(model.LeyendaSoloLibres))
					partesLeyenda.Add(model.LeyendaSoloLibres);

				return PartialView("_partialBoxes", model);

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
		public async Task<IActionResult> ObtenerBoxInfoStk(string boxId)
		{
			RespuestaGenerica<EntidadBase> response = new();
			GridCoreSmart<BoxInfoStkDto> grillaDatos;
			try
			{
				if (string.IsNullOrEmpty(boxId) || string.IsNullOrWhiteSpace(boxId))
				{
					throw new NegocioException("No se recepcionó el Box. Verifique");
				}
				var res = await _producto2Servicio.ObtenerBoxInfoStk(boxId, TokenCookie);
				if (res.Ok)
				{
					grillaDatos = ObtenerGridCoreSmart<BoxInfoStkDto>(res.ListaEntidad);
				}
				else
				{
					throw new NegocioException(res.Mensaje ?? "Hubo un problema al obtener info de Stk en el Box");
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
			catch (UnauthorizedException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
			return PartialView("_gridBoxInfoStk", grillaDatos);
		}

		[HttpPost]
		public async Task<IActionResult> ObtenerBoxInfoMovStk(string boxId, string sm, DateTime desde, DateTime hasta)
		{
			RespuestaGenerica<EntidadBase> response = new();
			GridCoreSmart<BoxInfoMovStkDto> grillaDatos;
			try
			{
				if (string.IsNullOrEmpty(boxId) || string.IsNullOrWhiteSpace(boxId))
				{
					throw new NegocioException("No se recepcionó el Box. Verifique");
				}

				//sm = sm ?? "%";

				var res = await _producto2Servicio.ObtenerBoxInfoMovStk(boxId, sm, desde, hasta, TokenCookie);
				if (res.Ok)
				{
					grillaDatos = ObtenerGridCoreSmart<BoxInfoMovStkDto>(res.ListaEntidad);
				}
				else
				{
					throw new NegocioException(res.Mensaje ?? "Hubo un problema para obtener la información de los Movimientos de Productos.");
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
			catch (UnauthorizedException ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = true;
				response.EsError = false;
				return PartialView("_gridMensaje", response);
			}
			catch (Exception ex)
			{
				response.Mensaje = ex.Message;
				response.Ok = false;
				response.EsWarn = false;
				response.EsError = true;
				return PartialView("_gridMensaje", response);
			}
			return PartialView("_gridBoxInfoMovStk", grillaDatos);
		}

		#region METODOS PRIVADOS
		private static string ConstruirLeyendaLibres(string titulo, string textos)
		{
			// Caso normal
			if (!string.IsNullOrWhiteSpace(textos))
			{
				var txt = textos.Trim() == "%" ? "No" : "Si";
				return $"{titulo}: {txt}";
			}
			return string.Empty;
		}
		private static string ConstruirLeyenda(string titulo, string textos)
		{
			// Caso normal
			if (!string.IsNullOrWhiteSpace(textos))
			{
				var txt = textos.Trim() == "%" ? "Todos" : textos.Trim();
				return $"{titulo}: {txt}";
			}
			return string.Empty;
		}
		private void CargarDatosIniciales(InformacionDeBoxModel model)
		{
			var depositos = _depositoServicio.ObtenerDepositosDeAdministracion("%", TokenCookie);
			if (depositos != null && depositos.Count > 0)
				model.ListaDepositos = ComboDepositos(depositos);
			else
				model.ListaDepositos = HelperMvc<ComboGenDto>.ListaGenerica([]);
			
		}
		private SelectList ComboTipoMovStk()
		{
			var tm = _tipoMovStkServicio.ObtenerTiposDeMovimientosDeStock(TokenCookie);
			var lista = tm.Select(x => new ComboGenDto { Id = x.sm_tipo, Descripcion = x.sm_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ComboDepositos(List<DepositoDto> depos)
		{
			var lista = depos.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
