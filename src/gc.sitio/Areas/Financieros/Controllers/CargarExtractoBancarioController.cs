using ClosedXML.Excel;
using gc.api.core.Entidades;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Financieros.Request;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Financieros.Models;
using gc.sitio.Areas.Financieros.Models.CargarExtractoBancario;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.DocManager;
using gc.sitio.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace gc.sitio.Areas.Financieros.Controllers
{
	[Area("Financieros")]
	public class CargarExtractoBancarioController : CargarExtractoBancarioControladorBase
	{
		private readonly AppSettings _setting;
		private readonly ImportacionExtracto _importacionExtracto;
		private readonly IFinancieroServicio _financieroServicio;
		private readonly ITipoConciliadoServicio _tipoConciliadoServicio;
		private readonly string tipoCTAF = "BA";
		public CargarExtractoBancarioController(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger<CargarExtractoBancarioController> logger,
												  IDocManagerServicio docManager, IOptions<DocsManager> docsManager, ITipoConciliadoServicio tipoConciliadoServicio,
												  IFinancieroServicio financieroServicio, IOptions<ImportacionExtracto> options2) : base(options, accessor, logger)
		{
			_setting = options.Value;
			_importacionExtracto = options2.Value;
			_financieroServicio = financieroServicio;
			_tipoConciliadoServicio = tipoConciliadoServicio;
		}
		public IActionResult Index()
		{
			var model = new FiltroExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				var titulo = "CARGAR EXTRACTO BANCARIO";
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

		public JsonResult ObtenerCuentaBanco(string ctaf_id)
		{
			try
			{
				if (ctaf_id == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var lista = ListaCuentaBancos.Where(x => x.ctaf_id == ctaf_id);
				if (lista == null || !lista.Any())
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado la cuenta banco solicitada." });

				return Json(new { error = false, warn = false, msg = "", lista.First().ext_fecha });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar obtener los datos de la cuenta banco seleccionada." });
			}
		}

		public IActionResult CargarExtractoBancarioCrud(FinancieroBcoExtractoRequest request)
		{
			var model = new CrudExtractoBancarioModel();
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
				var extractoLista = _financieroServicio.GetFinancieroBcoExtracto(request, TokenCookie);
				ListaExtractoBancario = extractoLista;
				if (extractoLista != null && extractoLista.Any())
				{
					ListaCrudExtractoBancario = [.. extractoLista.Select((x, index) => new CrudExtractoBancarioDto
					{
						orden = index + 1,
						ctaf_id = x.ctaf_id,
						ext_fecha = x.ext_fecha,
						ext_fecha_ori = x.ext_fecha,
						ext_fecha_movi = x.ext_fecha_movi,
						ext_debe = x.ext_debe,
						ext_haber = x.ext_haber,
						extr_id = x.extr_id,
						extr_desc = x.extr_desc,
						ext_concepto = x.ext_concepto,
						abm = "",
						ext_conciliado = x.ext_conciliado??'N',
						ext_conciliado_nro = x.ext_conciliado_nro,
						ext_conciliado_tipo = x.ext_conciliado_tipo,
						ctl_cierre = x.ctl_cierre??'N',
						ext_saldo = x.ext_saldo,
						ct_tipo = x.ct_tipo,
						ct_modo= x.ct_modo,
						cargado_desde_filtros = true
					})];
					model.CuentaBanco = $"{extractoLista.First().ctaf_denominacion} ({extractoLista.First().ctaf_id})";
				}
				else
				{
					var banco = ListaCuentaBancos.Where(x => x.ctaf_id == request.ctaf_id).FirstOrDefault();
					model.CuentaBanco = $"{banco?.ctaf_denominacion} ({request.ctaf_id})";
					ListaCrudExtractoBancario = [];
				}
				model.GrillaExtracto = ObtenerGridCoreSmart<CrudExtractoBancarioDto>(ListaCrudExtractoBancario.OrderBy(x => x.ext_fecha).ToList());
				return PartialView("_crudExtractoBancario", model);
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

		public IActionResult AbrirModalImportarExtracto()
		{
			var model = new ModalImportarExtractoModel();
			try
			{
				var lista = new List<ComboGenDto>
				{
					new() { Id = "1", Descripcion = "Texto Plano" },
					new() { Id = "2", Descripcion = "Standard" },
					new() { Id = "3", Descripcion = "XLSX" },
				};
				model.OrigenDeDatos = HelperMvc<ComboGenDto>.ListaGenerica(lista);
				return PartialView("_modal_importar_extracto", model);
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

		public IActionResult AbrirModalAgregarItemExtracto(string abm, int orden)
		{
			var model = new AgregarItemExtractoModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				if (abm == "A")
				{
					model.Fecha = CalcularYProponerFecha("A");
					model.Fecha_Movi = CalcularYProponerFecha("A");
					model.Comprobante = string.Empty;
					model.Insertar = false;
					model.Debe = 0;
					model.Haber = 0;
					model.Movimiento = ComboTiposConciliado();
					model.abm = abm;
					model.orden = orden;
				}
				else
				{
					var item = ListaCrudExtractoBancario.Where(x => x.orden == orden).FirstOrDefault();
					if (item == null)
					{
						RespuestaGenerica<EntidadBase> response = new()
						{
							Ok = false,
							EsError = true,
							EsWarn = false,
							Mensaje = "No se ha encontrado el elemento seleccionado."
						};
						return PartialView("_gridMensaje", response);
					}
					else
					{
						model.Fecha = item.ext_fecha;
						model.Fecha_Movi = item.ext_fecha_movi ?? DateTime.Now;
						model.Comprobante = item.ext_concepto;
						model.abm = abm;
						model.selected = item.extr_id;
						model.Haber = item.ext_haber;
						model.Debe = item.ext_debe;
						model.Insertar = false;
						model.Movimiento = ComboTiposConciliado();
						model.orden = orden;
					}
				}

				model.esPrimerRegistro = !ListaCrudExtractoBancario.Any();
				return PartialView("_modal_agregar_item_extr", model);
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

		public JsonResult QuitarItemExtracto(int orden)
		{
			try
			{
				if (orden <= 0)
					return Json(new { error = true, warn = false, msg = $"Debe especificar un ítem extracto a quitar." });

				var item = ListaCrudExtractoBancario.Where(x => x.orden == orden).FirstOrDefault();
				if (item == null)
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado el elemento a quitar." });

				//Actualizar lista de grilla
				var listaTemp = ListaCrudExtractoBancario;
				listaTemp = [.. listaTemp.Where(x => x.orden != item.orden)];
				//Reordenar(listaTemp);
				RecalcularExtracto(listaTemp);
				ListaCrudExtractoBancario = listaTemp;

				//Actualizar lista de elementos eliminiados, para informar luego al guardar
				if (item.cargado_desde_filtros)
				{
					var listaTempEliminados = ListaCrudExtractoBancarioEliminados;
					listaTempEliminados.Add(item);
					ListaCrudExtractoBancarioEliminados = listaTempEliminados;
				}

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}

		public JsonResult AgregarItemExtracto(ExtractoCrudItemRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });
				if (request.ext_debe > 0 && request.ext_haber > 0)
					return Json(new { error = true, warn = false, msg = $"Ambos valores (Debe y Haber) no pueden ser mayores a 0." });

				var maxOrden = 0;

				var listaTemp = ListaCrudExtractoBancario;
				maxOrden = listaTemp.Any() ? listaTemp.Max(x => x.orden) : 0;

				var newCrudItem = new CrudExtractoBancarioDto
				{
					ctaf_id = request.ctaf_id,
					ext_fecha = request.ext_fecha,
					ext_fecha_movi = request.ext_fecha_movi,
					ext_haber = request.ext_haber,
					ext_debe = request.ext_debe,
					ext_saldo = CalcularSaldo(ListaCrudExtractoBancario, request.ext_debe, request.ext_haber),
					extr_id = request.extr_id,
					extr_desc = request.extr_desc,
					ext_concepto = request.ext_concepto,
					abm = request.abm,
					ext_conciliado = 'N',
					ctl_cierre = 'N',
					orden = 0
				};

				if (!request.insertar)
				{

					//Si la fecha del item de extracto que estoy intentando ingresar, como ultimo registro, es menor a la fecha del ultimo registro, devuelvo
					//error informando
					if (listaTemp.Any())
					{
						var ultima_fecha = listaTemp.OrderByDescending(x => x.ext_fecha).First().ext_fecha;
						if (ultima_fecha > newCrudItem.ext_fecha)
						{
							newCrudItem.ext_fecha = SugerirFechaPosterior(ultima_fecha, 1, "segundos");
							newCrudItem.ext_fecha_movi = newCrudItem.ext_fecha_movi;
						}
					}

					newCrudItem.orden = maxOrden + 1;
					listaTemp.Add(newCrudItem);
				}
				else
				{
					InsertarYReordenar(listaTemp, newCrudItem, request.orden);
				}
				RecalcularExtracto(listaTemp);
				ListaCrudExtractoBancario = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}



		public JsonResult ModificarItemExtracto(ExtractoCrudItemRequest request)
		{
			try
			{
				if (request == null)
					return Json(new { error = true, warn = false, msg = $"Request vacío." });

				var listaTemp = ListaCrudExtractoBancario;
				var newCrudItem = listaTemp.Where(x => x.orden == request.orden).FirstOrDefault();
				if (newCrudItem == null)
					return Json(new { error = true, warn = false, msg = $"No se ha encontrado el ítem a modificar." });

				newCrudItem.extr_id = request.extr_id;
				newCrudItem.extr_desc = request.extr_desc;
				newCrudItem.ext_concepto = request.ext_concepto;
				newCrudItem.ext_debe = request.ext_debe;
				newCrudItem.ext_haber = request.ext_haber;
				if (newCrudItem.abm == "") //Si es alta, lo dejo como "A"
					newCrudItem.abm = request.abm;

				RecalcularExtracto(listaTemp);
				ListaCrudExtractoBancario = listaTemp;

				return Json(new { error = false, warn = false, msg = "" });
			}
			catch (Exception)
			{
				return Json(new { error = true, warn = false, msg = $"Se ha producido un error al intentar agregar un ítem al extracto." });
			}
		}

		public IActionResult ObtenerListaExtractoBancario()
		{
			var model = new CrudExtractoBancarioModel();
			try
			{
				var auth = EstaAutenticado;
				if (!auth.Item1 || auth.Item2 < DateTime.Now)
					return RedirectToAction("Login", "Token", new { area = "seguridad" });

				model.GrillaExtracto = ObtenerGridCoreSmart<CrudExtractoBancarioDto>(ListaCrudExtractoBancario);
				return PartialView("_grillaExtractoBancario", model);
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

		public JsonResult InicializarDatosEnSesion()
		{
			try
			{
				ListaCrudExtractoBancario = [];
				ListaTempArchivoParaImportar = [];
				ListaCrudExtractoBancarioEliminados = [];
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

		public JsonResult ValidarAntesDeConfirmar()
		{
			try
			{
				return Json(new { error = false, warn = false, existenRegistros = ListaCrudExtractoBancario.Any() });
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

		public JsonResult ConfirmarExtracto(string ctaf_id)
		{
			try
			{
				if (string.IsNullOrEmpty(ctaf_id))
					return Json(new { error = true, warn = false, msg = "Request vacío." });
				var jsonExtracto = ListaCrudExtractoBancario.Select((x, index) => new JsonExtractoModel
				{
					ctaf_id = x.ctaf_id,
					ext_fecha = x.ext_fecha,
					extr_id = x.extr_id,
					extr_desc = x.extr_desc,
					ext_concepto = x.ext_concepto,
					ext_debe = x.ext_debe,
					ext_haber = x.ext_haber,
					ext_saldo = x.ext_saldo,
					ct_tipo = x.ct_tipo,
					ct_modo = x.ct_modo,
					abm = x.abm,
					ext_fecha_movi = x.ext_fecha_movi,
					orden = x.orden,
					ext_conciliado = x.ext_conciliado,
					ext_conciliado_nro = x.ext_conciliado_nro,
					ext_conciliado_tipo = x.ext_conciliado_tipo,
					usu_id_carga = UserName,
					usu_id_concilia = x.usu_id_concilia,
					ext_fecha_ori = x.ext_fecha_ori
				});
				var jsonExtractoEliminado = ListaCrudExtractoBancarioEliminados.Select((x, index) => new JsonExtractoEliminadoModel
				{
					ctaf_id = x.ctaf_id,
					ext_fecha = x.ext_fecha,
					ext_debe = x.ext_debe,
					ext_haber = x.ext_haber,
				});
				var request = new SetExtractoBancarioConfirmaRequest
				{
					ctaf_id = ctaf_id,
					adm_id = AdministracionId,
					usu_id = UserName,
					json_extracto = JsonConvert.SerializeObject(jsonExtracto, new JsonSerializerSettings()),
					json_eliminado = JsonConvert.SerializeObject(jsonExtractoEliminado, new JsonSerializerSettings())
				};
				Console.WriteLine($"json_extracto: {request.json_extracto}");
				Console.WriteLine($"json_eliminado: {request.json_eliminado}");
				Console.WriteLine($"ctaf_id: {request.ctaf_id}");
				Console.WriteLine($"usu_id: {request.usu_id}");
				var respuesta = _financieroServicio.SetExtractoBancarioConfirmar(request, TokenCookie);
				return AnalizarRespuesta(respuesta, "El Extracto se confirmó con éxito.");
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

		private char ObtenerTipoFile(int origenId)
		{
			if ((int)TipoEstructuraFileImportExtracto.Fijo == origenId)
				return 'J';
			else if ((int)TipoEstructuraFileImportExtracto.Delimitado == origenId)
				return 'D';
			else
				return 'M';
		}

		public JsonResult ProcesarArchivoImportado(string ctafId, int origenId)
		{
			try
			{
				var tipo_file = ObtenerTipoFile(origenId);
				var json_file = JsonConvert.SerializeObject(ListaTempArchivoParaImportar, new JsonSerializerSettings());
				var request = new ExtractoBcoFileRequest()
				{
					ctaf_id = ctafId,
					json_file = json_file,
					tipo_file = tipo_file,
					usu_id = AdministracionId
				};
				Console.WriteLine($"json_file: {request.json_file}");
				Console.WriteLine($"tipo_file: {request.tipo_file}");
				var lista = _financieroServicio.GetBcoExtractoDesdeFile(request, TokenCookie);
				if (lista == null || lista.Count <= 0)
					return Json(new { error = true, warn = false, msg = "Ha ocurrido un error al intentar procesar los registros del archivo importado." });
				var item = lista[0];
				if (item.resultado > 0)
					return Json(new { error = true, warn = false, msg = item.resultado_msj });

				lista.ForEach(x => 
				{ 
					x.ext_fecha_ori = x.ext_fecha; 
					x.abm = "A"; 
				});
				ActualizarSaldosPostImportacionYReordenar(lista);
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

		private void ActualizarSaldosPostImportacionYReordenar(List<CrudExtractoBancarioDto> listaItemsParaAgregar)
		{
			var temp = ListaCrudExtractoBancario;
			var orden = temp.Any() ? temp.Max(x => x.orden) : 0;
			if (listaItemsParaAgregar != null && listaItemsParaAgregar.Any())
			{
				foreach (var item in listaItemsParaAgregar)
				{
					orden++;
					item.ext_saldo = CalcularSaldo(temp, item.ext_debe, item.ext_haber);
					item.orden = orden;
					temp.Add(item);
				}
				//Reordenar(temp);
				ListaCrudExtractoBancario = temp;
			}
		}

		[HttpPost]
		public IActionResult ImportarArchivo(IFormFile archivoImportar, int origenId)
		{
			ListaTempArchivoParaImportar = [];
			var formatos = _importacionExtracto.Formatos;
			var formato = formatos.FirstOrDefault(f => f.Id == origenId.ToString());
			if (formato == null) return BadRequest("Origen no configurado.");

			if (formato.Tipo == "XLSX")
			{
				var salida = ProcesadorExtracto.ParsearXlsx(archivoImportar, formato);
				ListaTempArchivoParaImportar = salida;
				return Ok(salida);
			}

			var lineas = LeerLineasTxt(archivoImportar, origenId); // método que lee el txt
			if (!ProcesadorExtracto.EsValido(lineas, formato, out var error))
				return BadRequest(new { mensaje = error, errores = new[] { error } });

			var salidaJson = ProcesadorExtracto.ParsearTxt(lineas, formato);
			ListaTempArchivoParaImportar = salidaJson;
			return Ok(salidaJson);
		}


		[HttpPost]
		public async Task<IActionResult> ProcesarArchivo(IFormFile archivoImportar)
		{
			if (archivoImportar == null || archivoImportar.Length == 0)
				return BadRequest("Archivo no válido.");

			var lista = new List<CrudExtractoBancarioDto>();
			var errores = new List<string>();
			int filaActual = 0;

			string[] encabezadosEsperados = new[]
			{
				"ctaf_id", "ext_fecha", "extr_id", "extr_desc", "ext_concepto",
				"ext_debe", "ext_haber", "ext_saldo", "ct_tipo", "ct_modo", "abm"
			};

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

					if (filaActual == 1)
					{
						if (campos.Length != encabezadosEsperados.Length ||
							!campos.SequenceEqual(encabezadosEsperados, StringComparer.OrdinalIgnoreCase))
						{
							return BadRequest("Encabezados inválidos o en orden incorrecto.");
						}
						continue;
					}

					if (campos.Length < encabezadosEsperados.Length)
					{
						errores.Add($"Fila {filaActual}: columnas insuficientes ({campos.Length}).");
						continue;
					}

					try
					{
						lista.Add(new CrudExtractoBancarioDto
						{
							ctaf_id = campos[0],
							ext_fecha = DateTime.Parse(campos[1]),
							extr_id = campos[2],
							extr_desc = campos[3],
							ext_concepto = campos[4],
							ext_debe = decimal.Parse(campos[5], CultureInfo.InvariantCulture),
							ext_haber = decimal.Parse(campos[6], CultureInfo.InvariantCulture),
							ext_saldo = decimal.Parse(campos[7], CultureInfo.InvariantCulture),
							ct_tipo = campos[8],
							ct_modo = campos[9],
							abm = campos[10]
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

				if (!encabezado.SequenceEqual(encabezadosEsperados, StringComparer.OrdinalIgnoreCase))
				{
					return BadRequest("Encabezados inválidos o en orden incorrecto.");
				}

				foreach (var fila in hoja.RowsUsed().Skip(1))
				{
					filaActual = fila.RowNumber();
					try
					{
						var item = new CrudExtractoBancarioDto
						{
							ctaf_id = fila.Cell(1).GetString(),
							ext_fecha = fila.Cell(2).GetDateTime(),
							extr_id = fila.Cell(3).GetString(),
							extr_desc = fila.Cell(4).GetString(),
							ext_concepto = fila.Cell(5).GetString(),
							ext_debe = Convert.ToDecimal(fila.Cell(6).Value, CultureInfo.InvariantCulture),
							ext_haber = Convert.ToDecimal(fila.Cell(7).Value, CultureInfo.InvariantCulture),
							ext_saldo = Convert.ToDecimal(fila.Cell(8).Value, CultureInfo.InvariantCulture),
							ct_tipo = fila.Cell(9).GetString(),
							ct_modo = fila.Cell(10).GetString(),
							abm = fila.Cell(11).GetString()
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

			return Ok(lista);
		}



		#region Clases Complementarias Importar Extracto
		enum TipoEstructuraFileImportExtracto
		{
			Fijo = 1,
			Delimitado,
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
				if (!string.IsNullOrWhiteSpace(linea) && (linea.StartsWith('2') && (int)TipoEstructuraFileImportExtracto.Fijo == origenId))
					lineas.Add(linea.Trim());
			}

			return lineas.ToArray();
		}


		public static class ProcesadorExtracto
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
		}

		#endregion

		#region Metodos Privados
		private static DateTime SugerirFechaPosterior(DateTime fechaBase, double cantidad, string unidad)
		{
			return unidad.ToLower() switch
			{
				"dias" or "día" => fechaBase.AddDays(cantidad),
				"horas" => fechaBase.AddHours(cantidad),
				"minutos" => fechaBase.AddMinutes(cantidad),
				"segundos" => fechaBase.AddSeconds(cantidad),
				"milisegundos" => fechaBase.AddMilliseconds(cantidad),
				"meses" => fechaBase.AddMonths((int)cantidad),
				"años" => fechaBase.AddYears((int)cantidad),
				_ => throw new ArgumentException("Unidad de tiempo no reconocida.")
			};
		}


		private DateTime CalcularYProponerFecha(string tipoOp)
		{
			if (ListaCrudExtractoBancario == null || ListaCrudExtractoBancario.Count <= 0)
				return DateTime.Now;
			if (tipoOp == "A")
			{
				var item = ListaCrudExtractoBancario.OrderByDescending(x => x.orden).First();
				return SugerirFechaPosterior(item.ext_fecha, 1, "minutos");
			}
			else
			{
				return DateTime.Now;
			}
		}

		private decimal CalcularSaldo(List<CrudExtractoBancarioDto> lista, decimal debe, decimal haber)
		{
			if (lista == null || lista.Count <= 0)
				return 0.00M;
			var item = lista.OrderByDescending(x => x.orden).First();
			if (debe > 0)
				return item.ext_saldo - debe;
			else
				return item.ext_saldo + haber;
		}

		public void InsertarYReordenar(List<CrudExtractoBancarioDto> lista, CrudExtractoBancarioDto nuevoItem, int posicion)
		{
			// Validar límites
			if (posicion < 0) posicion = 0;
			if (posicion > lista.Count) posicion = lista.Count;

			// Insertar en la posición deseada
			lista.Insert(posicion, nuevoItem);

			// Reordenar campo 'orden'
			for (int i = 0; i < lista.Count; i++)
			{
				lista[i].orden = i + 1;
			}

			// Base: fecha sin hora del primer ítem
			DateTime fechaBase = lista[0].ext_fecha.Date;
			TimeSpan incremento = TimeSpan.FromSeconds(1); // Podés usar segundos si necesitás más granularidad

			for (int i = 0; i < lista.Count; i++)
			{
				var fecha = lista[i].ext_fecha.Date;
				lista[i].ext_fecha = fecha.Add(incremento * i);
			}
		}

		public void Reordenar(List<CrudExtractoBancarioDto> lista)
		{
			// Reordenar todos los elementos
			for (int i = 0; i < lista.Count; i++)
			{
				lista[i].orden = i + 1; // o i si querés que empiece en 0
			}
		}

		public static void RecalcularExtracto(List<CrudExtractoBancarioDto> lista)
		{
			if (lista == null || lista.Count == 0) return;

			// Ordenar por fecha original y orden original si aplica
			//lista = [.. lista.OrderBy(x => x.ext_fecha).ThenBy(x => x.orden)];
			lista = [.. lista.OrderBy(x => x.orden)];

			// Reasignar orden incremental
			for (int i = 0; i < lista.Count; i++)
			{
				lista[i].orden = i + 1;
			}

			// Recalcular saldo
			if (lista.Count > 1)
			{
				for (int i = 1; i < lista.Count; i++)
				{
					var anterior = lista[i - 1];
					var actual = lista[i];

					actual.ext_saldo = anterior.ext_saldo - actual.ext_debe + actual.ext_haber;
				}
			}
			else
			{
				lista[0].ext_saldo = lista[0].ext_debe > 0 ? (-1 * lista[0].ext_debe) : lista[0].ext_haber;
			}

			// Recalcular fechas con segundos incrementales si hay fechas iguales
			var fechaBase = lista[0].ext_fecha.Date;
			var segundos = 0;

			for (int i = 0; i < lista.Count; i++)
			{
				var actual = lista[i];

				if (actual.ext_fecha.Date == fechaBase)
				{
					actual.ext_fecha = actual.ext_fecha.Date.AddSeconds(segundos);
					segundos++;
				}
				else
				{
					fechaBase = actual.ext_fecha.Date;
					segundos = 0;
					actual.ext_fecha = actual.ext_fecha.Date;
				}
			}
		}

		private void CargarDatosIniciales(FiltroExtractoModel model)
		{
			var ctfLista = _financieroServicio.GetFinancieroDesdeTipoParaSeleccionDeValores("BA", AdministracionId, TokenCookie);
			ListaCuentaBancos = ctfLista;
			model.CuentaBanco = HelperMvc<ComboGenDto>.ListaGenerica(ctfLista.Select(x => new ComboGenDto { Id = x.ctaf_id, Descripcion = $"{x.ctaf_denominacion} ({x.ctaf_id})" }));
			var cuentaBancoList = new List<ComboGenDto>();

			if (TipoConciliadoLista.Count == 0)
				ObtenerTiposConciliado(_tipoConciliadoServicio);
		}
		#endregion
	}
}
