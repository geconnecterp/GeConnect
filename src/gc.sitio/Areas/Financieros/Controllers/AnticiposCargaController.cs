using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
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
		private readonly ICuentaServicio _ctaSrv;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.ADE.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		public AnticiposCargaController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<AnticiposCargaController> logger,
										IFinancieroServicio financieroServicio, ICuentaServicio cuentaServicio,
										IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
										ITipoAnticipoEmpleadoServicio tipoAnticipoEmpleadoServicio, ICuentaServicio ctaSrv) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_cuentaServicio = cuentaServicio;
			_tipoAnticipoEmpleadoServicio = tipoAnticipoEmpleadoServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: ADE
			_docMSv = docManager; //instancio el servicio de impresión
			_ctaSrv = ctaSrv;
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
				AnticiposLista = [];
				CargarDatosIniciales(true);

				model.ListaTipo = ComboTipoAnticipoEmpleados();
				model.Concepto = string.Empty;
				model.porc_interes = 0;
				model.GrillaAnticipos = ObtenerGridCoreSmart<AnticipoDto>([]);
				model.prov_id_selected = ProveedorDefault.Cta_Id;
				model.prov_denominacion_selected = ProveedorDefault.Cta_Denominacion;
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

				//var tempList = 
				//Cargar cuyentas de empleados 
				var ctasEmpleados = _ctaSrv.ObtenerListaCuentaComercial("%", 'E', TokenCookie).Result;
				model.CuentaEmpleados = ObtenerGridCoreSmart<CuentaDto>(ctasEmpleados);
				model.CuentaEmpleadosParaCargar = ObtenerGridCoreSmart<CuentaDto>(new List<CuentaDto>());
				model.cuotasModalCarga = 1;
				model.importeModalCarga = 0;
				model.interesModalCarga = 0;
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

		public class CuentaSeleccionada
		{
			public string id { get; set; } = string.Empty;
			public string descripcion { get; set; } = string.Empty;
		}

		public JsonResult AgregarAnticipo(List<CuentaSeleccionada> empleadosSeleccionados, int cuotas, decimal importe, decimal intereses)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (empleadosSeleccionados == null || empleadosSeleccionados.Count <= 0)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar al menos un cliente." });
				if (importe <= 0)
					return Json(new { error = true, warn = false, msg = "El importe debe ser mayor a cero." });
				if (cuotas <= 0)
					return Json(new { error = true, warn = false, msg = "La cuota debe ser mayor a 0." });
				var listaIdsPorAgregar = new List<string>();
				listaIdsPorAgregar.AddRange(empleadosSeleccionados.Select(x => x.id).ToList());
				if (AnticiposLista.Any() && AnticiposLista.Where(x => listaIdsPorAgregar.Contains(x.id)).Any())
					return Json(new { error = true, warn = false, msg = "Alguna de las cuentas que intenta agregar ya existen." });

				string ids = string.Join(", ", listaIdsPorAgregar);
				var tope = 0.00M;
				var topeCtaLista = _financieroServicio.GetFinancieroTopePorCuentaLista(ids, TokenCookie);

				var listaTemp = AnticiposLista;
				foreach (var item in empleadosSeleccionados)
				{
					var inter = Math.Round((importe * (intereses / 100)) , 2);
					var val_cuota = (importe + inter) / cuotas;
					var val_total = importe + inter;
					var tope_ = topeCtaLista.Where(x => x.cta_id == item.id)?.FirstOrDefault()?.saldo;
					var nuevoAnticipo = new AnticipoDto
					{
						id = Guid.NewGuid().ToString(),
						cta_id = item.id,
						cta_denominacion = item.descripcion,
						importe = importe,
						cuotas = cuotas,
						intereses = inter,
						valor_cuota = val_cuota,
						valor_total = val_total,
						tope = tope_ ?? 0,
						tope_original = tope_ ?? 0,
						mostrar_alerta = (val_total > tope_)
					};

					listaTemp.Add(nuevoAnticipo);
				}

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

		public JsonResult AplicarCambiosEnFilas(List<CuentaSeleccionada> anticipos, int cuotas, decimal importe, decimal interes, bool aplicarCuotas, bool aplicarImporte, bool aplicarInteres)
		{
			try
			{
				var auth = EstaAutenticado;

				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				if (anticipos == null || !anticipos.Any())
					return Json(new { error = true, warn = false, msg = "Debe seleccionar al menos un anticipo." });

				if (!aplicarCuotas && !aplicarImporte && !aplicarInteres)
					return Json(new { error = true, warn = false, msg = "Debe seleccionar al menos un campo para aplicar." });

				if (aplicarCuotas && (cuotas < 1 || cuotas > 60))
					return Json(new { error = true, warn = false, msg = "Las cuotas deben estar entre 1 y 60." });

				if (aplicarImporte && importe <= 0)
					return Json(new { error = true, warn = false, msg = "El importe debe ser mayor a cero." });

				if (aplicarInteres && interes < 0)
					return Json(new { error = true, warn = false, msg = "El interés debe ser mayor o igual a cero." });

				var idsSeleccionados = anticipos.Select(x => x.id).ToHashSet();
				var listaTemp = AnticiposLista;

				foreach (var item in listaTemp.Where(x => idsSeleccionados.Contains(x.cta_id)))
				{
					if (aplicarCuotas)
						item.cuotas = cuotas;

					if (aplicarImporte)
						item.importe = importe;

					decimal porcentajeInteres = interes;

					if (aplicarInteres)
						item.intereses = Math.Round((item.importe * (interes / 100)), 2);
					
					item.valor_total = item.importe + item.intereses;
					item.valor_cuota =
						item.cuotas > 0
							? item.valor_total / item.cuotas
							: 0;

					item.mostrar_alerta = item.valor_total > item.tope;
				}

				AnticiposLista = listaTemp;

				return Json(new
				{
					error = false,
					warn = false,
					msg = ""
				});
			}
			catch (Exception ex)
			{
				return Json(new
				{
					error = true,
					warn = false,
					msg = ex.InnerException?.Message ?? ex.Message
				});
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
				return AnalizarRespuesta(respuesta, "El Anticipo de empleado se ha cargado con éxito.");
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
			var tipos = top.Select(x => new
			{
				Id = x.Cta_Id,
				Descripcion = $"{x.Cta_Denominacion} ({x.Cta_Id})",
				TipoDesc = x.Tipo_Desc,
				Tipo = x.Tipo
			});
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
				if (lista != null && lista.Count > 0)
				{
					ProveedorDefault = lista.Where(x => x.Cta_Id == "C0030000").First();
				}
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
