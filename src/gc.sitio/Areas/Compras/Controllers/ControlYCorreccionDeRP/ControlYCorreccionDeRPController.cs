using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.ControlYCorreccionDeRP;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Compras.Controllers.ControlYCorreccionDeRP
{
	[Area("Compras")]
	public class ControlYCorreccionDeRPController : ControlYCorreccionDeRPControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IProductoServicio _prdSrv;
		public ControlYCorreccionDeRPController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ControlYCorreccionDeRPController> logger,
												ICuentaServicio cuentaServicio, IProductoServicio prdSrv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_cuentaServicio = cuentaServicio;
			_prdSrv = prdSrv;
		}

		public IActionResult Index()
		{
			var model = new InitModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var titulo = "CONTROL DE RECEPCIÓN DE PROVEEDORES";
				ViewData["Titulo"] = titulo;

				CargarDatosIniciales(true);
				model.ListaRecepciones = HelperMvc<ComboGenDto>.ListaGenerica([]);
				model.RecepcionSeleccionada = string.Empty;
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

		public IActionResult CargarRecepcionesDelProveedor(string cta_id)
		{ 
			var model = new InitRecepcionesModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var recepciones = _prdSrv.ObtenerRprCtlPendiente(cta_id, TokenCookie).Result;
				RprCtlPendienteLista = recepciones;
				model.ListaRecepciones = ComboComptesPendientes();
				return PartialView("_listaRecepciones", model);
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

		public IActionResult ObtenerDetalleDeRecepcion(string rp_compte, string prov, string rp_compte_desc)
		{
			var model = new InitDetalleDeRecepcionModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				var detalle = _prdSrv.ObtenerRprCtlDetalle(rp_compte, TokenCookie).Result;
				RprCtlDetalleLista = detalle;
				model.GrillaDetalle = ObtenerGridCoreSmart<RprCtlItemDetalleDto>(detalle);
				model.Recepcion = rp_compte_desc;
				model.Proveedor = prov;
				return PartialView("_partial_Principal", model);
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
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio, "BI");
			}
		}
		protected SelectList ComboComptesPendientes()
		{
			var lista = RprCtlPendienteLista.Select(x => new ComboGenDto { Id = x.rp_compte.ToString(), Descripcion = $"{x.tco_desc_rp} ({x.tco_id_rp}) {x.cm_compte_rp} {x.cm_fecha_rp.ToShortDateString()} {x.cm_importe_rp}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
