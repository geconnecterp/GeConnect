using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen.Request;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Twilio.TwiML.Voice;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class AnticiposCargaController : AnticiposCargaControladorBase
	{
		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ICuentaServicio _cuentaServicio;
		private readonly ITipoAnticipoEmpleadoServicio _tipoAnticipoEmpleadoServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.ADE.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		public AnticiposCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnticiposCargaController> logger,
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
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: ADE
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new CargaDeAnticiposModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGA DE ANTICIPOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				CargarDatosIniciales(true);

				model.ListaTipo = ComboTipoAnticipoEmpleados();
				model.Concepto = string.Empty;
				model.porc_interes = 0;
				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>([]);

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

		public IActionResult AbrirModalAgregarAnticipo(decimal intereses, string cta_id, string cta_desc)
		{
			var model = new CargaAnticipoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.cuotas = 0;
				model.importe = 0;
				model.intereses = intereses;
				model.cta_id = cta_id;
				model.cta_desc = cta_desc;

				return PartialView("_modalCargaAnticipo", model);
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

		public JsonResult AgregarAnticipo(string cta_id, string cta_desc, int cuotas, decimal importe, int intereses)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (string.IsNullOrEmpty(cta_id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un cliente." });
				if (importe <= 0)
					return Json(new { error = true, warn = false, msg = "El importe debe ser mayor a cero." });
				if (cuotas <= 0)
					return Json(new { error = true, warn = false, msg = "La cuota debe ser mayor a 0." });
				if (AnticiposLista.Any() && AnticiposLista.Where(x => x.cta_id == cta_id).Any())
					return Json(new { error = true, warn = false, msg = "El cliente ya se encuentra en la lista de anticipos." });

				var tope = 0.00M;
				var tope_original = 0.00M;
				var topeCtaLista = _financieroServicio.GetFinancieroTopePorCuenta(cta_id, TokenCookie);
				if (topeCtaLista == null || topeCtaLista.Count == 0)
				{
					tope = 0;
					tope_original = 0;
				}
				else
				{
					tope = topeCtaLista[0].saldo ?? 0.00M;
					tope_original = topeCtaLista[0].saldo ?? 0.00M;
				}

				var listaTemp = AnticiposLista;
				var nuevoAnticipo = new AnticipoDto
				{
					id = Guid.NewGuid().ToString(),
					cta_id = cta_id,
					cta_denominacion = cta_desc,
					importe = importe,
					cuotas = cuotas,
					intereses = Math.Round((importe * intereses) / 100, 2),
					valor_cuota = (importe + Math.Round((importe * intereses) / 100, 2)) / cuotas,
					valor_total = (importe + Math.Round((importe * intereses) / 100, 2)),
					tope = tope,
					tope_original = tope_original,
					mostrar_alerta = ((importe + Math.Round((importe * intereses) / 100, 2)) > tope)
				};

				listaTemp.Add(nuevoAnticipo);
				AnticiposLista = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public JsonResult EliminarItemAnticipo(string id)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (string.IsNullOrEmpty(id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un elemento." });

				var listaTemp = AnticiposLista;
				listaTemp = [.. listaTemp.Where(x => x.id != id)];
				AnticiposLista = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		[HttpPost]
		public IActionResult ActualizarListaDeAnticipos()
		{
			var model = new CargaDeAnticiposModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>(AnticiposLista);
				return PartialView("_grillaAnticipos", model);
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

		public IActionResult CancelarAnticipos()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				AnticiposLista = [];
				return RedirectToAction("Index");
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

		public JsonResult ActualizarInteresDeAnticipos(int nuevo_interes)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (nuevo_interes <= 0)
					return Json(new { error = true, warn = false, msg = "El interes debe ser mayor a 0." });

				var listaTemp = AnticiposLista;
				foreach (var item in listaTemp)
				{
					item.intereses = Math.Round((item.importe * nuevo_interes) / 100, 2);
					item.valor_cuota = (item.importe + Math.Round((item.importe * nuevo_interes) / 100, 2)) / item.cuotas;
					item.valor_total = (item.importe + Math.Round((item.importe * nuevo_interes) / 100, 2));
					item.mostrar_alerta = (item.valor_total > item.tope);
				}
				AnticiposLista = listaTemp;
				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		public JsonResult ConfirmarCargaDeAnticipo(CargaAnticipoEmpleadoRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (request == null)
					return Json(new { error = true, warn = false, msg = "No se recibieron datos para procesar." });
				if (string.IsNullOrEmpty(request.cta_id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar una cuenta de contrapartida." });
				if (string.IsNullOrEmpty(request.an_concepto))
					return Json(new { error = true, warn = false, msg = "Debe ingresar un concepto para el anticipo." });
				if (string.IsNullOrEmpty(request.ant_id))
					return Json(new { error = true, warn = false, msg = "Debe seleccionar un tipo de anticipo." });
				if (request.an_porc_interes < 0)
					return Json(new { error = true, warn = false, msg = "El porcentaje de interés no puede ser menor a cero." });
				if (AnticiposLista == null || AnticiposLista.Count == 0)
					return Json(new { error = true, warn = false, msg = "No se han agregado anticipos para procesar." });

				request.adm_id = AdministracionId;
				request.usu_id = UserName;
				var listaAnticipos = new List<AnticipoParaCarga>();
				listaAnticipos = [.. AnticiposLista.Select(x => new AnticipoParaCarga
				{
					cta_id = x.cta_id,
					cuota = x.cuotas,
					importe = x.importe,
					interes = x.intereses,
					iva_alicuota = x.valor_cuota,
					importe_total = x.valor_total,
					vencimiento = DateTime.Today,
					tope = x.tope
				})];

				request.json_anticipos = JsonConvert.SerializeObject(listaAnticipos);
				request.json_anticipos = request.json_anticipos.Replace("-03:00", "");

				var respuesta = _financieroServicio.FinancieroAnticipoEmpleadoConfirma(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "El Anticipo de empleado se ha cargado con éxtio.");
			}
			catch (Exception ex)
			{
				return Json(new { error = true, warn = false, msg = ex.InnerException });
			}
		}

		[HttpPost]
		public JsonResult BuscarContrapartidas(string prefix)
		{
			var top = ProveedoresLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})" });
			return Json(tipos);
		}

		[HttpPost]
		public JsonResult BuscarClientes(string prefix)
		{
			var top = ClientesLista.Where(x => x.Cta_Denominacion.ToUpperInvariant().Contains(prefix.ToUpperInvariant()));
			var tipos = top.Select(x => new ComboGenDto { Id = x.Cta_Id, Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})" });
			return Json(tipos);
		}

		#region Clases auxiliares
		public class AnticipoParaCarga
		{
			public string cta_id { get; set; } = string.Empty;
			public int cuota { get; set; } = 0;
			public decimal importe { get; set; } = 0.00M;
			public decimal interes { get; set; } = 0.00M;
			public decimal iva_alicuota { get; set; } = 0.00M;
			public decimal importe_total { get; set; } = 0.00M;
			public DateTime vencimiento { get; set; } = DateTime.Today;
			public decimal tope { get; set; }
		}
		#endregion

		#region Métodos privados
		private void CargarDatosIniciales(bool actualizar)
		{

			if (ProveedoresLista.Count == 0)
			{
				var lista = _cuentaServicio.ObtenerListaCuentaComercial("%", 'S', TokenCookie).Result;
				ProveedoresLista = lista;
			}

			if (TipoAnticipoEmpleadoLista.Count == 0)
				ObtenerTiposAnticipoEmpleado(_tipoAnticipoEmpleadoServicio);

			if (ClientesLista.Count == 0)
			{
				var lista = _cuentaServicio.ObtenerListaCuentaComercial("%", 'C', TokenCookie).Result;
				ClientesLista = lista;
			}
		}
		#endregion
	}
}
