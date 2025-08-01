using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.OrdenDePago.Request;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Models;
using gc.sitio.Areas.Compras.Models.OrdenDePagoConsulta;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;

namespace gc.sitio.Areas.Compras.Controllers
{
	[Area("Compras")]
	public class OrdenDePagoConsultaController : OrdenDePagoConsultaControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private AppModulo _modulo_2; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.COP.ToString();
		private string APP_MODULO_2 = AppModulos.OPP.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _settings;
		private readonly ITipoOrdenDePagoServicio _tipoOrdenDePagoServicio;
		private readonly IOrdenDePagoServicio _ordenDePagoServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly IConsultasServicio _consultasServicio;
		public OrdenDePagoConsultaController(ITipoOrdenDePagoServicio tipoOrdenDePagoServicio, IOrdenDePagoServicio ordenDePagoServicio, ICuentaServicio cuentaServicio,
											 IConsultasServicio consultasServicio,
											 IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<OrdenDePagoConsultaController> logger,
											 IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, accessor, logger)
		{
			_settings = options.Value;
			_tipoOrdenDePagoServicio = tipoOrdenDePagoServicio;
			_ordenDePagoServicio = ordenDePagoServicio;
			_cuentaServicio = cuentaServicio;
			_consultasServicio = consultasServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: COP
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2); //identifico los datos del modulo que necesito: COP
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				string titulo = "CONSULTA DE ORDENES DE PAGO";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);

				#endregion

				var listR01 = new List<ComboGenDto>();
				ViewBag.Rel01List = HelperMvc<ComboGenDto>.ListaGenerica(listR01);

				var listR02 = new List<ComboGenDto>();
				ViewBag.Rel02List = HelperMvc<ComboGenDto>.ListaGenerica(listR02);

				var listR03 = new List<ComboGenDto>();
				ViewBag.Rel03List = HelperMvc<ComboGenDto>.ListaGenerica(listR03);

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

		public async Task<IActionResult> BuscarOrdenesDePago(BuscarOrdenesDePagoRequest request)
		{
			MetadataGrid metadata;
			GridCoreSmart<OrdenDePagoConsultaDto> grillaDatos;
			ConsultaOPModel model = new();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
				{
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				}

				request.Registros = _settings.NroRegistrosPagina;
				var productos = _ordenDePagoServicio.CargarOrdenDePagoConsultaLista(request, TokenCookie).Result;
				MetadataGeneral = productos.Item2 ?? new MetadataGrid();
				metadata = MetadataGeneral;

				var pag = request.Pagina == null ? 1 : request.Pagina.Value;
				ListaOrdenDePagoConsulta = productos.Item1;
				grillaDatos = GenerarGrillaSmart(productos.Item1, request.Sort, _settings.NroRegistrosPagina, pag, metadata.TotalCount, metadata.TotalPages, request.SortDir);
				model.GrillaOP = grillaDatos;
				model.Importe = ListaOrdenDePagoConsulta.Count > 0 ? ListaOrdenDePagoConsulta.Sum(x => x.op_importe) : 0;
				model.ListaTipoCertificado = new SelectList(ListaTipoCertificado, "id", "descripcion");
				model.MostrarListaTipoCertificado = false;
				return PartialView("_tabOrdenesDePago", model);
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
		public JsonResult ConsultarExistenciaDeCertificados(string op_compte)
		{
			try
			{
				if (ListaOrdenDePagoConsulta == null || ListaOrdenDePagoConsulta.Count <= 0)
					return Json(new { error = true, warn = false, msg = $"No existen Ordenes de Pago para el filtro seleccionado." });
				var opLista = ListaOrdenDePagoConsulta.Where(x => x.op_compte.Equals(op_compte));
				if (opLista != null && opLista.Count() >= 0)
				{
					var op = opLista.First();
					var tieneCertificados = op.certificado_ga || op.certificado_iva || op.certificado_ib;
					return Json(new { error = false, warn = false, msg = "", tieneCertificados });
				}
				else
					return Json(new { error = true, warn = false, msg = $"No existen la Ordene de Pago seleccionada ({op_compte})." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar consultar existencia de certificados." });
			}
		}

		[HttpPost]
		public IActionResult CargarListaTiposCertificados(string op_compte)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var model = new ListaTipoCertificadoModel();
				var lista = ListaTipoCertificado;
				var op = ListaOrdenDePagoConsulta.Where(x => x.op_compte.Equals(op_compte)).First();
				if (op.certificado_ga)
					lista.Add(new TipoCertificadoModel { id = "GA", descripcion = "Certificado de Ganancias" });
				if (op.certificado_iva)
					lista.Add(new TipoCertificadoModel { id = "IVA", descripcion = "Certificado de IVA" });
				if (op.certificado_ib)
					lista.Add(new TipoCertificadoModel { id = "IB", descripcion = "Certificado de Ingresos Brutos" });
				model.ListaTipoCertificado = new SelectList(lista, "id", "descripcion");
				return PartialView("_listaTipoCertificado", model);
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
		public JsonResult AnularOrdenDePago(string op_compte)
		{
			try
			{
				if (string.IsNullOrEmpty(op_compte))
					return Json(new { error = true, warn = false, msg = $"Debe seleccionar una Orden de Pago." });

				var respuesta = _ordenDePagoServicio.AnularOrdenDePago(new AnularOrdenDePagoRequest() { op_compte = op_compte, adm_id = AdministracionId, usu_id = UserName }, TokenCookie);
				if (respuesta == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar anular la Orden de Pago ({op_compte})." });
				if (respuesta.Entidad == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar anular la Orden de Pago ({op_compte})." });
				if (respuesta.Entidad.resultado > 0)
					return Json(new { error = true, warn = false, msg = respuesta.Entidad.resultado_msj });
				return Json(new { error = false, warn = false, msg = $"Se ha anulado la Orden de Pago ({op_compte})" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar anular la Orden de Pago ({op_compte})." });
			}
		}

		[HttpPost]
		public JsonResult AnularCertificadoDeOrdenDePago(string op_compte, string imp_id)
		{
			try
			{
				if (string.IsNullOrEmpty(op_compte))
					return Json(new { error = true, warn = false, msg = $"Debe seleccionar una Orden de Pago." });

				if (string.IsNullOrEmpty(imp_id))
					return Json(new { error = true, warn = false, msg = $"Debe seleccionar un Tipo de Certificado." });

				var respuesta = _ordenDePagoServicio.AnularCertificadoDeOrdenDePago(new AnularCertificadoDeOrdenDePagoRequest() { op_compte = op_compte, imp_id = imp_id, adm_id = AdministracionId, usu_id = UserName }, TokenCookie);
				//var respuesta = ObtenerRespuestaGenericaMock(TipoDeRespuestaMock.EsOk, "", 0);
				if (respuesta == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar anular el Certificado de la Orden de Pago ({op_compte})." });
				if (respuesta.Entidad == null)
					return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar anular el Certificado de la Orden de Pago ({op_compte})." });
				if (respuesta.Entidad.resultado > 0)
					return Json(new { error = true, warn = false, msg = respuesta.Entidad.resultado_msj });

				var listaTemp = ListaOrdenDePagoConsulta;
				var item = listaTemp.Where(x => x.op_compte.Equals(op_compte)).First();
				if (item != null)
				{
					switch (imp_id)
					{
						case "GA":
							item.certificado_ga = false;
							break;
						case "IV":
							item.certificado_iva = false;
							break;
						case "IB":
							item.certificado_ib = false;
							break;
						default:
							break;
					}
					ListaOrdenDePagoConsulta = listaTemp;
				}
				return Json(new { error = false, warn = false, msg = $"Se ha anulado el Certificado de la Orden de Pago ({op_compte})" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar anular el Certificado de la Orden de Pago ({op_compte})." });
			}
		}

		[HttpPost]
		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaOrdenDePagoConsulta = [];

				return Json(new { error = false, warn = false, msg = "Inicializacion correcta." });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar inicializar los datos en Sesion - ORDENDEPAGO" });
			}
		}

		[HttpPost]
		public JsonResult BuscarTipos(string prefix)
		{
			var adms = TipoOrdenDePagoLista.Where(x => x.opt_lista.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = adms.Select(x => new ComboGenDto { Id = x.opt_id, Descripcion = x.opt_lista });
			return Json(lista);
		}

		[HttpPost]
		public JsonResult BuscarUsuarios(string prefix)
		{
			var adms = ListaOPUsuarios.Where(x => x.usu_apellidoynombre.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var lista = adms.Select(x => new ComboGenDto { Id = x.usu_id, Descripcion = x.usu_apellidoynombre });
			return Json(lista);
		}

		[HttpPost]
		public JsonResult ActualizarListaDeUsuariosOP(DateTime f_desde, DateTime f_hasta)
		{
			try
			{
				ObtenerOPUSuarios(_ordenDePagoServicio, f_desde, f_hasta);
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar actualizar los datos de los usuarios de Ordenes de Pagos." });
			}

		}

		[HttpPost]
		public IActionResult ActualizarListaDeUsuarios(DateTime f_desde, DateTime f_hasta)
		{
			try
			{
				var model = new Ls02Model();
				var lista = _ordenDePagoServicio.ObtenerOPUsuarios(f_desde, f_hasta, TokenCookie);
				model.id = "";
				model.ListaLs02 = new SelectList(lista, "usu_id", "usu_apellidoynombre");
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

		[HttpPost]
		public IActionResult ActualizarListaDeTipos()
		{
			try
			{
				var model = new Ls03Model
				{
					id = "",
					ListaLs03 = ComboTipoDeOrdenDePago(TipoDeOrdenDePago.Otros)
				};
				return PartialView("_listaTipos", model);
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

		/// <summary>
		/// Establece el tipo de reporte seleccionado por el usuario para la consulta de órdenes de pago.
		/// Inicializa el gestor de impresión y carga los documentos disponibles según el tipo de reporte.
		/// </summary>
		public JsonResult SetearTipoDeReporte(int tipoReporte)
		{
			try
			{
				if (tipoReporte < 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de reporte." });

				string titulo = "CONSULTA DE ORDENES DE PAGO";
				//Seteo el tipo de reporte en la sesion
				if (tipoReporte == 1)
				{
					#region Gestor Impresion - Inicializacion de variables
					//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
					DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
					// en este mismo acto se cargan los posibles documentos
					//que se pueden imprimir, exportar, enviar por email o whatsapp
					ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

					#endregion
				}
				else
				{
					#region Gestor Impresion - Inicializacion de variables
					//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
					DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_2);
					// en este mismo acto se cargan los posibles documentos
					//que se pueden imprimir, exportar, enviar por email o whatsapp
					ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_2);

					#endregion
				}
				return Json(new { error = false, warn = false, msg = "Tipo de reporte actualizado correctamente." });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = $"Se prudujo un error al intentar setear el tipo de reporte: {ex.Message}" });
			}
		}

		public JsonResult ValidarExistenciaDeCertificadosParaImprimir(string opCompteLista)
		{
			if (string.IsNullOrEmpty(opCompteLista))
				return Json(new { error = true, warn = false, msg = $"Request vacío, por favor revise." });

			try
			{
				var boolExisteIIBB = false;
				var boolExisteIVA = false;
				var boolExisteGAN = false;
				var existeIIBB = _consultasServicio.ConsultaCertRetenIBFromList(opCompteLista, TokenCookie);
				var existeIVA = _consultasServicio.ConsultaCertRetenIVAFromList(opCompteLista, TokenCookie);
				var existeGanancias = _consultasServicio.ConsultaCertRetenGAFromList(opCompteLista, TokenCookie);

				if (existeIIBB != null && existeIIBB.Count > 0)
					boolExisteIIBB = true;
				if (existeIVA != null && existeIVA.Count > 0)
					boolExisteIVA = true;
				if (existeGanancias != null && existeGanancias.Count > 0)
					boolExisteGAN = true;

				return Json(new { error = false, warn = false, msg = "", imprimeIIBB = boolExisteIIBB, imprimeIVA = boolExisteIVA, imprimeGAN = boolExisteGAN });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = "Se ha producido un error al intentar validar certificados de retencion para imprimir." });
			}
		}


		#region Métodos Privados
		private void CargarDatosIniciales(bool actualizar)
		{
			if (ProveedoresLista.Count == 0 || actualizar)
			{
				ObtenerProveedores(_cuentaServicio, "%");
			}
			if (TipoOrdenDePagoLista.Count == 0 && actualizar)
			{
				ObtenerTiposDeOrdenDePago(_tipoOrdenDePagoServicio);
			}
			if (ListaOPUsuarios.Count == 0 && actualizar)
			{
				ObtenerOPUSuarios(_ordenDePagoServicio, DateTime.Now.AddMonths(-1), DateTime.Now);
			}
		}
		protected void ObtenerOPUSuarios(IOrdenDePagoServicio _opSv, DateTime f_desde, DateTime f_hasta)
		{
			ListaOPUsuarios = _opSv.ObtenerOPUsuarios(f_desde, f_hasta, TokenCookie);
		}
		#endregion
	}
}
