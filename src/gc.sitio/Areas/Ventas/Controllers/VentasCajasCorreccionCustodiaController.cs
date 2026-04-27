using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Ventas;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Ventas.Models.VentasCajasCorreccionCustodia;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Ventas.Controllers
{
	[Area("Ventas")]
	public class VentasCajasCorreccionCustodiaController : VentasCajasCorreccionCustodiaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IApiVentasServicio _apiVentasServicio;
		private readonly IAdministracionServicio _administracionServicio;

		public VentasCajasCorreccionCustodiaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<VentasCajasCorreccionCustodiaController> logger,
													   IApiVentasServicio apiVentasServicio, IAdministracionServicio administracionServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_apiVentasServicio = apiVentasServicio;
			_administracionServicio = administracionServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroCtlCustodiaModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CORRECCIÓN DE VALORES ENTREGADOS EN CUSTODIA";
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
		public async Task<IActionResult> CargarDatosDeValores(string admDesc, string admId, string tipo)
		{
			var model = new InitializeViewCusModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(admDesc))
					throw new NegocioException("Faltan datos obligatorios: Sucursal");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlEntregaLista(admId, tipo, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de valores entregados en custodia");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de valores entregados en custodia");
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
				{
					model.GrillaVtasPVCtlEntrega = ObtenerGridCoreSmart<VtasPVCtlEntregaDto>([]);
					VtasPVCtlEntregaLista = [];
				}
				else
				{
					model.GrillaVtasPVCtlEntrega = ObtenerGridCoreSmart<VtasPVCtlEntregaDto>(resultado.ListaEntidad ?? []);
					VtasPVCtlEntregaLista = resultado.ListaEntidad ?? [];
				}
				model.GrillaVtasPVCtlEntregaRend = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>([]);
				model.TipoEntrega = tipo;
				model.Sucursal = admDesc;
				return PartialView("_datos_custodia", model);
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
		public async Task<IActionResult> ObtenerRendDeEntregaSeleccionada(string ent_compte)
		{
			var model = new GridCoreSmart<VtasPVCtlEntregaRendDto>();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(ent_compte))
					throw new NegocioException("Faltan datos obligatorios: Entrega");
				var resultado = await _apiVentasServicio.ObtenerVtasPVCtlEntregaRendLista(ent_compte, TokenCookie);
				if (resultado == null)
					throw new NegocioException("Error al obtener datos de rendición de entrega seleccionada");
				if (!resultado.Ok)
					throw new NegocioException(resultado.Mensaje ?? "Error al obtener datos de rendición de entrega seleccionada");
				if (resultado.ListaEntidad == null || resultado.ListaEntidad.Count == 0)
				{
					model = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>([]);
					VtasPVCtlEntregaRendLista = [];
				}
				else
				{
					var item = VtasPVCtlEntregaLista.Where(x=>x.ent_compte == ent_compte).FirstOrDefault();
					var listaTempo = resultado.ListaEntidad ?? [];
					listaTempo.ForEach(x => x.ent_estado = item?.ent_estado ?? ' ');
					VtasPVCtlEntregaRendLista = listaTempo;
					model = ObtenerGridCoreSmart<VtasPVCtlEntregaRendDto>(listaTempo);
				}

				return PartialView("_datos_correccion_VtasPVCtlEntregaRend", model);
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
		public IActionResult ObtenerEntregasParaCambioDeRendicion(string ent_compte)
		{
			var model = new ListaEntregasModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (string.IsNullOrEmpty(ent_compte))
					throw new NegocioException("Faltan datos obligatorios: Entrega");
				var listaTempo = VtasPVCtlEntregaLista.Where(x => x.ent_compte != ent_compte).ToList();
				if (listaTempo == null || listaTempo.Count == 0)
					model.ListaEntregas = new SelectList(Enumerable.Empty<ComboGenDto>());
				else
					model.ListaEntregas = ObtenerListaEntregas(listaTempo);
				return PartialView("_grid_entregas_para_cambio_rendicion", model);
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

		#region Metodos Privados
		private void CargarDatosIniciales(FiltroCtlCustodiaModel model)
		{
			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
		}
		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		private SelectList ObtenerListaEntregas(List<VtasPVCtlEntregaDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.ent_compte, Descripcion = x.ent_compte });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
