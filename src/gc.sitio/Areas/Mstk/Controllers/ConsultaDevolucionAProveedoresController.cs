using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor;
using gc.infraestructura.Dtos.Almacen.DevolucionAProveedor.Request;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia;
using gc.infraestructura.Dtos.Almacen.Tr.Transferencia.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Mstk;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Mstk.Models;
using gc.sitio.Areas.Mstk.Models.ConsultaDeRecepcionDeProveedores;
using gc.sitio.Areas.Mstk.Models.ConsultaDeTransfInternaDeStock;
using gc.sitio.Areas.Mstk.Models.ConsultaDevolucionAProveedores;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Mstk.Controllers
{
	[Area("Mstk")]
	public class ConsultaDevolucionAProveedoresController : ConsultaDevolucionAProveedoresControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IAdministracionServicio _administracionServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IProductoServicio _productoServicio;
		public ConsultaDevolucionAProveedoresController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<ConsultaDevolucionAProveedoresController> logger, 
														IAdministracionServicio administracionServicio, ICuentaServicio cuentaServicio, IProductoServicio productoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_administracionServicio = administracionServicio;
			_cuentaServicio = cuentaServicio;
			_productoServicio = productoServicio;
		}

		public IActionResult Index()
		{
			var model = new ConsultaDevolucionAProveedoresModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "REPORTE DE DEVOLUCIÓN A PROVEEDORES";
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
		public IActionResult InicializarPantallPrincipal(string sucursalesText, string provText, DateTime f_desde, DateTime f_hasta)
		{
			var model = new PrincipalConsultaDeTransfInternaDeStockModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.Sucursales = sucursalesText;
				model.Proveedores = provText;
				model.Desde = f_desde;
				model.Hasta = f_hasta;
				return PartialView("_pantallaPrincipal", model);
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
		public async Task<IActionResult> CargarDevoluciones(string sucIdsList, string provIdsList, DateTime f_desde, DateTime f_hasta, bool buscaNew, string sort = "p_id", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new DevolucionesModel();
			var lista = new List<DevolucionProveedoresListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<DevolucionProveedoresListaDto> grillaDatos;

			try
			{
				if (!buscaNew)
				{
					lista = ListaDevoluciones.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaDevoluciones = lista;
				}
				else
				{
					var request = new CargarDevolucionesRequest
					{
						fecha_d = f_desde,
						fecha_h = f_hasta,
						adm_list = sucIdsList,
						cta_list = provIdsList,
						Sort = sort,
						SortDir = sortDir,
						Registros = _setting.NroRegistrosPagina
					};
					//request.Pagina = pag;

					var res = await _productoServicio.DevolucionAProveedoresLista(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaDevoluciones = lista;

				}
				metadata = MetadataListaDevoluciones;
				grillaDatos = GenerarGrillaSmart(ListaDevoluciones, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaDevoluciones = grillaDatos;
				return PartialView("_grillaDevoluciones", model);
				
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

		#region Metodos privados
		private void CargarDatosIniciales(ConsultaDevolucionAProveedoresModel model)
		{
			var hoy = DateTime.Today;
			model.Desde = new DateTime(hoy.Year, hoy.Month, 1);
			model.Hasta = DateTime.Today;

			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio, "BI");

			var sucursales = _administracionServicio.ObtenerAdministraciones("S", TokenCookie);
			if (sucursales != null && sucursales.Count > 0)
				model.ListaSucursales = ObtenerLista(sucursales);
			else
				model.ListaSucursales = HelperMvc<ComboGenDto>.ListaGenerica([]);
			ViewBag.SucursalesList = HelperMvc<ComboGenDto>.ListaGenerica([]);
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
		}

		private SelectList ObtenerLista(List<AdministracionDto> adms)
		{
			var lista = adms.Select(x => new ComboGenDto { Id = x.Adm_id, Descripcion = x.Adm_nombre });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
