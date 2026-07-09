using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Almacen.AjusteDeStock;
using gc.infraestructura.Dtos.Deposito;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Consultas.Models.ReporteDeVentas;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Microsoft.Win32;

namespace gc.sitio.Areas.Mstk.Controllers.RegistrarRemitoExterno
{
	[Area("Mstk")]
	public class RegistrarRemitoExternoController : RegistrarRemitoExternoControladorBase
	{
		private const string _tipoOP = "VE";
		private readonly AppSettings _setting;
		private readonly ITipoComprobanteServicio _tipoCompteServicio;
		private readonly IDepositoServicio _depositoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IRemitoServicio _remitoServicio;
		public RegistrarRemitoExternoController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<RegistrarRemitoExternoController> logger,
												ITipoComprobanteServicio tipoComprobanteServicio, IDepositoServicio depositoServicio,
												ICuentaServicio cuentaServicio, IRemitoServicio remitoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_tipoCompteServicio = tipoComprobanteServicio;
			_depositoServicio = depositoServicio;
			_cuentaServicio = cuentaServicio;
			_remitoServicio = remitoServicio;
		}

		public IActionResult Index()
		{
			var model = new InitCargaRegExt();
			List<DepositoInfoBoxDto> boxes = [];
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REMITOS EXTERNOS";
				ViewData["Titulo"] = titulo;

				InicializarDatosDeSession(model);
				model.TipoComprobantes = ComboTipoComprobante("%", _tipoOP);
				model.ComboDepositos = CargarComboDepositos();
				model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
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

		public async Task<IActionResult> ObtenerBoxesDesdeDeposito(string depoId)
		{
			var model = new BoxListDto();
			try
			{
				if (depoId != "0")
					model.ComboBoxes = CargarComboBoxes(depoId);
				else
				{
					List<DepositoInfoBoxDto> boxes = [];
					model.ComboBoxes = HelperMvc<ComboGenDto>.ListaGenerica(boxes.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" }));
				}
				return PartialView("_listaBox", model);
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

		public async Task<JsonResult> VerificarExistenciaDeProductosDesdeComprobantes(RemitoExternoValidaRequest request)
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return Json(redirectResult);
				if (request == null)
					return Json(CrearRespuestaWarning("no se proporcionaron los datos de búsqueda."));
				var lista = await _remitoServicio.CargarProductosDesdeComprobante(request, TokenCookie);
				if (!lista.Ok)
					throw new NegocioException(lista.Mensaje ?? "No se ha podido obtener la lista de productos desde el comprobante.");
				if (lista.ListaEntidad == null || lista.ListaEntidad.Count() == 0)
				{
					_logger?.LogInformation("No se encontraron los datos de productos desde el comprobante");
					return Json(CrearRespuestaWarning("No se encontraron los datos de productos desde el comprobante."));
				}
				lista.ListaEntidad.ForEach(x => x.box_id = request.box_id);
				ListaRemitoExternoValida = lista.ListaEntidad;
				return Json(CrearRespuestaOk("Se encontraron los datos de productos desde el comprobante."));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return Json(CrearRespuestaError("Error"));
			}
		}

		public IActionResult CargarProductosDesdeComprobante()
		{
			try
			{
				if (!VerificarAutenticacion(out IActionResult redirectResult))
					return redirectResult;

				var lista = ListaRemitoExternoValida;

				return PartialView("_partialProdsDelCompte", ObtenerGridCoreSmart<RemitoExternoValidaDto>(lista));
			}
			catch (NegocioException ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaWarning(ex.Message));
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Error");
				return PartialView("_gridMensaje", CrearRespuestaError("Error"));
			}
		}

		#region Metodos Privados
		private void InicializarDatosDeSession(InitCargaRegExt model) 
		{
			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");
		}
		protected SelectList ComboTipoComprobante(string afip_id, string opt_id)
		{
			var listaTemp = _tipoCompteServicio.BuscarTipoComprobanteListaPorTipoAfip(afip_id, opt_id, Token).Result;
			TiposComprobante = listaTemp;
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.tco_id, Descripcion = x.tco_desc });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboDepositos()
		{
			var adms = _depositoServicio.ObtenerDepositosDeAdministracion(AdministracionId, TokenCookie);
			DepositoLista = adms;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Depo_Id, Descripcion = x.Depo_Nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList CargarComboBoxes(string depoId)
		{
			var adms = _depositoServicio.BuscarBoxPorDeposito(depoId, TokenCookie).Result;
			var lista = adms.Select(x => new ComboGenDto { Id = x.Box_Id, Descripcion = $"{x.Box_Id}__{x.Box_desc}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
