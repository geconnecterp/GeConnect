using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.ComprobanteDeCompra;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.ValorizacionDeComprobante;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class ValorizacionDeComprobanteController : ValorizacionDeComprobanteControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;


		public ValorizacionDeComprobanteController(ICuentaServicio cuentaServicio, IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ValorizacionDeComprobanteController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
		}

		public IActionResult Index()
		{
			MetadataGrid metadata;
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				ViewData["Titulo"] = "VALORIZACIÓN DE COMPROBANTE";
				CargarDatosIniciales(true);
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

		[HttpPost]
		public IActionResult CargarComprobantesDelProveedorSeleccionado(string ctaId)
		{
			var model = new ListaComptePendienteDeValorizarModel();
			try
			{
				CtaIdSelected = ctaId;
				CargarComprobantesDelProveedor(ctaId, _cuentaServicio);
				model.LstComptePendiente = ComboComptesPendientes();
				model.cm_compte = string.Empty;
				return PartialView("_listaComptesPendientes", model);
			}
			catch (Exception ex)
			{
				return PartialView("_empty_view");
			}

		}

		[HttpPost]
		public IActionResult CargarDatosParaValorizar(string cm_compte) 
		{
			var model = new TabComprobanteModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}
				if (string.IsNullOrEmpty(cm_compte))
					return PartialView("_tabComprobante", model);

				//Armado de Request, que es común para ambos servicios
				var compteSeleccionado = ComprobantesPendientesDeValorizarLista.Where(x=>x.cm_compte.Equals(cm_compte)).First();
				if (compteSeleccionado == null)
					return PartialView("_tabComprobante", model);

				var req = new CompteValorizaRprDtosRequest()
				{ 
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
				};
				//Cargar Detalle de Productos RPR
				var responseRpr = _cuentaServicio.ObtenerComprobantesDetalleRpr(req, TokenCookie);
				var jsonResponseRpr = JsonConvert.SerializeObject(responseRpr, new JsonSerializerSettings());

				//Cargar Detalle de Descuentos Financieros
				var responseDtos = _cuentaServicio.ObtenerComprobantesDtos(req, TokenCookie);
				var jsonResponseDtos = JsonConvert.SerializeObject(responseDtos, new JsonSerializerSettings());

				//Cargar Datos Valorizados
				var reqValorizados = new CompteValorizaRequest()
				{
					cm_compte = compteSeleccionado.cm_compte,
					cta_id = compteSeleccionado.cta_id,
					dia_movi = compteSeleccionado.dia_movi,
					tco_id = compteSeleccionado.tco_id,
					json_detalle = jsonResponseRpr,
					json_dtos = jsonResponseDtos,
				};
				var responseValorizar=_cuentaServicio.ObtenerComprobanteValorizaLista(reqValorizados, TokenCookie);

				model.GrillaValoracion = ObtenerGridCoreSmart<CompteValorizaListaDto>(responseValorizar);
				model.GrillaDescuentosFin = ObtenerGridCoreSmart<CompteValorizaDtosListaDto>(responseDtos);
				///TODO MARCE: Seguir aca
				return PartialView("_tabComprobante", model);
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
		protected SelectList ComboComptesPendientes()
		{
			var lista = ComprobantesPendientesDeValorizarLista.Select(x => new ComboGenDto { Id = x.cm_compte.ToString(), Descripcion = $"{x.tco_desc} ({x.tco_id}) {x.cm_compte} {x.cm_fecha.ToShortDateString()} {x.cm_total}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
				ObtenerProveedores(_cuentaServicio);
		}
		protected void CargarComprobantesDelProveedor(string ctaId, ICuentaServicio _cuentaServicio)
		{
			var adms = _cuentaServicio.ObtenerComprobantesPendientesDeValorizar(ctaId, TokenCookie);
			ComprobantesPendientesDeValorizarLista = adms;
		}
		#endregion
	}
}
