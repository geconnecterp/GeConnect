using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using ExcelDataReader;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Enumeraciones;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class LiquidacionDeEmpleadosController : LiquidacionDeEmpleadosControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ImportacionLiquidacionDeEmpleado _importacionLiquidacionDeEmpleado;
		private readonly IFinancieroServicio _financieroServicio;

		//PARA MODULO DE IMPRESION
		private readonly DocsManager _docsManager; //recupero los datos desde el appsettings.json
		private AppModulo _modulo; //tengo el AppModulo que corresponde a la consulta de cuentas
		private string APP_MODULO = AppModulos.DDH.ToString();
		private readonly IDocManagerServicio _docMSv;

		//************************
		public LiquidacionDeEmpleadosController(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger<LiquidacionDeEmpleadosController> logger,
												IFinancieroServicio financieroServicio, IOptions<ImportacionLiquidacionDeEmpleado> options2,
												IDocManagerServicio docManager, IOptions<DocsManager> docsManager) : base(options, contexto, logger)
		{
			_setting = options.Value;
			_financieroServicio = financieroServicio;
			_importacionLiquidacionDeEmpleado = options2.Value;

			//PARA MODULO DE IMPRESION
			_docsManager = docsManager.Value; //recupero los datos desde el appsettings.json
			_modulo = _docsManager.Modulos.First(x => x.Id == APP_MODULO); //identifico los datos del modulo que necesito: ADE
			_docMSv = docManager; //instancio el servicio de impresión
		}

		public IActionResult Index()
		{
			var model = new LiqDeEmpleadoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "LIQUIDACIONES DE EMPLEADOS";
				ViewData["Titulo"] = titulo;

				#region Gestor Impresion - Inicializacion de variables
				//Inicializa el objeto MODAL del GESTOR DE IMPRESIÓN
				DocumentManager = _docMSv.InicializaObjeto(titulo, _modulo);
				// en este mismo acto se cargan los posibles documentos
				//que se pueden imprimir, exportar, enviar por email o whatsapp
				ArchivosCargadosModulo = _docMSv.GeneraArbolArchivos(_modulo);

				#endregion

				//Cargar combos
				var proximaLE = _financieroServicio.GetFinancieroProximaLE(TokenCookie);
				if (proximaLE != null && proximaLE.Count >= 0)
				{
					model.SelectedValueAnio = proximaLE[0].ProximaLE!.Substring(0, 4);
					model.SelectedValueMes = proximaLE[0].ProximaLE!.Substring(4, 2);
				}
				else
				{
					model.SelectedValueAnio = string.Empty;
					model.SelectedValueMes = string.Empty;
				}
				model.Anio = ComboAnios(ObtenerUltimosAnios(5));
				model.Mes = ComboMeses(ObtenerMeses());
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

		public IActionResult AbrirModalImportarArchivo()
		{
			var model = new ModalImportarArchivoModel();
			try
			{
				var lista = new List<ComboGenDto>
				{
					new() { Id = "1", Descripcion = "Tabulado" },
					new() { Id = "2", Descripcion = "XLS" },
				};
				model.OrigenDeDatos = HelperMvc<ComboGenDto>.ListaGenerica(lista);
				return PartialView("_modal_importar_archivo", model);
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
		public IActionResult ImportarArchivo(IFormFile archivoImportar, int origenId)
		{
			ListaTempArchivoParaImportar = [];
			var formatos = _importacionLiquidacionDeEmpleado.Formatos;
			var formato = formatos.FirstOrDefault(f => f.Id == origenId.ToString());
			if (formato == null) return BadRequest("Origen no configurado.");

			if (formato.Tipo == "XLSX")
			{
				var salida = ProcesadorArchivo.ParsearExcelConDeteccionAutomatica(archivoImportar, formato);
				ListaTempArchivoParaImportar = salida;
				return Ok(salida);
			}

			var lineas = LeerLineasTxt(archivoImportar, origenId); // método que lee el txt
			if (!ProcesadorArchivo.EsValido(lineas, formato, out var error))
				return BadRequest(new { mensaje = error, errores = new[] { error } });

			var salidaJson = ProcesadorArchivo.ParsearTxt(lineas, formato);
			ListaTempArchivoParaImportar = salidaJson;
			return Ok(salidaJson);
		}

		public JsonResult ValidarSiExistenRegistrosDeArchivoParaImportar()
		{
			try
			{
				return Json(new { error = false, warn = false, existenRegistros = ListaTempArchivoParaImportar.Any() });
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
		public async Task<IActionResult> ProcesarArchivo(IFormFile archivoImportar)
		{
			if (archivoImportar == null || archivoImportar.Length == 0)
				return BadRequest("Archivo no válido.");

			LiqTopeLista = [];
			var lista = new List<LiqTopeDto>();
			var errores = new List<string>();
			int filaActual = 0;

			string[] encabezadosEsperados =
			[
				"legajo", "importe"
			];

			if (archivoImportar.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
			{
				using var stream = archivoImportar.OpenReadStream();
				using var reader = new StreamReader(stream, Encoding.UTF8);

				while (!reader.EndOfStream)
				{
					var linea = await reader.ReadLineAsync();
					filaActual++;

					if (string.IsNullOrWhiteSpace(linea)) continue;

					var campos = linea.Split('\t');

					if (campos.Length < encabezadosEsperados.Length)
					{
						errores.Add($"Fila {filaActual}: columnas insuficientes ({campos.Length}).");
						continue;
					}

					try
					{
						lista.Add(new LiqTopeDto
						{
							legajo = campos[0],
							importe = decimal.Parse(campos[1], CultureInfo.InvariantCulture)
						});
					}
					catch (Exception ex)
					{
						errores.Add($"Fila {filaActual}: error de conversión → {ex.Message}");
					}
				}
			}
			else if (archivoImportar.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
			{
				using var ms = new MemoryStream();
				await archivoImportar.CopyToAsync(ms);
				using var workbook = new XLWorkbook(ms);
				var hoja = workbook.Worksheets.First();
				var encabezado = hoja.Row(1).Cells(1, encabezadosEsperados.Length).Select(c => c.GetString().Trim()).ToArray();

				//foreach (var fila in hoja.RowsUsed().Skip(1))
				foreach (var fila in hoja.RowsUsed())
				{
					filaActual = fila.RowNumber();
					try
					{
						var item = new LiqTopeDto
						{
							legajo = fila.Cell(1).GetString(),
							importe = Convert.ToDecimal(fila.Cell(2).Value, CultureInfo.InvariantCulture)
						};

						lista.Add(item);
					}
					catch (Exception ex)
					{
						errores.Add($"Fila {filaActual}: error de conversión → {ex.Message}");
					}
				}
			}
			else
			{
				return BadRequest("Formato de archivo no soportado.");
			}

			if (errores.Any())
			{
				return BadRequest(new { errores });
			}

			LiqTopeLista = lista;
			return Ok(lista);
		}

		public JsonResult ProcesarArchivoImportado(string periodo, string mes, int porcTope)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				LiqEmpleadoEncabezadoLista = [];
				LiqEmpleadoDetalleLista = [];

				var json_file = JsonConvert.SerializeObject(ListaTempArchivoParaImportar, new JsonSerializerSettings());
				var request = new FinancieroLiqEmpCargaRequest()
				{
					periodo = periodo,
					mes = mes,
					porc_tope = porcTope,
					json_topes = json_file
				};
				Console.WriteLine($"json_file: {request.json_topes}");
				if (!string.IsNullOrEmpty(json_file))
					LiqTopeLista = JsonConvert.DeserializeObject<List<LiqTopeDto>>(json_file);
				var lista = _financieroServicio.GetLiqEmpCarga(request, TokenCookie);
				if (lista == null || lista.Count <= 0)
					return Json(new { error = true, warn = false, msg = "Ha ocurrido un error al intentar obtener los datos desde la importación." });

				var json_encabezado_aux = JsonConvert.DeserializeObject<List<LiqEmpleadoEncabezadoDto>>(lista[0].json_c) ?? [];
				var json_detalle_aux = JsonConvert.DeserializeObject<List<LiqEmpleadoDetalleDto>>(lista[0].json_d) ?? [];

				if (json_encabezado_aux != null && json_encabezado_aux.Count > 0)
					LiqEmpleadoEncabezadoLista = json_encabezado_aux;

				if (json_detalle_aux != null && json_detalle_aux.Count > 0)
				{
					json_detalle_aux.ForEach(x => x.id = Guid.NewGuid().ToString());
					LiqEmpleadoDetalleLista = json_detalle_aux;
				}
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

		public IActionResult ObtenerGrillaEncabezado()
		{
			var model = new LiqDeEmpleadoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaEncabezado = ObtenerGridCoreSmart<LiqEmpleadoEncabezadoDto>(LiqEmpleadoEncabezadoLista);
				return PartialView("_grillaEncabezado", model);
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

		public IActionResult ObtenerGrillaDetalle(string cta_id)
		{
			var model = new LiqDeEmpleadoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaDetalle = ObtenerGridCoreSmart<LiqEmpleadoDetalleDto>(LiqEmpleadoDetalleLista.Where(x => x.cta_id == cta_id).ToList());
				return PartialView("_grillaDetalle", model);
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

		public JsonResult EditarItemEnLiqEmpDetalle(string cta_id, string dia_movi, string cm_compte, string tco_id, int cm_compte_cuota, string id, decimal val, string idSeleccionado)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				//Actualizamos el valor en la lista de detalle
				var listaTemp = LiqEmpleadoDetalleLista;
				var listaTempFiltrada = listaTemp.Where(x => x.cta_id == cta_id && x.dia_movi == dia_movi && x.cm_compte == cm_compte
												&& x.tco_id == tco_id && x.cm_compte_cuota == cm_compte_cuota).ToList();
				if (listaTempFiltrada == null || listaTempFiltrada.Count <= 0)
				{
					return Json(new { error = true, warn = false, msg = "No se ha encontrado el elemento para editar." });
				}
				listaTempFiltrada[0].cv_importe_imputado = val;
				LiqEmpleadoDetalleLista = listaTemp;
				//Fin de actualizacion de item en detalle

				//Actualizamos el valor en la lista de encabezado
				var listaTempEnc = LiqEmpleadoEncabezadoLista;
				var listaTempEncFiltrada = listaTempEnc.Where(x => x.cta_id == cta_id).ToList();
				if (listaTempEncFiltrada == null || listaTempEncFiltrada.Count <= 0)
				{
					return Json(new { error = true, warn = false, msg = "No se ha encontrado el elemento de encabezado para actualizar." });
				}
				var dtoSueldo = LiqEmpleadoDetalleLista.Where(x => x.cta_id == cta_id).Sum(y => y.cv_importe_imputado);
				var pendiente = listaTempEncFiltrada[0].cv_importe_tot_pend - dtoSueldo; //TODO MARCE: Pendiente de respueta de Carlos
				var porc = 0.00M;
				if (listaTempEncFiltrada[0].tope > 0)
					porc = RedondearHaciaArriba((dtoSueldo / listaTempEncFiltrada[0].tope), 2);

				listaTempEncFiltrada[0].cv_importe_tot_imputado = dtoSueldo;	// Dto Sueldo igual a la suma de Dto sueldo del detalle
				listaTempEncFiltrada[0].porc_imputado_sobre_tope = porc;		// %:  es igual a Dto Sueldo / Tope
				listaTempEncFiltrada[0].cv_importe_tot_pend = pendiente;		// Pendiente: igual a Obligaciones empleados – Dto Sueldo
				return Json(new { error = false, warn = false, msg = "", data = new { listaTempFiltrada[0].id, importe = val, dtoSueldo, pendiente, porc } });
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

		public JsonResult FinancieroLiqEmpleadoConfirmar(FinancieroLiqEmpleadoConfirmarRequest request)
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				request.usu_id = UserName;
				request.adm_id = AdministracionId;
				request.json_tope = JsonConvert.SerializeObject(LiqTopeLista);
				request.json_detalle = JsonConvert.SerializeObject(MapperDetalle(LiqEmpleadoDetalleLista));

				#region impresion de parametros
				Console.WriteLine($"json_tope: {request.json_tope}");
				Console.WriteLine($"json_detalle: {request.json_detalle}");
				Console.WriteLine($"periodo: {request.periodo}");
				Console.WriteLine($"mes: {request.mes}");
				Console.WriteLine($"porc_tope: {request.porc_tope}");
				Console.WriteLine($"actualiza_tope: {request.actualiza_tope}");
				Console.WriteLine($"concepto: {request.concepto}");
				Console.WriteLine($"usu_id: {request.usu_id}");
				Console.WriteLine($"adm_id: {request.adm_id}");
				#endregion
				var respuesta = _financieroServicio.FinancieroLiqEmpleadoConfirmar(request, TokenCookie);

				return AnalizarRespuesta(respuesta, "La Liquidación de ha confirmado con éxito.");
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

		public JsonResult CancelarCargaLiqEmp()
		{
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return Json(new { error = true, warn = false, msg = "No autenticado." });

				ListaTempArchivoParaImportar = [];
				LiqTopeLista = [];
				LiqEmpleadoEncabezadoLista = [];
				LiqEmpleadoDetalleLista = [];
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

		#region Clases Complementarias Importar Extracto
		enum TipoEstructuraFileImportExtracto
		{
			Delimitado = 1,
			XLSX
		}

		public static string[] LeerLineasTxt(IFormFile archivo, int origenId)
		{
			if (archivo == null || archivo.Length == 0)
				throw new ArgumentException("El archivo está vacío o no fue proporcionado.");

			using var reader = new StreamReader(archivo.OpenReadStream(), Encoding.UTF8);
			var lineas = new List<string>();

			while (!reader.EndOfStream)
			{
				var linea = reader.ReadLine();
				if (!string.IsNullOrWhiteSpace(linea))
					lineas.Add(linea.Trim());
			}

			return lineas.ToArray();
		}


		private static class ProcesadorArchivo
		{
			public static bool EsValido(string[] lineas, FormatoExtractoConfig config, out string error)
			{
				error = "";
				if (config.Tipo == "Fijo" && config.LongitudEsperada.HasValue)
				{
					if (lineas.Any(l => l.Length < config.LongitudEsperada.Value))
					{
						error = "Líneas con longitud insuficiente.";
						return false;
					}
				}
				else if (config.Tipo == "Delimitado")
				{
					foreach (var linea in lineas)
					{
						var partes = linea.Split(config.Separador);
						if (partes.Length < config.Campos.Count)
						{
							error = "Cantidad de columnas insuficiente.";
							return false;
						}
					}
				}
				return true;
			}

			public static List<Dictionary<string, object>> ParsearTxt(string[] lineas, FormatoExtractoConfig config)
			{
				var resultado = new List<Dictionary<string, object>>();

				foreach (var linea in lineas)
				{
					var fila = new Dictionary<string, object>();

					if (config.Tipo == "Fijo")
					{
						foreach (var campo in config.Campos)
						{
							var valor = linea.Substring(campo.Inicio!.Value, campo.Longitud!.Value).Trim();
							fila[campo.Nombre] = valor;
						}
					}
					else if (config.Tipo == "Delimitado")
					{
						var partes = linea.Split(config.Separador);
						foreach (var campo in config.Campos)
						{
							var valor = partes[campo.Posicion!.Value].Trim();
							fila[campo.Nombre] = valor;
						}
					}

					resultado.Add(fila);
				}

				return resultado;
			}

			public static List<Dictionary<string, object>> ParsearXlsx(IFormFile archivo, FormatoExtractoConfig config)
			{
				using var stream = archivo.OpenReadStream();
				using var workbook = new XLWorkbook(stream);
				var hoja = workbook.Worksheets.First();
				var rows = hoja.RangeUsed().RowsUsed().Skip(1);

				var resultado = new List<Dictionary<string, object>>();
				foreach (var row in rows)
				{
					var fila = new Dictionary<string, object>();
					for (int i = 0; i < config.Columnas.Count; i++)
					{
						var nombre = config.Columnas[i];
						var valor = row.Cell(i + 1).GetValue<string>() ?? "";
						fila[nombre] = valor;
					}
					resultado.Add(fila);
				}
				return resultado;
			}

			public static List<Dictionary<string, object>> ParsearExcelConDeteccionAutomatica(IFormFile archivo, FormatoExtractoConfig config)
			{
				using var stream = archivo.OpenReadStream();

				// Detectar si es XLS (binario) o XLSX (OpenXML)
				byte[] cabecera = new byte[8];
				stream.Read(cabecera, 0, 8);
				stream.Position = 0; // resetear el stream

				bool esXlsBinario = cabecera[0] == 0xD0 && cabecera[1] == 0xCF; // firma típica de .xls
				var resultado = new List<Dictionary<string, object>>();

				if (esXlsBinario)
				{
					// XLS binario → usar ExcelDataReader
					System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
					using var reader = ExcelReaderFactory.CreateReader(stream);

					int filaActual = 0;
					while (reader.Read())
					{
						if (filaActual++ == 0) continue; // salta encabezado

						var fila = new Dictionary<string, object>();
						for (int i = 0; i < config.Columnas.Count; i++)
						{
							var nombre = config.Columnas[i];
							var valor = reader.GetValue(i)?.ToString() ?? "";
							fila[nombre] = valor;
						}
						resultado.Add(fila);
					}
				}
				else
				{
					// XLSX → usar ClosedXML
					using var workbook = new XLWorkbook(stream);
					var hoja = workbook.Worksheets.First();
					var rows = hoja.RangeUsed().RowsUsed().Skip(1);

					foreach (var row in rows)
					{
						var fila = new Dictionary<string, object>();
						for (int i = 0; i < config.Columnas.Count; i++)
						{
							var nombre = config.Columnas[i];
							var valor = row.Cell(i + 1).GetValue<string>() ?? "";
							fila[nombre] = valor;
						}
						resultado.Add(fila);
					}
				}

				return resultado;
			}

		}

		#endregion

		#region Clases Complementarias
		private class DetalleLiquidacion
		{
			public string cta_id { get; set; } = string.Empty;
			public string cta_denominacion { get; set; } = string.Empty;
			public string cta_emp { get; set; } = string.Empty;
			public string cta_emp_legajo { get; set; } = string.Empty;
			public string cta_emp_ctaf { get; set; } = string.Empty;
			public decimal tope { get; set; } = 0.00M;
			public string dia_movi { get; set; } = string.Empty;
			public string tco_id { get; set; } = string.Empty;
			public string cm_compte { get; set; } = string.Empty;
			public int cm_compte_cuota { get; set; }
			public DateTime cv_fecha_vto { get; set; }
			public decimal cv_importe { get; set; } = 0.00M;
			public decimal cv_importe_imputado { get; set; } = 0.00M;
			public string concepto { get; set; } = string.Empty;
			public string ccb_id { get; set; } = string.Empty;
		}
		#endregion

		#region Métodos Privados
		private static decimal RedondearHaciaArriba(decimal valor, int decimales)
		{
			decimal factor = (decimal)Math.Pow(10, decimales);
			return Math.Ceiling(valor * factor) / factor;
		}

		private List<DetalleLiquidacion> MapperDetalle(List<LiqEmpleadoDetalleDto> listaDto)
		{
			var lista = new List<DetalleLiquidacion>();
			foreach (var dto in listaDto)
			{
				var item = new DetalleLiquidacion
				{
					cta_id = dto.cta_id,
					cta_denominacion = dto.cta_denominacion,
					cta_emp = dto.cta_emp,
					cta_emp_legajo = dto.cta_emp_legajo,
					cta_emp_ctaf = dto.cta_emp_ctaf,
					tope = dto.tope,
					dia_movi = dto.dia_movi,
					tco_id = dto.tco_id,
					cm_compte = dto.cm_compte,
					cm_compte_cuota = dto.cm_compte_cuota,
					cv_fecha_vto = dto.cv_fecha_vto,
					cv_importe = dto.cv_importe,
					cv_importe_imputado = dto.cv_importe_imputado,
					concepto = dto.concepto,
					ccb_id = dto.ccb_id
				};
				lista.Add(item);
			}
			return lista;
		}
		#endregion


	}
}
