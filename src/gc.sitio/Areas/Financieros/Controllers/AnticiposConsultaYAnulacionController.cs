using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class AnticiposConsultaYAnulacionController : AnticiposConsultaYAnulacionControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoAnticipoEmpleadoServicio _tipoAnticipoEmpleadoServicio;

		public AnticiposConsultaYAnulacionController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnticiposConsultaYAnulacionController> logger,
													IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio,
													IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
													ITipoAnticipoEmpleadoServicio tipoAnticipoEmpleadoServicio) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
			_tipoAnticipoEmpleadoServicio = tipoAnticipoEmpleadoServicio;
		}

		public IActionResult Index()
		{
			var model = new FiltroAnticipoConsYAnuModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CONSULTA Y ANULACIÓN DE ANTICIPOS";
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
		public async Task<IActionResult> BuscarAnticipoFinancierosDeEmpleados(ConsultaAnticipoFinanEmpRequest request, bool buscaNew, string sort = "an_compte", string sortDir = "asc", int pag = 1, bool actualizar = false)
		{
			var model = new AnticipoFinanEmpModel();
			var lista = new List<AnticipoFinanEmpListaDto>();
			MetadataGrid metadata;
			GridCoreSmart<AnticipoFinanEmpListaDto> grillaDatos;
			try
			{
				if (!buscaNew)
				{
					lista = ListaAnticipoFinanEmp.ToList();
					lista = OrdenarEntidad(lista, sortDir, sort);
					ListaAnticipoFinanEmp = lista;
				}
				else
				{
					request.Sort = sort;
					request.SortDir = sortDir;
					request.Registros = _setting.NroRegistrosPagina;
					request.Pagina = pag;

					var res = await _financieroServicio.BuscarAnticipoFinancierosDeEmpleados(request, TokenCookie);
					lista = res.Item1 ?? [];
					MetadataGeneral = res.Item2 ?? new MetadataGrid();
					ListaAnticipoFinanEmp = lista;

				}
				metadata = MetadataAnticipoFinanEmp;
				grillaDatos = GenerarGrillaSmart(ListaAnticipoFinanEmp, sort, _setting.NroRegistrosPagina, pag, MetadataGeneral.TotalCount, MetadataGeneral.TotalPages, sortDir);
				model.GrillaAnticipoFinanEmp = grillaDatos;
				return PartialView("_gridAnticipoFinanEmp", model);
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
		public IActionResult CargarDetalleDeAnticipo(string anCompte)
		{
			var model = new AnticipoFinanEmpDetalleModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var resultado = _financieroServicio.GetAnticipoDetalle(anCompte, TokenCookie);
				if (resultado == null || resultado.Count <= 0)
					return PartialView("_gridAnticipoFinanEmpDetalle", model);

				model.Leyenda = $"Detalle de Cuentas del Anticipo N°: {anCompte}";
				model.GrillaAnticipoFinanEmpDetalle = ObtenerGridCoreSmart<AnticipoDetalleDto>(resultado);
				return PartialView("_gridAnticipoFinanEmpDetalle", model);
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
		public JsonResult BuscarClientes(string prefix)
		{
			var top = ClientesLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})" });
			return Json(tipos);
		}

		#region Métodos privados
		private void CargarDatosIniciales(FiltroAnticipoConsYAnuModel model)
		{
			model.Date1 = DateTime.Today.AddMonths(-1);
			model.Date2 = DateTime.Today;

			if (TipoAnticipoEmpleadoLista.Count == 0)
				ObtenerTiposAnticipoEmpleado(_tipoAnticipoEmpleadoServicio);
			model.ListaTipo = ComboTipos(TipoAnticipoEmpleadoLista);

			if (FinancieroUsuariosLista.Count == 0)
			{
				var usuarios = _financieroServicio.GetFinancieroUsuarios(new GetFinancieroUsuariosRequest()
				{
					desde = model.Date1,
					hasta = model.Date2
				}, TokenCookie);
			}
			model.ListaUsuario = ComboUsuarios(FinancieroUsuariosLista);

			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);
			var UsuList = new List<ComboGenDto>();
			ViewBag.UsuarioList = HelperMvc<ComboGenDto>.ListaGenerica(UsuList);

			if (ClientesLista.Count == 0)
			{
				var lista = _cuentaServicio.ObtenerListaCuentaComercial("%", 'C', TokenCookie).Result;
				ClientesLista = lista;
			}
		}

		protected SelectList ComboTipos(List<TipoAnticipoEmpleadoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ant_id, Descripcion = $"{x.ant_desc} ({x.ant_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		protected SelectList ComboUsuarios(List<FinancieroUsuarioDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre}" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
