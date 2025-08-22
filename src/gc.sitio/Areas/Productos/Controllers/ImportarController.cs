using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Helpers;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.Areas.Productos.Models;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Globalization;

namespace gc.sitio.Areas.Productos.Controllers
{
    [Area("Productos")]
    public class ImportarController : ControladorImportarBase
    {
        private readonly AppSettings _appSettings;
        private readonly IProducto2Servicio _productoServicio;
        private readonly ICuentaServicio _cuentaServicio;
        private readonly IImportarServicio _impServicio;
        private readonly ICompositeViewEngine _viewEngine; // ✅ AGREGAR: Inyección de dependencia
        private ProveedorListaDto _datosProveedor;
        private ProveedorPerfilDto _perfilProv;

        private class MapeoConfig
        {
            public string[] Sinonimos { get; set; } = Array.Empty<string>();
            public string[] PalabrasClave { get; set; } = Array.Empty<string>();
            public char[] TiposCompatibles { get; set; } = Array.Empty<char>();
            public int PrioridadBase { get; set; } = 50;
        }

        private class CeldaCombinada
        {
            public int FilaInicio { get; set; }
            public int FilaFin { get; set; }
            public int ColumnaInicio { get; set; }
            public int ColumnaFin { get; set; }
            public string Valor { get; set; } = string.Empty;
            public ExcelAddress? Direccion { get; set; }
        }

        public ImportarController(
            ICuentaServicio cuentaServicio,
            IProducto2Servicio productoServicio,
            ILogger<CompraController> logger,
            IOptions<AppSettings> options,
            IImportarServicio impServicio,
            ICompositeViewEngine viewEngine, // ✅ AGREGAR: Inyectar ViewEngine
            IHttpContextAccessor context) : base(options, context, logger)

        {
            _cuentaServicio = cuentaServicio;
            _productoServicio = productoServicio;
            _appSettings = options.Value;
            _impServicio = impServicio;
            _viewEngine = viewEngine; // ✅ ASIGNAR: ViewEngine
            _datosProveedor = ProveedorSeleccionado;
            _perfilProv = new ProveedorPerfilDto { detalles = [] };
        }

        public IActionResult Index()
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                CargarDatosIniciales();
                string titulo = "Productos - IMPORTACIÓN de Precios";
                ViewData["Titulo"] = titulo;
            }
            catch (NegocioException ex)
            {
                _logger?.LogError(ex, "Error de negocio al cargar la vista de BSS");
                TempData["error"] = ex.Message;
                return View();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error al cargar la vista de BSS");
                TempData["error"] = "Hubo un problema al cargar la vista del BSS. Si el problema persiste, contacte al administrador.";
                return View();
            }
            return View(_datosProveedor);
        }

        /// <summary>
        /// Notas y recomendaciones
        /// Tolerancia a encabezados multi-línea(dos filas de cabeceras) : Si llegás a tener archivos con cabeceras en 2 filas(no títulos, sino headers multinivel), podés ajustar la heurística para elegir la última fila candidata de cabecera antes de que empiecen los datos, o bien consolidar ambos niveles(concatenando FilaHeader1[col] + "_" + FilaHeader2[col]).
        /// Métricas: Si ves falsos positivos con títulos muy “tabulares”, subí el umbral de cobertura(ej. 0.5) o el de contraste(ej. 0.4), o aumenta la penalización por merges/long text.
        /// Rendimiento: El análisis toca solo las primeras maxFilasExploracion filas y calcula stats simples; es rápido para hojas grandes.
        /// </summary>
        /// <param name="archivo"></param>
        /// <returns></returns>


        [HttpPost]
        public async Task<IActionResult> AnalizarColumnas(IFormFile archivo)
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new
                    {
                        error = true,
                        mensaje = "No se recibió ningún archivo"
                    });
                }

                // Validar extensión
                var extension = Path.GetExtension(archivo.FileName).ToLower();
                if (extension != ".xlsx" && extension != ".xls")
                {
                    return Json(new
                    {
                        error = true,
                        mensaje = "Solo se permiten archivos Excel (.xlsx, .xls)"
                    });
                }

                var analisis = await AnalizarEstructuraExcel(archivo);

                // ✅ NUEVO: Incluir campos disponibles para mapeo
                analisis.CamposDisponibles = DatosParaImportacion;

                // ✅ NUEVO: Aplicar mapeo automático inteligente
                AplicarMapeoAutomaticoInteligente(analisis);

                return Json(new
                {
                    error = false,
                    analisis = analisis,
                    mensaje = "Análisis de estructura completado"
                });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error analizando archivo Excel");
                return Json(new
                {
                    error = true,
                    mensaje = "Error al analizar el archivo: " + ex.Message
                });
            }
        }

        // ✅ CORREGIR: Endpoint ProcesarExcel - Procesar datos reales
        [HttpPost]
        public async Task<IActionResult> ProcesarExcel(IFormFile archivo, string proveedorId, string mapeoColumnas)
        {
            try
            {
                // Versión optimizada del código de autenticación
                if (!VerificarAutenticacion(out IActionResult redirectResult))
                    return redirectResult;

                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new { error = true, mensaje = "No se recibió ningún archivo para procesar" });
                }

                if (string.IsNullOrEmpty(proveedorId))
                {
                    return Json(new { error = true, mensaje = "ID de proveedor requerido" });
                }

                _logger?.LogInformation($"Procesando Excel: {archivo.FileName} para proveedor: {proveedorId}");

                // ✅ 1. Analizar estructura del Excel
                var analisis = await AnalizarEstructuraExcel(archivo);

                // ✅ 2. Aplicar mapeo
                analisis.CamposDisponibles = DatosParaImportacion;
                AplicarMapeoAutomaticoInteligente(analisis);

                if (!string.IsNullOrEmpty(mapeoColumnas))
                {
                    AplicarMapeoManual(analisis, mapeoColumnas);
                }

                // ✅ 3. Procesar datos del Excel
                var datosImportacion = await ProcesarDatosDelExcel(archivo, analisis, proveedorId);

                if (datosImportacion?.Filas?.Any() != true)
                {
                    return Json(new { error = true, mensaje = "No se encontraron datos válidos para procesar en el Excel" });
                }

                // ✅ 4. Enviar datos a la API - OPTIMIZADO
                var resultado = await EnviarDatosALaAPIOptimizado(datosImportacion);

                if (resultado.Ok && resultado.ListaEntidad?.Any() == true)
                {
                    // ✅ 5. Generar vista parcial con resultados
                    var vistaResultado = await GenerarVistaResultadosImportacion(resultado.ListaEntidad, datosImportacion);

                    _logger?.LogInformation($"✅ Importación procesada: {resultado.ListaEntidad.Count} registros");

                    return Json(new
                    {
                        error = false,
                        mensaje = "Importación procesada exitosamente",
                        datos = new
                        {
                            registrosProcesados = resultado.ListaEntidad.Count,
                            registrosConError = resultado.ListaEntidad.Count(r => r.registro_estado != 0),
                            registrosExitosos = resultado.ListaEntidad.Count(r => r.registro_estado == 0),
                            archivo = archivo.FileName,
                            proveedor = proveedorId,
                            fechaProceso = datosImportacion.FechaProceso.ToString("yyyy-MM-dd HH:mm:ss")
                        },
                        vistaResultados = vistaResultado
                    });
                }
                else
                {
                    _logger?.LogWarning($"❌ Error en importación: {resultado.Mensaje}");
                    return Json(new { error = true, mensaje = resultado.Mensaje ?? "Error desconocido procesando la importación" });
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error procesando Excel para importación");
                return Json(new
                {
                    error = true,
                    mensaje = "Error interno al procesar la importación. Contacte al administrador."
                });
            }
        }

        // ✅ NUEVO: Método optimizado para envío a API
        private async Task<RespuestaGenerica<RespuestaCPDto>> EnviarDatosALaAPIOptimizado(DatosImportacionDto datosImportacion)
        {
            try
            {                

                // ✅ Serializar con configuración optimizada
                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss",
                    Formatting = Formatting.None,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                };
                //extraemos el mapeado de columnas para hacer un proceso paralelo a la carga de datos.
                var mapeadoColumnas = datosImportacion.MapeoColumnas;
                datosImportacion.MapeoColumnas = [];

                NormalizaDatos(datosImportacion);

                var datosJsonCarga = JsonConvert.SerializeObject(datosImportacion, jsonSettings);
                var datosJsonMap = JsonConvert.SerializeObject(mapeadoColumnas, jsonSettings);

                var abmDto = new AbmPlusGenDto
                {
                    Objeto = ProveedorSeleccionado.Cta_Id,
                    Usuario = User.Identity?.Name ?? "system",
                    Administracion = AdministracionId ?? "0000",
                    Json = datosJsonCarga,
                    Json2 = datosJsonMap,
                    Abm = 'A'
                };

                // ✅ Llamar al servicio optimizado
                var resultado = await _impServicio.CargarImportacionPrecio(abmDto, TokenCookie);

                _logger?.LogInformation($"Respuesta API - OK: {resultado.Ok}, Registros: {resultado.ListaEntidad?.Count ?? 0}");

                return resultado;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error enviando datos a la API");
                return new RespuestaGenerica<RespuestaCPDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al comunicarse con la API de importación"
                };
            }
        }

        /// <summary>
        /// Esta método tiene como misión truncar los valores que tiene solo 1 decimal o 2 decimales o 3 decimales
        /// </summary>
        /// <param name="datosImportacion"></param>
        private void NormalizaDatos(DatosImportacionDto datosImportacion)
        {
            var camposATruncar = new[] { "p_dto1", "p_dto2", "p_dto3", "p_dto4", "p_dto_pa", "p_porc_flete" };

            foreach (var fila in datosImportacion.Filas)
            {
                TruncarCampos(fila.Valores, camposATruncar, 1);
            }

            camposATruncar = new[] {  "p_pcosto" };
            foreach (var fila in datosImportacion.Filas)
            {
                TruncarCampos(fila.Valores, camposATruncar, 2);
            }

            camposATruncar = new[] { "p_plista" };
            foreach (var fila in datosImportacion.Filas)
            {
                TruncarCampos(fila.Valores, camposATruncar, 3);
            }
        }

        private void TruncarCampos(Dictionary<string, object?> diccionario, IEnumerable<string> claves, int decimales)
        {
            decimal factor = (decimal)Math.Pow(10, decimales);

            foreach (var clave in claves)
            {
                if (diccionario.TryGetValue(clave, out var valor))
                {
                    if (valor is decimal dec)
                    {
                        diccionario[clave] = Math.Truncate(dec * factor) / factor;
                    }
                    else if (valor is double dbl)
                    {
                        diccionario[clave] = (double)(Math.Truncate((decimal)dbl * factor) / factor);
                    }
                }
            }
        }

        // ✅ ACTUALIZADO: Método que usa la función corregida
        private async Task<string> GenerarVistaResultadosImportacion(List<RespuestaCPDto> resultados, DatosImportacionDto datosOriginales)
        {
            try
            {
                var modelo = new ResultadoImportacionViewModel
                {
                    Resultados = resultados.OrderBy(r => r.p_id).ToList(),
                    TotalRegistros = resultados.Count,
                    RegistrosExitosos = resultados.Count(r => r.registro_estado == 1),
                    RegistrosConError = resultados.Count(r => r.registro_estado == -1),
                    ArchivoOriginal = datosOriginales.NombreArchivo,
                    FechaProceso = datosOriginales.FechaProceso,
                    ProveedorId = datosOriginales.ProveedorId
                };

                // ✅ USAR: Método corregido
                return await RenderViewToStringAsyncSimplificado("_GridResultadosImportacion", modelo);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generando vista de resultados");
                return "<div class='alert alert-danger'>Error generando vista de resultados</div>";
            }
        }      

        // ✅ ALTERNATIVA: Método simplificado usando servicios de ASP.NET Core
        private async Task<string> RenderViewToStringAsyncSimplificado(string viewName, object model)
        {
            try
            {
                // ✅ MÉTODO ALTERNATIVO: Más simple y robusto
                var actionContext = new ActionContext(
                    HttpContext,
                    RouteData,
                    ControllerContext.ActionDescriptor
                );

                using var writer = new StringWriter();

                var viewResult = _viewEngine.FindView(actionContext, viewName, false);

                if (!viewResult.Success)
                {
                    return $"<div class='alert alert-warning'>Vista parcial '{viewName}' no encontrada</div>";
                }

                var viewDictionary = new ViewDataDictionary<object>(ViewData, model);

                var viewContext = new ViewContext(
                    actionContext,
                    viewResult.View,
                    viewDictionary,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.ToString();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Error renderizando vista '{viewName}'");
                return $"<div class='alert alert-danger'>Error interno renderizando vista</div>";
            }
        }

        // ✅ NUEVO: Aplicar mapeo manual enviado desde el frontend
        private void AplicarMapeoManual(AnalisisExcelDto analisis, string mapeoColumnasJson)
        {
            try
            {
                var mapeoManual = JsonConvert.DeserializeObject<Dictionary<int, string>>(mapeoColumnasJson);

                if (mapeoManual == null) return;

                foreach (var mapeo in mapeoManual)
                {
                    var columna = analisis.Columnas?.FirstOrDefault(c => c.Indice == mapeo.Key);
                    if (columna != null && !string.IsNullOrEmpty(mapeo.Value))
                    {
                        // Buscar información del campo
                        var campoInfo = DatosParaImportacion.FirstOrDefault(d => d.Campo == mapeo.Value);
                        if (campoInfo != null)
                        {
                            columna.CampoMapeado = mapeo.Value;
                            columna.DescripcionMapeado = campoInfo.Dato;
                            columna.MapeadoAutomatico = false;
                            columna.ConfianzaMapeo = 100; // Mapeo manual = 100% confianza

                            _logger?.LogInformation($"Mapeo manual aplicado: '{columna.Encabezado}' → '{campoInfo.Campo}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "Error aplicando mapeo manual");
            }
        }

        // ✅ NUEVO: Procesar datos reales del Excel
        // ✅ ACTUALIZAR: Procesar datos del Excel con campo BD
        private async Task<DatosImportacionDto?> ProcesarDatosDelExcel(IFormFile archivo, AnalisisExcelDto analisis, string proveedorId)
        {
            try
            {
                using var stream = new MemoryStream();
                await archivo.CopyToAsync(stream);

                ExcelPackage.License.SetNonCommercialPersonal("Geconet");

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                // ✅ Verificar que hay mapeos válidos
                var columnasMapadas = analisis.Columnas?.Where(c => !string.IsNullOrEmpty(c.CampoMapeado)).ToList();
                if (columnasMapadas == null || !columnasMapadas.Any())
                {
                    _logger?.LogWarning("No hay columnas mapeadas para procesar");
                    return null;
                }

                var datosImportacion = new DatosImportacionDto
                {
                    ProveedorId = proveedorId,
                    NombreArchivo = archivo.FileName,
                    TotalFilas = analisis.TotalFilas,
                    TotalColumnas = analisis.TotalColumnas,
                    FilaEncabezados = 1,
                    FechaProceso = DateTime.Now,
                    MapeoColumnas = columnasMapadas.Select(c => new MapeoColumnaDto
                    {
                        IndiceColumna = c.Indice,
                        LetraColumna = c.Letra,
                        EncabezadoOriginal = c.Encabezado,
                        CampoBD = c.CampoMapeado,
                        DescripcionCampo = c.DescripcionMapeado,
                        TipoDato = c.TipoDetectado,
                        ConfianzaMapeo = c.ConfianzaMapeo,
                        MapeadoAutomatico = c.MapeadoAutomatico
                    }).ToList()
                };

                // ✅ Procesar cada fila de datos (saltar fila de encabezados)
                int filaInicioDatos = datosImportacion.FilaEncabezados + 1;
                int filasProceadas = 0;

                for (int fila = filaInicioDatos; fila <= analisis.TotalFilas; fila++)
                {
                    var filaDatos = new FilaDatosDto
                    {
                        NumeroFila = fila
                    };

                    bool filaConDatos = false;

                    // ✅ Procesar cada columna mapeada
                    foreach (var mapeo in datosImportacion.MapeoColumnas)
                    {
                        var valorCelda = worksheet.Cells[fila, mapeo.IndiceColumna].Value;

                        // ✅ CORREGIR: Pasar el campo BD para tratamiento especial
                        var valorProcesado = ProcesarValorCelda(valorCelda, mapeo.TipoDato, mapeo.CampoBD);

                        filaDatos.Valores[mapeo.CampoBD] = valorProcesado;

                        // ✅ Verificar si la fila tiene datos útiles
                        if (valorProcesado != null && !string.IsNullOrWhiteSpace(valorProcesado.ToString()))
                        {
                            filaConDatos = true;
                        }

                        // ✅ LOGGING: Para campos identificadores
                        if (EsCampoIdentificador(mapeo.CampoBD) && valorProcesado != null)
                        {
                            _logger?.LogDebug($"Campo identificador procesado: {mapeo.CampoBD} = '{valorProcesado}' (Tipo: {valorProcesado.GetType().Name})");
                        }
                    }

                    // ✅ Solo agregar filas que tengan al menos un dato útil
                    if (filaConDatos)
                    {
                        datosImportacion.Filas.Add(filaDatos);
                        filasProceadas++;
                    }
                }

                _logger?.LogInformation($"Procesadas {filasProceadas} filas de datos con {datosImportacion.MapeoColumnas.Count} columnas mapeadas");

                return datosImportacion;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error procesando datos del Excel");
                return null;
            }
        }
        // ✅ NUEVO: Procesar valor de celda según tipo
        // ✅ CORREGIR: Procesar valor de celda con tratamiento especial para EAN
        private object? ProcesarValorCelda(object? valorCelda, string tipoDato, string? campoBD = null)
        {
            if (valorCelda == null) return null;

            var valorTexto = valorCelda.ToString()?.Trim();
            if (string.IsNullOrEmpty(valorTexto)) return null;

            // ✅ ESPECIAL: Campos que SIEMPRE deben ser string (identificadores)
            if (!string.IsNullOrEmpty(campoBD) && EsCampoIdentificador(campoBD))
            {
                return valorTexto; // Mantener como string sin conversión
            }

            return tipoDato switch
            {
                "Número" => double.TryParse(valorTexto, NumberStyles.Any, CultureInfo.InvariantCulture, out var numero) ? numero : null,
                "Fecha" => DateTime.TryParse(valorTexto, out var fecha) ? fecha : null,
                "Texto" => valorTexto,
                _ => valorTexto
            };
        }

        // ✅ NUEVO: Identificar campos que deben mantenerse como string
        private bool EsCampoIdentificador(string campoBD)
        {
            var camposIdentificadores = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "p_ean",
                "p_ean_otro",
                "p_dun",
                "p_id",
                "p_id_prov",
                "p_codigo"
            };

            return camposIdentificadores.Contains(campoBD);
        }

        // ✅ NUEVO: Endpoint para verificar datos de importación
        [HttpPost]
        public JsonResult VerificarDatosImportacion()
        {
            try
            {
                _logger?.LogInformation("=== VERIFICANDO DATOS DE IMPORTACIÓN ===");

                var resultado = new
                {
                    cantidadCampos = DatosParaImportacion.Count,
                    campos = DatosParaImportacion.Select(c => new
                    {
                        campo = c.Campo,
                        codigo = c.Dato,
                        tipo = c.Tipo.ToString(),
                        limpio = LimpiarTexto(c.Campo)
                    }).ToList(),
                    tieneEAN = DatosParaImportacion.Any(c =>
                        c.Campo.Contains("EAN", StringComparison.OrdinalIgnoreCase) ||
                        c.Dato.Contains("p_ean", StringComparison.OrdinalIgnoreCase)),
                    camprosEAN = DatosParaImportacion.Where(c =>
                        c.Campo.Contains("EAN", StringComparison.OrdinalIgnoreCase) ||
                        c.Dato.Contains("p_ean", StringComparison.OrdinalIgnoreCase)).ToList()
                };

                _logger?.LogInformation($"Cantidad de campos: {resultado.cantidadCampos}");
                _logger?.LogInformation($"Tiene campos EAN: {resultado.tieneEAN}");

                return Json(new { error = false, datos = resultado });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verificando datos de importación");
                return Json(new { error = true, mensaje = ex.Message });
            }
        }

        // ✅ NUEVO: Endpoint de diagnóstico para celdas combinadas
        [HttpPost]
        public JsonResult DiagnosticarCeldasCombinadas(IFormFile archivo)
        {
            try
            {
                if (archivo == null || archivo.Length == 0)
                {
                    return Json(new { error = true, mensaje = "No se recibió archivo" });
                }

                using var stream = new MemoryStream();
                archivo.CopyTo(stream);

                ExcelPackage.License.SetNonCommercialPersonal("Geconet");

                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];

                var celdasCombinadas = ObtenerInformacionCeldasCombinadas(worksheet);

                var diagnostico = new
                {
                    nombreArchivo = archivo.FileName,
                    nombreHoja = worksheet.Name,
                    totalFilas = worksheet.Dimension?.End.Row ?? 0,
                    totalColumnas = worksheet.Dimension?.End.Column ?? 0,
                    cantidadCeldasCombinadas = celdasCombinadas.Count,
                    celdasCombinadas = celdasCombinadas.Select(cc => new
                    {
                        rango = $"{GetColumnName(cc.ColumnaInicio)}{cc.FilaInicio}:{GetColumnName(cc.ColumnaFin)}{cc.FilaFin}",
                        valor = cc.Valor,
                        filas = $"{cc.FilaInicio}-{cc.FilaFin}",
                        columnas = $"{cc.ColumnaInicio}-{cc.ColumnaFin}"
                    }).ToList(),

                    // ✅ Análisis de impacto en encabezados
                    impactoEncabezados = AnalizarImpactoEnEncabezados(worksheet, celdasCombinadas)
                };

                _logger?.LogInformation($"Diagnóstico de celdas combinadas: {celdasCombinadas.Count} encontradas");

                return Json(new { error = false, diagnostico = diagnostico });
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error diagnosticando celdas combinadas");
                return Json(new { error = true, mensaje = ex.Message });
            }
        }

        // ✅ NUEVO: Analizar impacto de celdas combinadas en encabezados
        private object AnalizarImpactoEnEncabezados(ExcelWorksheet worksheet, List<CeldaCombinada> celdasCombinadas)
        {
            var impacto = new
            {
                filasAfectadas = celdasCombinadas.SelectMany(cc =>
                    Enumerable.Range(cc.FilaInicio, cc.FilaFin - cc.FilaInicio + 1)).Distinct().OrderBy(f => f).ToList(),

                columnasAfectadas = celdasCombinadas.SelectMany(cc =>
                    Enumerable.Range(cc.ColumnaInicio, cc.ColumnaFin - cc.ColumnaInicio + 1)).Distinct().OrderBy(c => c).ToList(),

                posiblesEncabezadosPerdidos = celdasCombinadas.Where(cc =>
                    !string.IsNullOrEmpty(cc.Valor) && cc.FilaFin > cc.FilaInicio).Select(cc => new
                    {
                        valor = cc.Valor,
                        filaConValor = cc.FilaInicio,
                        filasVacias = Enumerable.Range(cc.FilaInicio + 1, cc.FilaFin - cc.FilaInicio).ToList()
                    }).ToList()
            };

            return impacto;
        }

        // ✅ MEJORAR: Búsqueda por palabras clave más flexible
        private int BuscarPorPalabrasClave(string encabezado, string campoBD, bool debug = false)
        {
            var palabrasEspecificas = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["p_ean"] = [
                    // ✅ EXPANDIDO: Más variaciones de EAN
                    "ean", "ean13", "ean 13", "ean2", "ean 2", "ean8", "ean 8",
            "barcode", "codigo barras", "cod barras", "barras",
            "gtin", "gtin13", "upc", "isbn"
                ],
                ["p_codigo"] = [
                    "cod", "codigo", "código", "code", "item", "art", "articulo", "artículo",
            "cod art", "codigo art", "codigo articulo", "ref", "referencia",
            "id", "identificador", "sku", "part number"
                ],
                ["p_desc"] = [
                    "desc", "descripcion", "descripción", "nombre", "producto",
            "detalle", "denominacion", "denominación", "titulo", "título",
            "description", "name", "product"
                ],
                ["p_plista"] = [
                    "precio", "price", "valor", "importe", "monto",
            "precio lista", "precio_lista", "preciolista", "lista",
            "pvp", "precio venta", "venta", "$", "pesos"
                ],
                ["p_marca"] = [
                    "marca", "brand", "fabricante", "laboratorio", "lab",
            "manufacturer", "make", "company"
                ],
                ["p_costo"] = [
                    "costo", "cost", "precio compra", "precio_compra", "compra",
            "coste", "cost price", "wholesale"
                ]
            };

            if (!palabrasEspecificas.TryGetValue(campoBD, out var palabras))
                return 0;

            int mejorCoincidencia = 0;
            string mejorPalabra = "";

            // ✅ BUSCAR: Coincidencias exactas primero
            foreach (var palabra in palabras)
            {
                if (encabezado.Equals(palabra, StringComparison.OrdinalIgnoreCase))
                {
                    mejorCoincidencia = 90;
                    mejorPalabra = palabra;
                    break;
                }
            }

            // ✅ BUSCAR: Coincidencias parciales
            if (mejorCoincidencia == 0)
            {
                foreach (var palabra in palabras)
                {
                    if (encabezado.Contains(palabra, StringComparison.OrdinalIgnoreCase))
                    {
                        // ✅ BONUS: Por longitud de la palabra encontrada
                        var factorLongitud = Math.Min(1.0, palabra.Length / (double)encabezado.Length);
                        var confianza = (int)(60 + (factorLongitud * 20));

                        if (confianza > mejorCoincidencia)
                        {
                            mejorCoincidencia = confianza;
                            mejorPalabra = palabra;
                        }
                    }
                }
            }

            // ✅ BUSCAR: Similitud aproximada con palabras clave
            if (mejorCoincidencia == 0)
            {
                foreach (var palabra in palabras)
                {
                    var similitud = SimilitudTexto.PorcentajeSimilitud(encabezado, palabra);
                    if (similitud >= 70)
                    {
                        var confianza = (int)(40 + (similitud * 0.3)); // 40-70% según similitud

                        if (confianza > mejorCoincidencia)
                        {
                            mejorCoincidencia = confianza;
                            mejorPalabra = palabra;
                        }
                    }
                }
            }

            if (debug && mejorCoincidencia > 0)
                _logger?.LogDebug($"→ Palabra clave encontrada: '{mejorPalabra}' → {mejorCoincidencia}%");

            return mejorCoincidencia;
        }

        // ✅ OPTIMIZAR: Función de limpieza más inteligente
        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            // ✅ PRESERVAR: Números que pueden ser importantes (EAN 13, EAN 2)
            var resultado = texto.Trim()
                                 .ToLowerInvariant()
                                 .Replace("_", " ")
                                 .Replace("-", " ")
                                 .Replace(".", " ")
                                 .Replace("\t", " ")
                                 .Replace("\n", " ")
                                 .Replace("\r", " ")
                                 .Replace("/", " ")
                                 .Replace("\\", " ");

            // ✅ NORMALIZAR: Acentos y caracteres especiales
            resultado = resultado.Replace("á", "a").Replace("é", "e").Replace("í", "i")
                                .Replace("ó", "o").Replace("ú", "u").Replace("ñ", "n");

            // ✅ MANTENER: Solo letras, números y espacios
            var chars = resultado.Where(c => char.IsLetterOrDigit(c) || c == ' ').ToArray();
            resultado = new string(chars);

            // ✅ NORMALIZAR: Espacios múltiples
            while (resultado.Contains("  "))
            {
                resultado = resultado.Replace("  ", " ");
            }

            return resultado.Trim();
        }

        private bool VerificarCompatibilidadTipo(string tipoDetectado, char tipoCampo)
        {
            return tipoDetectado switch
            {
                "Número" => tipoCampo == 'N',
                "Texto" => tipoCampo == 'T',
                "Fecha" => tipoCampo == 'D',
                _ => false
            };
        }

        // ✅ NUEVO: Método para diagnosticar mapeos (útil para debugging)
        private void DiagnosticarMapeos(AnalisisExcelDto analisis)
        {
            if (_logger == null || !_logger.IsEnabled(LogLevel.Information)) return;

            _logger.LogInformation("=== DIAGNÓSTICO DE MAPEOS ===");

            var agrupados = analisis.Columnas
                .Where(c => !string.IsNullOrEmpty(c.CampoMapeado))
                .GroupBy(c => c.CampoMapeado)
                .Where(g => g.Count() > 1); // Solo conflictos

            foreach (var grupo in agrupados)
            {
                _logger.LogWarning($"⚠️ CONFLICTO: Campo BD '{grupo.Key}' mapeado a múltiples columnas:");
                foreach (var columna in grupo)
                {
                    _logger.LogWarning($"  - '{columna.Encabezado}' (Confianza: {columna.ConfianzaMapeo}%)");
                }
            }

            var noMapeados = analisis.Columnas.Count(c => string.IsNullOrEmpty(c.CampoMapeado));
            var mapeados = analisis.Columnas.Count - noMapeados;

            _logger.LogInformation($"📊 Resumen: {mapeados} mapeados, {noMapeados} sin mapear de {analisis.Columnas.Count} total");
        }

        // ✅ SIMPLIFICAR: Mapeo automático optimizado
        private void AplicarMapeoAutomaticoInteligente(AnalisisExcelDto analisis)
        {
            _logger?.LogInformation($"Iniciando mapeo automático: {analisis.Columnas?.Count} columnas");

            if (analisis.Columnas == null || !analisis.Columnas.Any())
                return;

            foreach (var columna in analisis.Columnas)
            {
                var mapeoEncontrado = BuscarMejorMapeo(columna, analisis.CamposDisponibles, debug: true);

                if (mapeoEncontrado != null)
                {
                    columna.CampoMapeado = mapeoEncontrado.Campo;
                    columna.DescripcionMapeado = mapeoEncontrado.Dato;
                    columna.MapeadoAutomatico = true;

                    _logger?.LogInformation($"✅ Mapeado: '{columna.Encabezado}' → '{mapeoEncontrado.Campo}' ({columna.ConfianzaMapeo}%)");
                }
            }

            DiagnosticarMapeos(analisis);
            ResolverConflictosMapeo(analisis);
        }

        // ✅ MEJORAR: Función de cálculo de confianza con similitud aproximada
        private int CalcularConfianza(string encabezado, PrecioFileDatos campo, string tipoDetectado, bool debug = false)
        {
            var campoLimpio = LimpiarTexto(campo.Campo);
            var campoBD = campo.Dato.ToLowerInvariant();

            if (debug)
                _logger?.LogDebug($"Comparando '{encabezado}' vs BD:'{campoBD}' Campo:'{campoLimpio}'");

            // CRITERIO 1: Coincidencia exacta (100%)
            if (encabezado.Equals(campoLimpio, StringComparison.OrdinalIgnoreCase) ||
                encabezado.Equals(campoBD, StringComparison.OrdinalIgnoreCase))
            {
                if (debug) _logger?.LogDebug($"→ Coincidencia exacta: 100%");
                return 100;
            }

            // ✅ NUEVO: CRITERIO 2: Similitud aproximada inteligente
            var confianzaSimilitud = CalcularSimilitudAproximada(encabezado, campo, debug);
            if (confianzaSimilitud >= 70)
            {
                if (debug) _logger?.LogDebug($"→ Similitud aproximada alta: {confianzaSimilitud}%");
                return confianzaSimilitud;
            }

            // CRITERIO 3: Coincidencia parcial (mejorada)
            var confianzaParcial = CalcularCoincidenciaParcial(encabezado, campoLimpio, debug);

            // CRITERIO 4: Búsqueda por palabras clave específicas
            var confianzaPorPalabra = BuscarPorPalabrasClave(encabezado, campoBD, debug);

            // CRITERIO 5: Similitud semántica por contexto
            //var confianzaSemantica = CalcularSimilitudSemantica(encabezado, campo, debug);
            var confianzaSemantica = 0;
            // ✅ COMBINAR: Tomar la mejor confianza de los criterios
            var mejorConfianza = Math.Max(confianzaSimilitud,
                                 Math.Max(confianzaParcial,
                                 Math.Max(confianzaPorPalabra, confianzaSemantica)));

            // CRITERIO 6: Bonus por compatibilidad de tipos
            var bonusTipo = VerificarCompatibilidadTipo(tipoDetectado, campo.Tipo) ? 10 : -5;
            var confianzaFinal = mejorConfianza + bonusTipo;

            if (debug && mejorConfianza > 0)
                _logger?.LogDebug($"→ Mejor confianza: {mejorConfianza}%, Final con tipo: {confianzaFinal}%");

            return Math.Max(0, Math.Min(confianzaFinal, 100));
        }

        // ✅ NUEVO: Calcular similitud aproximada inteligente
        private int CalcularSimilitudAproximada(string encabezado, PrecioFileDatos campo, bool debug = false)
        {
            var campoLimpio = LimpiarTexto(campo.Campo);
            var campoBD = campo.Dato.ToLowerInvariant();

            // ✅ CASO ESPECIAL: Extraer términos clave de códigos de BD
            var terminosClaveBD = ExtraerTerminosClave(campoBD);
            var terminosClaveEncabezado = ExtraerTerminosClave(encabezado);

            int mejorSimilitud = 0;

            // ✅ COMPARAR: Cada término clave del encabezado con términos del campo BD
            foreach (var terminoEncabezado in terminosClaveEncabezado)
            {
                foreach (var terminoBD in terminosClaveBD)
                {
                    // Similitud de Levenshtein
                    var simLevenshtein = SimilitudTexto.PorcentajeSimilitud(terminoEncabezado, terminoBD);

                    // Similitud Jaccard (bigramas)
                    var simJaccard = SimilitudTexto.SimilitudJaccard(terminoEncabezado, terminoBD, 2) * 100;

                    // Similitud fonética (para casos como EAN/ENA)
                    var simFonetica = SimilitudTexto.SimilitudFonetica(terminoEncabezado, terminoBD);

                    // ✅ TOMAR: La mejor similitud
                    var similitudMaxima = Math.Max(simLevenshtein, Math.Max(simJaccard, simFonetica));

                    if (similitudMaxima > mejorSimilitud)
                    {
                        mejorSimilitud = (int)similitudMaxima;

                        if (debug && similitudMaxima >= 70)
                        {
                            _logger?.LogDebug($"  Alta similitud: '{terminoEncabezado}' ≈ '{terminoBD}' " +
                                            $"(Lev:{simLevenshtein:F1}%, Jac:{simJaccard:F1}%, Fon:{simFonetica:F1}%)");
                        }
                    }
                }
            }

            // ✅ BONUS: Para similitudes muy altas
            if (mejorSimilitud >= 90)
            {
                mejorSimilitud = Math.Min(95, mejorSimilitud + 5);
            }
            else if (mejorSimilitud >= 80)
            {
                mejorSimilitud = Math.Min(90, mejorSimilitud + 3);
            }

            return mejorSimilitud;
        }

        // ✅ NUEVO: Extraer términos clave de un texto
        private List<string> ExtraerTerminosClave(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return new List<string>();

            var terminos = new List<string>();

            // ✅ EXTRAER: Términos de códigos de BD (p_ean → ean)
            if (texto.StartsWith("p_"))
            {
                var terminoPrincipal = texto.Substring(2); // Quitar "p_"
                terminos.Add(terminoPrincipal);

                // Agregar variaciones comunes
                if (terminoPrincipal == "ean")
                {
                    terminos.AddRange(new[] { "ean", "ena", "an", "barcode", "gtin" });
                }
                else if (terminoPrincipal == "codigo")
                {
                    terminos.AddRange(new[] { "codigo", "cod", "code", "item", "art" });
                }
                else if (terminoPrincipal == "desc")
                {
                    terminos.AddRange(new[] { "desc", "descripcion", "nombre", "producto" });
                }
            }
            else
            {
                // ✅ DIVIDIR: Por espacios y caracteres especiales
                var palabras = texto.ToLowerInvariant()
                                   .Split(new[] { ' ', '_', '-', '.', '/', '\\' },
                                         StringSplitOptions.RemoveEmptyEntries);

                terminos.AddRange(palabras);

                // ✅ AGREGAR: El texto completo sin espacios
                var textoSinEspacios = string.Concat(palabras);
                if (textoSinEspacios.Length > 2)
                {
                    terminos.Add(textoSinEspacios);
                }
            }

            return terminos.Where(t => t.Length >= 2).Distinct().ToList();
        }

        // ✅ NUEVO: Calcular coincidencia parcial mejorada
        private int CalcularCoincidenciaParcial(string encabezado, string campoLimpio, bool debug = false)
        {
            if (string.IsNullOrEmpty(encabezado) || string.IsNullOrEmpty(campoLimpio))
                return 0;

            // ✅ VERIFICAR: Contiene uno al otro
            bool encabezadoContieneCampo = encabezado.Contains(campoLimpio, StringComparison.OrdinalIgnoreCase);
            bool campoContieneEncabezado = campoLimpio.Contains(encabezado, StringComparison.OrdinalIgnoreCase);

            if (encabezadoContieneCampo || campoContieneEncabezado)
            {
                var factor = Math.Min(encabezado.Length, campoLimpio.Length) /
                            (double)Math.Max(encabezado.Length, campoLimpio.Length);
                var confianza = (int)(75 * factor); // Reducido de 80 para dar prioridad a similitud aproximada

                if (debug)
                    _logger?.LogDebug($"→ Coincidencia parcial (factor {factor:F2}): {confianza}%");

                return confianza;
            }

            // ✅ VERIFICAR: Coincidencia de palabras individuales
            var palabrasEncabezado = encabezado.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var palabrasCampo = campoLimpio.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var coincidencias = palabrasEncabezado.Count(pe =>
                palabrasCampo.Any(pc => pc.Equals(pe, StringComparison.OrdinalIgnoreCase)));

            if (coincidencias > 0)
            {
                var factorCoincidencias = (double)coincidencias / Math.Max(palabrasEncabezado.Length, palabrasCampo.Length);
                var confianza = (int)(60 * factorCoincidencias);

                if (debug)
                    _logger?.LogDebug($"→ Coincidencia de palabras ({coincidencias}/{Math.Max(palabrasEncabezado.Length, palabrasCampo.Length)}): {confianza}%");

                return confianza;
            }

            return 0;
        }

        // ✅ NUEVO: Similitud semántica por contexto
        private int CalcularSimilitudSemantica(string encabezado, PrecioFileDatos campo, bool debug = false)
        {
            // ✅ MAPEO: Contextos semánticos
            var contextosSemanticos = new Dictionary<string, string[]>
            {
                ["identificacion"] = ["ean", "codigo", "barcode", "gtin", "id", "item", "art"],
                ["descripcion"] = ["desc", "nombre", "producto", "detalle", "denominacion"],
                ["precio"] = ["precio", "price", "valor", "importe", "monto", "lista"],
                ["marca"] = ["marca", "brand", "fabricante", "laboratorio"],
                ["costo"] = ["costo", "cost", "compra"]
            };

            var campoBD = campo.Dato.ToLowerInvariant();
            var encabezadoLimpio = encabezado.ToLowerInvariant();

            foreach (var contexto in contextosSemanticos)
            {
                // ✅ VERIFICAR: Si el campo BD pertenece a este contexto
                bool campoPerteneceContexto = contexto.Value.Any(term => campoBD.Contains(term));

                if (campoPerteneceContexto)
                {
                    // ✅ VERIFICAR: Si el encabezado también pertenece al mismo contexto
                    var coincidenciasContexto = contexto.Value.Count(term =>
                        encabezadoLimpio.Contains(term));

                    if (coincidenciasContexto > 0)
                    {
                        var confianza = Math.Min(70, 50 + (coincidenciasContexto * 10));

                        if (debug)
                            _logger?.LogDebug($"→ Similitud semántica '{contexto.Key}': {confianza}%");

                        return confianza;
                    }
                }
            }

            return 0;
        }

        // ✅ OPTIMIZAR: Una sola función de búsqueda de mapeo
        private PrecioFileDatos? BuscarMejorMapeo(ColumnaExcelDto columna, List<PrecioFileDatos> camposDisponibles, bool debug = false)
        {
            var encabezadoLimpio = LimpiarTexto(columna.Encabezado);

            if (debug)
                _logger?.LogDebug($"Encabezado limpio: '{columna.Encabezado}' → '{encabezadoLimpio}'");

            if (string.IsNullOrEmpty(encabezadoLimpio))
            {
                if (debug) _logger?.LogWarning($"Encabezado vacío: '{columna.Encabezado}'");
                return null;
            }

            PrecioFileDatos? mejorMapeo = null;
            int mayorConfianza = 0;
            ////se armara un diccionario con la equivalencia de existente en camposDisponibles entre campo y dato
            //var camposDbDict = camposDisponibles
            //                        .Where(x => !x.HasChecked)
            //                        .ToDictionary(c => c.Campo, c => c.Dato, StringComparer.OrdinalIgnoreCase);

            foreach (var campo in camposDisponibles.Where(x => !x.HasChecked))
            {
                var confianza = CalcularConfianza(encabezadoLimpio, campo, columna.TipoDetectado, debug);

                if (confianza > mayorConfianza && confianza >= 50)
                {
                    mayorConfianza = confianza;
                    mejorMapeo = campo;
                    columna.ConfianzaMapeo = confianza;
                    campo.HasChecked = true;
                    break;
                }
            }

            return mejorMapeo;
        }

        // ✅ NUEVA: Resolver conflictos de mapeo automáticamente
        private void ResolverConflictosMapeo(AnalisisExcelDto analisis)
        {
            var conflictos = analisis.Columnas
                .Where(c => !string.IsNullOrEmpty(c.CampoMapeado))
                .GroupBy(c => c.CampoMapeado)
                .Where(g => g.Count() > 1)
                .ToList();

            foreach (var conflicto in conflictos)
            {
                // Mantener solo el de mayor confianza
                var mejorColumna = conflicto.OrderByDescending(c => c.ConfianzaMapeo).First();

                foreach (var columna in conflicto.Where(c => c != mejorColumna))
                {
                    _logger?.LogWarning($"🔧 Resolviendo conflicto: Removiendo mapeo '{columna.Encabezado}' → '{conflicto.Key}' " +
                                     $"(Confianza: {columna.ConfianzaMapeo}% < {mejorColumna.ConfianzaMapeo}%)");

                    columna.CampoMapeado = string.Empty;
                    columna.DescripcionMapeado = string.Empty;
                    columna.MapeadoAutomatico = false;
                    columna.ConfianzaMapeo = 0;
                }
            }
        }

        // ✅ OPTIMIZAR: Análisis de estructura Excel con manejo de celdas combinadas
        // ✅ ACTUALIZAR: Análisis de estructura Excel con mejor detección
        private async Task<AnalisisExcelDto> AnalizarEstructuraExcel(IFormFile archivo)
        {
            using var stream = new MemoryStream();
            await archivo.CopyToAsync(stream);

            ExcelPackage.License.SetNonCommercialPersonal("Geconet");

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            var analisis = new AnalisisExcelDto
            {
                NombreArchivo = archivo.FileName,
                NombreHoja = worksheet.Name,
                TotalFilas = worksheet.Dimension?.End.Row ?? 0,
                TotalColumnas = worksheet.Dimension?.End.Column ?? 0,
                Columnas = new List<ColumnaExcelDto>()
            };

            if (analisis.TotalFilas == 0 || analisis.TotalColumnas == 0)
                return analisis;

            var celdasCombinadas = ObtenerInformacionCeldasCombinadas(worksheet);
            var filaEncabezados = DetectarFilaEncabezadosConCombinadas(worksheet, celdasCombinadas, 10, 2);
            var filaDatosInicio = Math.Min(analisis.TotalFilas, filaEncabezados + 1);
            var totalFilasDatos = Math.Max(0, analisis.TotalFilas - filaEncabezados);

            _logger?.LogInformation($"Fila encabezados detectada: {filaEncabezados}, Celdas combinadas: {celdasCombinadas.Count}");

            for (int col = 1; col <= analisis.TotalColumnas; col++)
            {
                var valorEncabezado = ObtenerValorEncabezadoConCombinadas(worksheet, filaEncabezados, col, celdasCombinadas);

                if (string.IsNullOrEmpty(valorEncabezado))
                {
                    valorEncabezado = $"Columna {GetColumnName(col)}";
                }

                // ✅ MEJORAR: Pasar encabezado para mejor detección
                var tipoDetectado = DetectarTipoDato(worksheet, col, filaDatosInicio, Math.Min(10, totalFilasDatos), valorEncabezado);
                var valoresNoVacios = ContarValoresNoVacios(worksheet, col, filaDatosInicio, analisis.TotalFilas);

                analisis.Columnas.Add(new ColumnaExcelDto
                {
                    Indice = col,
                    Letra = GetColumnName(col),
                    Encabezado = valorEncabezado,
                    TipoDetectado = tipoDetectado,
                    ValoresNoVacios = valoresNoVacios,
                    PorcentajeLlenado = totalFilasDatos > 0 ? Math.Round((double)valoresNoVacios / totalFilasDatos * 100, 1) : 0,
                    EjemplosValores = ObtenerEjemplosValores(worksheet, col, 3, filaDatosInicio)
                });

                _logger?.LogDebug($"Columna {col} ({GetColumnName(col)}): '{valorEncabezado}' - Tipo: {tipoDetectado}");
            }

            return analisis;
        }


        // ✅ NUEVO: Obtener información de todas las celdas combinadas
        private List<CeldaCombinada> ObtenerInformacionCeldasCombinadas(ExcelWorksheet worksheet)
        {
            var celdasCombinadas = new List<CeldaCombinada>();

            if (worksheet.MergedCells == null || worksheet.MergedCells.Count == 0)
                return celdasCombinadas;

            foreach (var mergedCell in worksheet.MergedCells)
            {
                try
                {
                    var address = new ExcelAddress(mergedCell);
                    var valorCelda = worksheet.Cells[address.Start.Row, address.Start.Column].Value?.ToString()?.Trim() ?? string.Empty;

                    celdasCombinadas.Add(new CeldaCombinada
                    {
                        FilaInicio = address.Start.Row,
                        FilaFin = address.End.Row,
                        ColumnaInicio = address.Start.Column,
                        ColumnaFin = address.End.Column,
                        Valor = valorCelda,
                        Direccion = address
                    });

                    _logger?.LogDebug($"Celda combinada detectada: {mergedCell} = '{valorCelda}'");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning($"Error procesando celda combinada {mergedCell}: {ex.Message}");
                }
            }

            return celdasCombinadas.OrderBy(c => c.FilaInicio).ThenBy(c => c.ColumnaInicio).ToList();
        }

        // ✅ NUEVO: Obtener valor de encabezado considerando celdas combinadas
        private string ObtenerValorEncabezadoConCombinadas(ExcelWorksheet worksheet, int fila, int columna, List<CeldaCombinada> celdasCombinadas)
        {
            // 1. Intentar obtener valor directo de la celda
            var valorDirecto = worksheet.Cells[fila, columna].Value?.ToString()?.Trim();

            if (!string.IsNullOrEmpty(valorDirecto))
            {
                _logger?.LogDebug($"Valor directo en [{fila},{columna}]: '{valorDirecto}'");
                return valorDirecto;
            }

            // 2. Buscar si esta celda está dentro de una combinación
            var celdaCombinada = celdasCombinadas.FirstOrDefault(cc =>
                fila >= cc.FilaInicio && fila <= cc.FilaFin &&
                columna >= cc.ColumnaInicio && columna <= cc.ColumnaFin);

            if (celdaCombinada != null && !string.IsNullOrEmpty(celdaCombinada.Valor))
            {
                _logger?.LogDebug($"Valor desde celda combinada [{fila},{columna}]: '{celdaCombinada.Valor}'");
                return celdaCombinada.Valor;
            }

            // 3. Buscar en filas superiores (hasta 3 filas arriba)
            for (int filaSuperior = Math.Max(1, fila - 3); filaSuperior < fila; filaSuperior++)
            {
                var valorSuperior = worksheet.Cells[filaSuperior, columna].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(valorSuperior))
                {
                    // Verificar si esta celda superior es parte de una combinación que incluye nuestra fila objetivo
                    var combinacionSuperior = celdasCombinadas.FirstOrDefault(cc =>
                        filaSuperior >= cc.FilaInicio && filaSuperior <= cc.FilaFin &&
                        columna >= cc.ColumnaInicio && columna <= cc.ColumnaFin &&
                        fila >= cc.FilaInicio && fila <= cc.FilaFin);

                    if (combinacionSuperior != null)
                    {
                        _logger?.LogDebug($"Valor desde fila superior combinada [{fila},{columna}]: '{valorSuperior}'");
                        return valorSuperior;
                    }
                }
            }

            _logger?.LogDebug($"Sin valor encontrado para [{fila},{columna}]");
            return string.Empty;
        }

        // --------------------------------------------------------------------
        // FUNCIÓN DE ANÁLISIS: Detecta la fila de cabeceras (ignora títulos)
        // --------------------------------------------------------------------
        // ✅ MEJORAR: Detección de encabezados considerando celdas combinadas
        private int DetectarFilaEncabezadosConCombinadas(ExcelWorksheet ws, List<CeldaCombinada> celdasCombinadas,
            int maxFilasExploracion = 10, int minFilasDatos = 2)
        {
            int totalFilas = ws.Dimension?.End.Row ?? 0;
            int totalCols = ws.Dimension?.End.Column ?? 0;
            if (totalFilas == 0 || totalCols == 0) return 1;

            int maxRow = Math.Min(maxFilasExploracion, totalFilas);

            // ✅ NUEVO: Lista de tokens de headers ampliada
            var headerTokens = new HashSet<string>(new[]
            {
        "id","codigo","código","item","cliente","nombre","apellido","razon social","razón social",
        "direccion","dirección","domicilio","ciudad","provincia","pais","país","cp","c.p.",
        "email","correo","telefono","teléfono","movil","móvil","marca","ean","con iva","sin iva","dun",
        "fecha","fec","año","mes","dia","día","hora","bonificación","sku","barcode","gtin",
        "monto","importe","total","cantidad","precio","costo","neto","bruto","iva","tax","amount","qty","price",
        "estado","status","tipo","categoria","categoría","descripcion","descripción","obs","observaciones",
        "usuario","user","lote","vencimiento","stock","existencia","medida","unidad"
    }.Select(t => t.ToLowerInvariant()));

            // Helpers locales
            bool EsVacia(ExcelRangeBase cell) => string.IsNullOrWhiteSpace(cell?.Text);
            bool EsNumero(ExcelRangeBase cell) => cell?.Value != null &&
                (cell.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal ||
                 double.TryParse(cell.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out _));
            bool PareceFecha(ExcelRangeBase cell) => cell?.Value is DateTime ||
                DateTime.TryParse(cell?.Text?.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.None, out _);
            bool EsTexto(ExcelRangeBase cell) => cell?.Value != null && !EsNumero(cell) && !PareceFecha(cell) && !string.IsNullOrWhiteSpace(cell.Text);

            double mejorScore = double.NegativeInfinity;
            int mejorFila = 1;

            for (int r = 1; r <= maxRow; r++)
            {
                var scoreFilaConCombinadas = EvaluarFilaComoEncabezadoConCombinadas(
                    ws, r, totalCols, celdasCombinadas, headerTokens, EsVacia, EsNumero, PareceFecha, EsTexto);

                // Verificar continuidad de datos
                if (!HayContinuidadDatos(ws, r, minFilasDatos, EsVacia))
                {
                    scoreFilaConCombinadas.Score = 0; // Descalificar
                }

                if (scoreFilaConCombinadas.Score > mejorScore)
                {
                    mejorScore = scoreFilaConCombinadas.Score;
                    mejorFila = r;

                    _logger?.LogDebug($"Nueva mejor fila {r}: Score {scoreFilaConCombinadas.Score:F3}, " +
                                   $"Cobertura: {scoreFilaConCombinadas.Cobertura:F2}, " +
                                   $"Tokens: {scoreFilaConCombinadas.TokensEncontrados}");
                }
            }

            if (mejorScore == double.NegativeInfinity)
            {
                _logger?.LogWarning("No se encontró fila de encabezados válida, usando fila 1 por defecto");
                return 1;
            }

            _logger?.LogInformation($"Fila de encabezados seleccionada: {mejorFila} (Score: {mejorScore:F3})");
            return mejorFila;
        }

        // ✅ NUEVO: Evaluar fila como encabezado considerando celdas combinadas
        private (double Score, double Cobertura, int TokensEncontrados) EvaluarFilaComoEncabezadoConCombinadas(
            ExcelWorksheet ws, int fila, int totalCols, List<CeldaCombinada> celdasCombinadas,
            HashSet<string> headerTokens,
            Func<ExcelRangeBase, bool> esVacia,
            Func<ExcelRangeBase, bool> esNumero,
            Func<ExcelRangeBase, bool> pareceFecha,
            Func<ExcelRangeBase, bool> esTexto)
        {
            int nonEmpty = 0, cntTexto = 0, tokensEncontrados = 0;
            var valoresDistinct = new HashSet<string>();

            for (int c = 1; c <= totalCols; c++)
            {
                // ✅ Obtener valor considerando celdas combinadas
                var valor = ObtenerValorEncabezadoConCombinadas(ws, fila, c, celdasCombinadas);
                var cell = ws.Cells[fila, c];

                if (!string.IsNullOrEmpty(valor))
                {
                    nonEmpty++;
                    valoresDistinct.Add(valor.ToLowerInvariant());

                    // Verificar si contiene tokens de header
                    var valorLimpio = LimpiarTexto(valor);
                    foreach (var token in valorLimpio.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (headerTokens.Contains(token))
                        {
                            tokensEncontrados++;
                            break; // Solo contar una vez por columna
                        }
                    }
                }
                else if (!esVacia(cell))
                {
                    nonEmpty++;
                }

                if (esTexto(cell) || !string.IsNullOrEmpty(valor))
                {
                    cntTexto++;
                }
            }

            double cobertura = (double)nonEmpty / totalCols;
            double fracTexto = nonEmpty == 0 ? 0 : (double)cntTexto / nonEmpty;
            double distintividad = nonEmpty == 0 ? 0 : (double)valoresDistinct.Count / Math.Max(1, cntTexto);
            double bonusTokens = Math.Min(1.0, tokensEncontrados / Math.Max(1.0, totalCols / 4.0));

            // ✅ NUEVO: Bonus especial para filas con celdas combinadas (típico de encabezados)
            var tieneCeldasCombinadas = celdasCombinadas.Any(cc => fila >= cc.FilaInicio && fila <= cc.FilaFin);
            double bonusCombinadas = tieneCeldasCombinadas ? 0.1 : 0;

            // Contraste con fila siguiente
            double contraste = CalcularContrasteConFilaSiguiente(ws, fila, totalCols, esVacia, esNumero, pareceFecha, esTexto);

            // Score final optimizado
            double score = 0.35 * cobertura +
                           0.25 * fracTexto +
                           0.15 * distintividad +
                           0.15 * bonusTokens +
                           0.10 * contraste +
                           bonusCombinadas;

            // Reglas de elegibilidad
            bool elegible = (cobertura >= 0.30 && fracTexto >= 0.30) || tokensEncontrados >= 2 || tieneCeldasCombinadas;

            return (elegible ? score : 0, cobertura, tokensEncontrados);
        }

        // ✅ NUEVO: Calcular contraste con fila siguiente
        private double CalcularContrasteConFilaSiguiente(ExcelWorksheet ws, int fila, int totalCols,
            Func<ExcelRangeBase, bool> esVacia, Func<ExcelRangeBase, bool> esNumero,
            Func<ExcelRangeBase, bool> pareceFecha, Func<ExcelRangeBase, bool> esTexto)
        {
            if (fila >= ws.Dimension?.End.Row) return 0;

            int diferencias = 0, considerados = 0;
            int filaSiguiente = fila + 1;

            for (int c = 1; c <= totalCols; c++)
            {
                var cellActual = ws.Cells[fila, c];
                var cellSiguiente = ws.Cells[filaSiguiente, c];

                if (!esVacia(cellActual) || !esVacia(cellSiguiente))
                {
                    considerados++;

                    bool actualEsTexto = esTexto(cellActual);
                    bool siguienteEsNumero = esNumero(cellSiguiente);
                    bool siguienteEsFecha = pareceFecha(cellSiguiente);

                    // Patrón típico: encabezado de texto, datos numéricos/fechas
                    if (actualEsTexto && (siguienteEsNumero || siguienteEsFecha))
                    {
                        diferencias++;
                    }
                }
            }

            return considerados == 0 ? 0 : (double)diferencias / considerados;
        }

        // ✅ NUEVO: Verificar continuidad de datos
        private bool HayContinuidadDatos(ExcelWorksheet ws, int filaEncabezado, int minFilas,
            Func<ExcelRangeBase, bool> esVacia, double minDensidad = 0.3)
        {
            int totalCols = ws.Dimension?.End.Column ?? 0;
            int totalFilas = ws.Dimension?.End.Row ?? 0;
            int encontrados = 0;

            for (int r = filaEncabezado + 1; r <= totalFilas && encontrados < minFilas; r++)
            {
                int nonEmpty = 0;
                for (int c = 1; c <= totalCols; c++)
                {
                    if (!esVacia(ws.Cells[r, c])) nonEmpty++;
                }

                double densidad = (double)nonEmpty / totalCols;
                if (densidad >= minDensidad) encontrados++;
            }

            return encontrados >= minFilas;
        }
        // --------------------------------------------------------------------

        // ✅ MANTENER: Método original con sobrecarga para compatibilidad
        private string DetectarTipoDato(ExcelWorksheet ws, int columna, int filaInicio, int sampleRows)
        {
            return DetectarTipoDato(ws, columna, filaInicio, sampleRows, null);
        }
        // ✅ MEJORAR: Detección de tipo de dato con consideraciones especiales
        private string DetectarTipoDato(ExcelWorksheet ws, int columna, int filaInicio, int sampleRows, string? encabezado = null)
        {
            var tipos = new Dictionary<string, int>
            {
                ["Número"] = 0,
                ["Texto"] = 0,
                ["Fecha"] = 0,
                ["Vacío"] = 0
            };

            int totalRows = ws.Dimension?.End.Row ?? 0;
            if (totalRows == 0) return "Desconocido";

            int endRow = Math.Min(totalRows, filaInicio + sampleRows - 1);

            // ✅ ESPECIAL: Si el encabezado sugiere que es un identificador, forzar como texto
            if (!string.IsNullOrEmpty(encabezado) && EsEncabezadoDeIdentificador(encabezado))
            {
                _logger?.LogDebug($"Forzando detección como 'Texto' para columna con encabezado: '{encabezado}'");
                return "Texto";
            }

            for (int fila = filaInicio; fila <= endRow + 1; fila++)
            {
                var valor = ws.Cells[fila, columna].Value;

                if (valor == null || string.IsNullOrWhiteSpace(valor.ToString()))
                {
                    tipos["Vacío"]++;
                }
                else if (DateTime.TryParse(valor.ToString(), out _))
                {
                    tipos["Fecha"]++;
                }
                else if (decimal.TryParse(valor.ToString(), out _))
                {
                    // ✅ ESPECIAL: Verificar si parece un EAN (números largos)
                    var valorTexto = valor.ToString()?.Trim();
                    if (!string.IsNullOrEmpty(valorTexto) &&
                        valorTexto.Length >= 8 && // EAN mínimo 8 dígitos
                        valorTexto.All(char.IsDigit) &&
                        EsLongitudEAN(valorTexto.Length))
                    {
                        tipos["Texto"]++; // Tratarlo como texto
                        _logger?.LogDebug($"Número largo detectado como EAN: '{valorTexto}' - Tratando como texto");
                    }
                    else
                    {
                        tipos["Número"]++;
                    }
                }
                else
                {
                    tipos["Texto"]++;
                }
            }

            return tipos.OrderByDescending(x => x.Value).First().Key;
        }

        // ✅ NUEVO: Verificar si un encabezado sugiere un identificador
        private bool EsEncabezadoDeIdentificador(string encabezado)
        {
            var indicadoresIdentificador = new[]
            {
        "ean", "gtin", "upc", "barcode", "codigo barras", "cod barras",
        "codigo", "código", "id", "identificador", "dun", "isbn"
    };

            var encabezadoLimpio = LimpiarTexto(encabezado);

            return indicadoresIdentificador.Any(indicador =>
                encabezadoLimpio.Contains(indicador, StringComparison.OrdinalIgnoreCase));
        }

        // ✅ NUEVO: Verificar longitudes típicas de EAN
        private bool EsLongitudEAN(int longitud)
        {
            // Longitudes válidas de códigos EAN/UPC/GTIN
            var longitudesValidas = new[] { 8, 12, 13, 14 };
            return longitudesValidas.Contains(longitud);
        }
        private int ContarValoresNoVacios(ExcelWorksheet worksheet, int columna, int filaDatosInicio, int totalFilas)
        {
            int count = 0;
            for (int fila = filaDatosInicio; fila <= totalFilas; fila++)
            {
                var valor = worksheet.Cells[fila, columna].Value;
                if (valor != null && !string.IsNullOrWhiteSpace(valor.ToString()))
                {
                    count++;
                }
            }
            return count;
        }

        private List<string> ObtenerEjemplosValores(ExcelWorksheet worksheet, int columna, int cantidad, int filaDatosInicio)
        {
            var ejemplos = new List<string>();
            int encontrados = 0;

            for (int fila = filaDatosInicio; fila <= worksheet.Dimension?.End.Row && encontrados < cantidad; fila++)
            {
                var valor = worksheet.Cells[fila, columna].Value?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(valor))
                {
                    ejemplos.Add(valor);
                    encontrados++;
                }
            }

            return ejemplos;
        }

        private static string GetColumnName(int columnIndex)
        {
            string columnName = "";
            while (columnIndex > 0)
            {
                columnIndex--;
                columnName = (char)('A' + columnIndex % 26) + columnName;
                columnIndex /= 26;
            }
            return columnName;
        }

        private void CargarDatosIniciales()
        {
            if (DatosParaImportacion.Count == 0)
            {
                ObtenerDatosParaImportacion(_impServicio).GetAwaiter();
            }

            if (AnalisisFile == null || AnalisisFile.Columnas.Count == 0)
            {
                ObtenerPerfilDeProveedor(_impServicio, ProveedorSeleccionado.Cta_Id).GetAwaiter();
            }

        }
    }
}
