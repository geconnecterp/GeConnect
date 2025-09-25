using gc.api.core.Entidades;
using gc.api.core.Entidades.Tipos;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Tipos;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Users.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class ChequePropioEmitidoController : ChequePropioEmitidoControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.CPE.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IUserServicio _userServicio;
		public ChequePropioEmitidoController(IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio,
							   IDocManagerServicio docManager, IOptions<DocsManager> docsManager, IUserServicio userServicio,
							   IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<ChequePropioEmitidoController> logger) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
			_userServicio = userServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: TEC
			_docMSv = docManager; //instancio el servicio de impresión
		}
		public IActionResult Index()
		{
			var model = new FiltrosChequePropioEmitidoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CHEQUES PROPIOS EMITIDOS";
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

		public IActionResult BuscarChequesEmitidosPropios(FinancieroBcoVencChequeEmitidoListaRequest request)
		{
			var model = new ChequePropioEmitidoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var res = _financieroServicio.GetFinancieroBcoVencChequeEmitidoLista(request, TokenCookie);
				if (res == null || res.Count < 0)
					return PartialView("_partialChequePropioEmitidoLista", model);

				model.GrillaChequesDetalle  = ObtenerGridCoreSmart<FinancieroChequePropioEmitidoListaDto>(MapListaToChequePropio(res));
				return PartialView("_partialChequePropioEmitidoLista", model);
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

		public static List<FinancieroChequePropioEmitidoListaDto> MapListaToChequePropio(List<FinancieroBcoVencChequeEmitidoListaDto> sourceList)
		{
			return sourceList.Select(item => new FinancieroChequePropioEmitidoListaDto
			{
				ctaf_id = item.ctaf_id,
				ctaf_denominacion = item.ctaf_denominacion,
				che_emision = item.che_emision,
				che_nro = item.che_nro,
				che_fecha = item.che_fecha,
				che_anombre = item.che_anombre,
				che_importe = item.che_importe,
				che_estado = item.che_estado,
				che_estado_desc = item.che_estado_desc,
				usu_id = item.usu_id,
				che_fecha_emi = item.che_fecha_emi,
				che_impreso = item.che_impreso,
				che_op_tra = item.che_op_tra,
				op_compte = item.op_compte,
				cta_id = item.cta_id,
				cta_denominacion = item.cta_denominacion,
				ent_fecha = item.ent_fecha,
				ent_usu_id = item.ent_usu_id,
				che_auto = item.che_auto,
				modificado = item.modificado,
				dif_print = item.dif_print,
				cf_conciliado = item.cf_conciliado,
				diferido = item.diferido
			}).ToList();
		}


		#region Métodos privados
		private void CargarDatosIniciales(FiltrosChequePropioEmitidoModel model)
		{
			model.Date1 = DateTime.Today.AddMonths(-1);
			model.Date2 = DateTime.Today;
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.ListaCuentaBanco = ComboCTF(ctfLista);
			var usuLista = _userServicio.ObtenerUsuarioParaLista(new BuscarUsuarioRequest()
			{
				id = false,
				id_d = "aaaaaaaaaa",
				id_h = "zzzzzzzzzz",
				deno = false,
				deno_like = "%",
				registros = 1000,
				pagina = 1,
				ordenar = "usu_apellidoynombre"
			}, TokenCookie);
			model.ListaUsuarios = ComboUsu(usuLista);

			var estLista = _financieroServicio.GetChequesEmitidosEstadosLista(TokenCookie);
			model.ListaEstados = ComboEst(estLista);

			var CBList = new List<ComboGenDto>();
			ViewBag.CBList = HelperMvc<ComboGenDto>.ListaGenerica(CBList);
			var UsuList = new List<ComboGenDto>();
			ViewBag.UsuList = HelperMvc<ComboGenDto>.ListaGenerica(UsuList);
			var EstList = new List<ComboGenDto>();
			ViewBag.EstList = HelperMvc<ComboGenDto>.ListaGenerica(EstList);
			var Rel01List = new List<ComboGenDto>();
			ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(Rel01List);

			if (ProveedoresLista.Count == 0)
				ObtenerProveedores(_cuentaServicio);
		}

		protected SelectList ComboCTF(List<FinancieroDesdeSeleccionDeTipoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}

		protected SelectList ComboUsu(List<UserDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = $"{x.usu_apellidoynombre} ({x.usu_id})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		protected SelectList ComboEst(List<ChequeEmitidoEstadoDto> listaTemp)
		{
			var lista = listaTemp.Select(x => new ComboGenDto { Id = x.che_estado, Descripcion = $"{x.che_estado_desc} ({x.che_estado})" });
			return HelperMvc<ComboGenDto>.ListaGenerica(lista);
		}
		#endregion
	}
}
