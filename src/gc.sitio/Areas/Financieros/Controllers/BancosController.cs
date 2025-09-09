using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class BancosController : BancosControladorBase
	{
		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo_1; //VCE
		private AppModulo _modulo_2; //LBD
		private AppModulo _modulo_3; //LBR
		private AppModulo _modulo_4; //HLI
		private AppModulo _modulo_5; //EBA
		private string APP_MODULO_1 = AppModulos.VCE.ToString();
		private string APP_MODULO_2 = AppModulos.LBD.ToString();
		private string APP_MODULO_3 = AppModulos.LBR.ToString();
		private string APP_MODULO_4 = AppModulos.HLI.ToString();
		private string APP_MODULO_5 = AppModulos.EBA.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************

		private readonly AppSettings _setting;
		private readonly IFinancieroServicio _financieroServicio;
		public BancosController(IFinancieroServicio financieroServicio,
								IDocManagerServicio docManager, IOptions<DocsManager> docsManager,
								IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<BancosController> logger) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo_1 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_1);
			_modulo_2 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_2);
			_modulo_3 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_3);
			_modulo_4 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_4);
			_modulo_5 = _docsManager.Modulos.First(x => x.Id == APP_MODULO_5);
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new FiltroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "BANCOS";
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

		public IActionResult PosicionarseEnTabVencimientoChequeEmitido()
		{
			var model = new VencimientoChequeEmitidoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.FechaDesde = DateTime.Today;
				model.FechaHasta = DateTime.Today;

				if (ListaChequesAgrupados == null || ListaChequesAgrupados.Count == 0)
					model.GrillaCheques = new GridCoreSmart<FinancieroChequeDepositadoDto>();
				else
					model.GrillaCheques = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(ListaChequesAgrupados);

				if (ListaChequesDetalles == null || ListaChequesDetalles.Count == 0)
					model.GrillaChequesDetalle = new GridCoreSmart<FinancieroChequeDepositadoDto>();
				else
					model.GrillaChequesDetalle = ObtenerGridCoreSmart<FinancieroChequeDepositadoDto>(ListaChequesDetalles);

				model.Total = 0;
				return PartialView("_tabVencimientoChequeEmitido", model);
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

		public IActionResult PosicionarseEnTabExtractoBancario(FinancieroBcoExtractoRequest request)
		{
			var model = new ExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				model.GrillaExtracto = new GridCoreSmart<FinancieroBcoExtractoDto>();
				return PartialView("_tabExtractoBancario", model);
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

		public IActionResult ObtenerExtractoBancario(FinancieroBcoExtractoRequest request)
		{
			var model = new ExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}
				var lista = _financieroServicio.GetFinancieroBcoExtracto(request, TokenCookie);
				model.GrillaExtracto = ObtenerGridCoreSmart<FinancieroBcoExtractoDto>(lista);
				return PartialView("_tabExtractoBancario", model);
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

		public IActionResult PosicionarseEnTabHistoricoLibro(FinancieroBcoCtaCteRequest request)
		{
			var model = new HistoricoLibroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				model.GrillaHistorico = new GridCoreSmart<FinancieroBcoCtaCteDto>();
				return PartialView("_tabHistoricoLibro", model);
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

		public IActionResult ObtenerHistoricoLibro(FinancieroBcoCtaCteRequest request)
		{
			var model = new HistoricoLibroModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				var lista = _financieroServicio.GetFinancieroBcoCtaCte(request, TokenCookie);
				model.GrillaHistorico = ObtenerGridCoreSmart<FinancieroBcoCtaCteDto>(lista);
				return PartialView("_tabHistoricoLibro", model);
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

		public IActionResult PosicionarseEnTabLibroResumen(FinancieroBcoLibroResumenRequest request)
		{
			var model = new LibroBancoResumenModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				return PartialView("_tabLibroBancoResumen", model);
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

		public IActionResult ObtenerLibroResumen(FinancieroBcoLibroResumenRequest request)
		{
			var model = new LibroBancoResumenModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });
				if (request == null)
				{
					RespuestaGenerica<EntidadBase> response = new()
					{
						Ok = false,
						EsError = true,
						EsWarn = false,
						Mensaje = "Request vacío"
					};
					return PartialView("_gridMensaje", response);
				}

				var lista = _financieroServicio.GetFinancieroBcoLibroResumen(request, TokenCookie);
				model.GrillaCuentaFin = ObtenerGridCoreSmart<LibroBancoResumenDto>(ObtenerGrillaCuentaFinanciera(lista, TipoGrillaCuentaFinanciera.CuentaFinanciera));
				model.GrillaCuentaBan = ObtenerGridCoreSmart<LibroBancoResumenDto>(ObtenerGrillaCuentaFinanciera(lista, TipoGrillaCuentaFinanciera.CuentaBanco));
				return PartialView("_tabLibroBancoResumen", model);
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

				string titulo = string.Empty;
				switch ((TipoDeReporte)tipoReporte)
				{
					case TipoDeReporte.VencimientoChequeEmitido:
						break;
					case TipoDeReporte.LibroBancoDetalle:
						break;
					case TipoDeReporte.LibroBancoResumen:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "LIBRO BANCO RESUMEN";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_3);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_3);
						#endregion
						break;
					case TipoDeReporte.HistoricoLibro:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "HISTÓRICO LIBRO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_4);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_4);
						#endregion
						break;
					case TipoDeReporte.ExtractoBancario:
						#region Gestor Impresion - Inicializacion de variables
						titulo = "EXTRACTO BANCARIO";
						DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo_5);
						ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo_5);
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

		#region Métodos privados
		private List<LibroBancoResumenDto> ObtenerGrillaCuentaFinanciera(List<FinancieroBcoLibroResumenDto> lista, TipoGrillaCuentaFinanciera tipoGrilla)
		{
			var listaCuentaFin = new List<LibroBancoResumenDto>();
			if (lista == null || lista.Count == 0)
				return listaCuentaFin;

			var itemFinan = lista.First();
			var item = new LibroBancoResumenDto();

			if (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera)
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Financiera al Cierre",
					saldo = $"({(itemFinan.saldo_sis).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			else
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Banco al Cierre",
					saldo = $"{(itemFinan.saldo_ext).ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			var mas = itemFinan.cheques_sis + itemFinan.transferencias_h_sis + itemFinan.creditos_ext;
			item = new LibroBancoResumenDto { descripcion = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? "Mas" : "Menos"), saldo = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? $"{mas.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}" : $"({mas.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})"), es_fuente_negrita = true, background = "#60A5F3", es_header_2 = true };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Cheques emitidos no conciliados en el Sistema", saldo = $"{itemFinan.cheques_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Transferencias hacia bancos (extracciones, retiros) no conciliados en el Sistema", saldo = $"{itemFinan.transferencias_h_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Créditos realizadios por el banco (Perc., Imp., Ret., Com., etc.) no conciliados en Extracto", saldo = $"{itemFinan.creditos_ext.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			var menos = itemFinan.depositos_sis + itemFinan.transferencias_d_sis + itemFinan.debitos_ext;
			item = new LibroBancoResumenDto { descripcion = tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? "Menos" : "Mas", saldo = (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera ? $"({menos.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})" : $"{menos.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}"), es_fuente_negrita = true, background = "#60A5F3", es_header_2 = true };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Cheques de terceros depositados no conciliados en Sistema", saldo = $"{itemFinan.depositos_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Transferencias desde otros bancos (depósitos) pendientes no conciliados en el Sistema", saldo = $"{itemFinan.transferencias_d_sis.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			item = new LibroBancoResumenDto { descripcion = "Débitos realizadios por el banco (Int., Dev. de Perc., Dev. de Int., Dev. de Ret., Dev. de Com.) no conciliados en Extracto", saldo = $"{itemFinan.debitos_ext.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}", es_fuente_negrita = false, background = "" };
			listaCuentaFin.Add(item);
			var subTotal = mas - menos;
			if (tipoGrilla == TipoGrillaCuentaFinanciera.CuentaFinanciera)
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "SubTotal",
					saldo = subTotal < 0 ? $"{(-1 * subTotal).ToString("C", ForzarObtenerFormatoMonetario()).Trim()}" : $"{subTotal.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#60A5F3",
					es_header_2 = true
				};
				listaCuentaFin.Add(item);

				var saldo = itemFinan.saldo_sis + subTotal;
				if (saldo < 0) saldo *= -1;
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Cuenta Banco al Cierre",
					saldo = $"{saldo.ToString("C", ForzarObtenerFormatoMonetario()).Trim()}",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			else
			{
				item = new LibroBancoResumenDto
				{
					descripcion = "SubTotal",
					saldo = subTotal < 0 ? $"({(-1 * subTotal).ToString("C", ForzarObtenerFormatoMonetario()).Trim()})" : $"({subTotal.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#60A5F3",
					es_header_2 = true
				};
				listaCuentaFin.Add(item);

				var saldo = subTotal - itemFinan.saldo_ext;
				if (saldo < 0) saldo *= -1;
				item = new LibroBancoResumenDto
				{
					descripcion = "Saldo Estado de Cuenta Financiera al Cierre",
					saldo = $"({saldo.ToString("C", ForzarObtenerFormatoMonetario()).Trim()})",
					es_fuente_negrita = true,
					background = "#D3D047",
					es_header_1 = true
				};
				listaCuentaFin.Add(item);
			}
			return listaCuentaFin;
		}
		private void CargarDatosIniciales(FiltroModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();
			ViewBag.CuentaBancoList = HelperMvc<ComboGenDto>.ListaGenerica(cuentaBancoList);
		}

		enum TipoDeReporte
		{
			VencimientoChequeEmitido = 1,
			LibroBancoDetalle = 2,
			LibroBancoResumen = 3,
			HistoricoLibro = 4,
			ExtractoBancario = 5
		}

		enum TipoGrillaCuentaFinanciera
		{
			CuentaFinanciera = 1,
			CuentaBanco = 2
		}
		#endregion
	}
}
