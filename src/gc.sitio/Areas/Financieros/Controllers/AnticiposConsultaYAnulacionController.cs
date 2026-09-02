using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.Areas.Mstk.Models;
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
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //ADE
		private AppModulo _modulo_2; //DDA
		private string APP_MODULO_1 = AppModulos.ADE.ToString();
		private string APP_MODULO_2 = AppModulos.DDA.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
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

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_docMSv = docManager; //instancio el servicio de impresión
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
		public JsonResult FinancieroAnticipoAnular(string anCompte)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (string.IsNullOrEmpty(anCompte))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un Anticipo para anular." });

				var request = new FinancieroAnticipoAnularRequest()
				{
					an_compte = anCompte,
					adm_id = AdministracionId,
					usu_id = UserName
				};
				var respuesta = _financieroServicio.FinancieroAnticipoAnular(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "El Anticipo de empleado ha sido anulado con éxito.");
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		/// <summary>
		/// Establece el tipo de reporte seleccionado por el usuario para la consulta anticipos.
		/// Inicializa el gestor de impresión y carga los documentos disponibles según el tipo de reporte.
		/// </summary>
		/// <param name="tipoReporte">Tipo de reporte seleccionado.</param>
		/// <returns>Resultado en formato JSON indicando éxito o error.</returns>
		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.ImprimirVales:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "DETALLE DE ANTICIPO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_1);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_1);
						#endregion
						break;
					case TipoDeReporte.ImprimirDetalle:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "ANTICIPO DE EMPLEADO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);
						#endregion
						break;
					default:
						break;
				}

				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaAnticipoFinanEmp = [];
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (NegocioException ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		[HttpPost]
        public JsonResult BuscarClientes(string prefix)
        {
            var top = ClientesLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
            var tipos = top.Select(x => new
            {
                Id = x.Cta_Id,
                Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})",
                TipoDesc = x.Tipo_Desc,
                Tipo = x.Tipo
            });
            return Json(tipos);
        }

		[HttpPost]
		public IActionResult ObtenerUsuarios(DateTime desde, DateTime hasta)
		{
			try
			{
				var model = new ListaUsuModel();
				var usuarios = _financieroServicio.GetFinancieroUsuarios(new GetFinancieroUsuariosRequest()
				{
					desde = desde,
					hasta = hasta
				}, TokenCookie);
				model.ListaUsuario = ComboUsuarios(usuarios);
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

		enum TipoDeReporte
		{
			ImprimirDetalle = 1,
			ImprimirVales = 2,
		}
		#endregion
	}
}
