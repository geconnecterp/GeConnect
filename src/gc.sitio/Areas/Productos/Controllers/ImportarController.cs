using DocumentFormat.OpenXml.Spreadsheet;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Importacion;
using gc.sitio.Areas.Compras.Controllers;
using gc.sitio.core.Servicios.Contratos;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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
        private ProveedorListaDto _datosProveedor;
        // Analizar primera fila (encabezados)
        int filaEncabezados = 0;
        int filaDatosInicio = 0;
        int totalFilasDatos = 0;

        public ImportarController(
            ICuentaServicio cuentaServicio,
            IProducto2Servicio productoServicio,
            ILogger<CompraController> logger,
            IOptions<AppSettings> options,
            IImportarServicio impServicio,
            IHttpContextAccessor context) : base(options, context, logger)

        {
            _cuentaServicio = cuentaServicio;
            _productoServicio = productoServicio;
            _appSettings = options.Value;
            _impServicio = impServicio;
            _datosProveedor = ProveedorSeleccionado;
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
            catch(NegocioException ex)
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

        // ✅ NUEVA: Método para mapeo automático inteligente
        private void AplicarMapeoAutomaticoInteligente(AnalisisExcelDto analisis)
        {
            foreach (var columna in analisis.Columnas)
            {
                var mapeoEncontrado = BuscarMejorMapeo(columna, analisis.CamposDisponibles);

                if (mapeoEncontrado != null)
                {
                    columna.CampoMapeado = mapeoEncontrado.dato;
                    columna.DescripcionMapeado = mapeoEncontrado.Campo;
                    columna.MapeadoAutomatico = true;

                    _logger?.LogInformation($"Mapeo automático: '{columna.Encabezado}' → '{mapeoEncontrado.Campo}' (Confianza: {columna.ConfianzaMapeo}%)");
                }
            }
        }

        // ✅ NUEVA: Algoritmo inteligente de mapeo por similitud
        private PrecioFileDatos? BuscarMejorMapeo(ColumnaExcelDto columna, List<PrecioFileDatos> camposDisponibles)
        {
            var encabezadoLimpio = LimpiarTexto(columna.Encabezado);

            // ✅ DICCIONARIO DE SINÓNIMOS COMUNES
            var sinonimos = new Dictionary<string, string[]>
            {
                ["ean"] = ["ean", "codigo barras", "cod barras", "barcode", "gtin"],
                ["codigo"] = ["codigo", "cod", "code", "item", "articulo", "art"],
                ["descripcion"] = ["descripcion", "desc", "producto", "nombre", "detalle"],
                ["precio"] = ["precio", "price", "valor", "importe", "monto"],
                ["costo"] = ["costo", "cost", "precio compra"],
                ["descuento"] = ["descuento", "desc", "discount", "dto", "rebaja"],
                ["stock"] = ["stock", "existencia", "cantidad", "qty"],
                ["marca"] = ["marca", "brand", "fabricante"],
                ["categoria"] = ["categoria", "rubro", "familia", "grupo"],
                ["peso"] = ["peso", "weight", "kg"],
                ["medida"] = ["medida", "unidad", "unit", "um"]
            };

            PrecioFileDatos? mejorMapeo = null;
            int mayorConfianza = 0;

            foreach (var campo in camposDisponibles)
            {
                var confianza = CalcularConfianzaMapeo(encabezadoLimpio, campo, sinonimos, columna.TipoDetectado);

                if (confianza > mayorConfianza && confianza >= 70) // Umbral mínimo 70%
                {
                    mayorConfianza = confianza;
                    mejorMapeo = campo;
                    columna.ConfianzaMapeo = confianza;
                }
            }

            return mejorMapeo;
        }

        // ✅ NUEVA: Calcular confianza de mapeo con múltiples criterios
        private int CalcularConfianzaMapeo(string encabezado, PrecioFileDatos campo,
            Dictionary<string, string[]> sinonimos, string tipoDetectado)
        {
            var campoLimpio = LimpiarTexto(campo.Campo);
            int confianza = 0;

            // ✅ CRITERIO 1: Coincidencia exacta (100%)
            if (encabezado == campoLimpio)
                return 100;

            // ✅ CRITERIO 2: Coincidencia parcial directa (80%)
            if (encabezado.Contains(campoLimpio) || campoLimpio.Contains(encabezado))
                confianza = Math.Max(confianza, 80);

            // ✅ CRITERIO 3: Búsqueda en sinónimos (60-90%)
            foreach (var sinonimo in sinonimos)
            {
                if (sinonimo.Value.Any(s => encabezado.Contains(s)))
                {
                    var palabrasClave = new[] { "precio", "ean", "codigo", "descripcion" };
                    if (palabrasClave.Any(p => campoLimpio.Contains(p)))
                    {
                        confianza = Math.Max(confianza, 85);
                    }
                    else
                    {
                        confianza = Math.Max(confianza, 60);
                    }
                }
            }

            // ✅ CRITERIO 4: Compatibilidad de tipos (+10%)
            if (VerificarCompatibilidadTipo(tipoDetectado, campo.Tipo))
            {
                confianza += 10;
            }

            // ✅ CRITERIO 5: Patrones específicos
            confianza += DetectarPatronesEspecificos(encabezado, campo);

            return Math.Min(confianza, 100); // Máximo 100%
        }

        // ✅ NUEVA: Funciones auxiliares
        private string LimpiarTexto(string texto)
        {
            return texto.ToLowerInvariant()
                        .Replace("_", " ")
                        .Replace("-", " ")
                        .Replace(".", "")
                        .Trim();
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

        private int DetectarPatronesEspecificos(string encabezado, PrecioFileDatos campo)
        {
            var patrones = new Dictionary<string, string[]>
            {
                ["p_ean"] = ["ean", "13", "barras", "gtin"],
                ["p_plista"] = ["lista", "price", "precio", "$"],
                ["p_desc"] = ["desc", "nombre", "producto"],
                ["p_codigo"] = ["cod", "item", "art", "ref"]
            };

            if (patrones.ContainsKey(campo.dato))
            {
                var palabrasClave = patrones[campo.dato];
                return palabrasClave.Any(p => encabezado.Contains(p)) ? 15 : 0;
            }

            return 0;
        }
        private async Task<AnalisisExcelDto> AnalizarEstructuraExcel(IFormFile archivo)
        {
            using var stream = new MemoryStream();
            await archivo.CopyToAsync(stream);

            // Configurar EPPlus
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
            {
                return analisis;
            }

            // Analizar primera fila (encabezados)
             filaEncabezados = DetectarFilaEncabezados(worksheet, maxFilasExploracion: 10, minFilasDatos: 2);
             filaDatosInicio = Math.Min(analisis.TotalFilas, filaEncabezados + 1);
             totalFilasDatos = Math.Max(0, analisis.TotalFilas - filaEncabezados);

            // Analizar primera fila (encabezados)
            for (int col = 1; col <= analisis.TotalColumnas; col++)
            {
                var valorEncabezado = worksheet.Cells[filaEncabezados, col].Value?.ToString()?.Trim() ?? $"Columna {col}";

                // Analizar tipo de datos usando filas de datos (no la fila de encabezados)
                var tipoDetectado = DetectarTipoDato(worksheet, col, filaDatosInicio);//, sampleRows: Math.Min(10, totalFilasDatos));

                // Contar valores no vacíos SOLO en los datos
                var valoresNoVacios = ContarValoresNoVacios(worksheet, col, filaDatosInicio, analisis.TotalFilas);

                var columna = new ColumnaExcelDto
                {
                    Indice = col,
                    Letra = GetColumnName(col),
                    Encabezado = valorEncabezado,
                    TipoDetectado = tipoDetectado,
                    ValoresNoVacios = valoresNoVacios,
                    PorcentajeLlenado = totalFilasDatos > 0
                        ? Math.Round((double)valoresNoVacios / totalFilasDatos * 100, 1)
                        : 0,
                    EjemplosValores = ObtenerEjemplosValores(worksheet, col, cantidad: 3, filaDatosInicio)
                };

                analisis.Columnas.Add(columna);
            }

            return analisis;
        }

        // --------------------------------------------------------------------
        // FUNCIÓN DE ANÁLISIS: Detecta la fila de cabeceras (ignora títulos)
        // --------------------------------------------------------------------
        private int DetectarFilaEncabezados(ExcelWorksheet ws, int maxFilasExploracion = 10, int minFilasDatos = 2)
        {
            int totalFilas = ws.Dimension?.End.Row ?? 0;
            int totalCols = ws.Dimension?.End.Column ?? 0;
            if (totalFilas == 0 || totalCols == 0) return 1;

            int maxRow = Math.Min(maxFilasExploracion, totalFilas);

            // Lista básica de tokens comunes de headers (ES/EN)
            var headerTokens = new HashSet<string>(new[]
            {
                "id","codigo","código","item","cliente","nombre","apellido","razon social","razón social",
                "direccion","dirección","domicilio","ciudad","provincia","pais","país","cp","c.p.",
                "email","correo","telefono","teléfono","movil","móvil",
                "fecha","fec","año","mes","dia","día","hora",
                "monto","importe","total","cantidad","precio","costo","neto","bruto","iva","tax","amount","qty","price",
                "estado","status","tipo","categoria","categoría","descripcion","descripción","obs","observaciones",
                "usuario","user"
            }.Select(t => t.ToLowerInvariant()));

            // Helpers locales para el scoring
            bool EsVacia(ExcelRangeBase cell)
                => string.IsNullOrWhiteSpace(cell?.Text);

            bool EsBooleano(ExcelRangeBase cell)
                => cell?.Value is bool;

            bool EsNumero(ExcelRangeBase cell)
            {
                if (cell?.Value == null) return false;
                if (cell.Value is sbyte or byte or short or ushort or int or uint or long or ulong or float or double or decimal) return true;
                // A veces EPPlus expone todo como double/texto formateado:
                return double.TryParse(cell.Text?.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out _)
                    || double.TryParse(cell.Text?.Trim(), NumberStyles.Any, CultureInfo.CurrentCulture, out _);
            }

            bool PareceFecha(ExcelRangeBase cell)
            {
                if (cell?.Value is DateTime) return true;

                // Por formato: muy heurístico pero útil
                var fmt = cell?.Style?.Numberformat?.Format?.ToLowerInvariant() ?? string.Empty;
                if (!string.IsNullOrEmpty(fmt))
                {
                    if (fmt.Contains("yy") || fmt.Contains("dd") || fmt.Contains("mm") || fmt.Contains("hh") || fmt.Contains("ss"))
                        return true;
                }

                // Por texto: intenta parsear fecha en culturas comunes
                var txt = cell?.Text?.Trim();
                if (string.IsNullOrEmpty(txt)) return false;

                return DateTime.TryParse(txt, CultureInfo.CurrentCulture, DateTimeStyles.None, out _)
                    || DateTime.TryParse(txt, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
            }

            bool EsTexto(ExcelRangeBase cell)
            {
                if (cell?.Value == null) return false;
                if (EsNumero(cell) || PareceFecha(cell) || EsBooleano(cell)) return false;
                // Si no cae en los anteriores y tiene texto, lo consideramos texto:
                return !string.IsNullOrWhiteSpace(cell.Text);
            }

            bool EsTextoCorto(ExcelRangeBase cell)
            {
                var len = cell?.Text?.Trim().Length ?? 0;
                return len > 0 && len <= 30;
            }

            bool TieneTextoLargo(ExcelRangeBase cell)
            {
                var len = cell?.Text?.Trim().Length ?? 0;
                return len > 40; // típico de títulos
            }

            // Cuenta merges que toquen la fila r
            int MergesEnFila(int r)
            {
                if (ws.MergedCells == null || ws.MergedCells.Count == 0) return 0;
                int count = 0;
                foreach (var addr in ws.MergedCells)
                {
                    var a = new ExcelAddress(addr);
                    if (a.Start.Row <= r && a.End.Row >= r && a.Start.Column <= totalCols && a.End.Column >= 1)
                        count++;
                }
                return count;
            }

            // Devuelve la siguiente fila "con datos" (densidad mínima)
            int? SiguienteFilaConDatos(int startRow, double minDensidad = 0.3, int maxLookahead = 30)
            {
                int end = Math.Min(totalFilas, startRow + maxLookahead);
                for (int r = startRow; r <= end; r++)
                {
                    int nonEmpty = 0;
                    for (int c = 1; c <= totalCols; c++)
                        if (!EsVacia(ws.Cells[r, c])) nonEmpty++;

                    double densidad = (double)nonEmpty / totalCols;
                    if (densidad >= minDensidad) return r;
                }
                return null;
            }

            // Verifica que existan al menos N filas de datos "consistentes" debajo
            bool HayContinuidadDatos(int headerRow, int minFilas, double minDensidad = 0.3)
            {
                int encontrados = 0;
                for (int r = headerRow + 1; r <= totalFilas; r++)
                {
                    int nonEmpty = 0;
                    for (int c = 1; c <= totalCols; c++)
                        if (!EsVacia(ws.Cells[r, c])) nonEmpty++;

                    double densidad = (double)nonEmpty / totalCols;
                    if (densidad >= minDensidad) encontrados++;
                    if (encontrados >= minFilas) return true;
                }
                return false;
            }

            // Scoring por fila
            double MejorScore = double.NegativeInfinity;
            int mejorFila = 1;

            for (int r = 1; r <= maxRow; r++)
            {
                int nonEmpty = 0, cntTexto = 0, cntTextoCorto = 0, cntTextoLargo = 0;
                var valoresDistinct = new HashSet<string>();
                int merges = MergesEnFila(r);

                for (int c = 1; c <= totalCols; c++)
                {
                    var cell = ws.Cells[r, c];
                    if (!EsVacia(cell)) nonEmpty++;

                    if (EsTexto(cell))
                    {
                        cntTexto++;
                        if (EsTextoCorto(cell)) cntTextoCorto++;
                        if (TieneTextoLargo(cell)) cntTextoLargo++;

                        var norm = (cell.Text ?? string.Empty).Trim().ToLowerInvariant();
                        norm = new string(norm.Where(ch => char.IsLetterOrDigit(ch) || ch == '_' || ch == ' ').ToArray());
                        if (!string.IsNullOrWhiteSpace(norm))
                            valoresDistinct.Add(norm);
                    }
                }

                double cobertura = (double)nonEmpty / totalCols;                 // [0..1]
                double fracTexto = nonEmpty == 0 ? 0 : (double)cntTexto / nonEmpty;
                double fracTextoCorto = nonEmpty == 0 ? 0 : (double)cntTextoCorto / nonEmpty;
                double fracTextoLargo = nonEmpty == 0 ? 0 : (double)cntTextoLargo / nonEmpty;
                double distintividad = nonEmpty == 0 ? 0 : (double)valoresDistinct.Count / Math.Max(1, cntTexto);

                // Bonus por tokens típicos de headers
                int hitsTokens = 0;
                foreach (var v in valoresDistinct)
                {
                    var w = v.Trim().ToLowerInvariant();
                    // separa por espacios y evalúa cada token
                    foreach (var token in w.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        if (headerTokens.Contains(token)) hitsTokens++;
                }
                double bonusTokens = Math.Min(1.0, hitsTokens / Math.Max(1.0, totalCols / 4.0)); // normaliza aprox

                // Contraste con siguiente fila "de datos"
                double contraste = 0;
                var nextRow = SiguienteFilaConDatos(r + 1);
                if (nextRow.HasValue)
                {
                    int dif = 0, considerados = 0;
                    for (int c = 1; c <= totalCols; c++)
                    {
                        var hc = ws.Cells[r, c];
                        var dc = ws.Cells[nextRow.Value, c];

                        bool hTexto = EsTexto(hc);
                        bool dNum = EsNumero(dc);
                        bool dFecha = PareceFecha(dc);
                        bool dBool = EsBooleano(dc);

                        if (!EsVacia(hc) || !EsVacia(dc))
                        {
                            considerados++;
                            // Buscamos patrón: Header texto (o vacío) y Datos num/fecha/bool
                            if ((hTexto || EsVacia(hc)) && (dNum || dFecha || dBool)) dif++;
                        }
                    }
                    contraste = considerados == 0 ? 0 : (double)dif / considerados; // [0..1]
                }

                // Penalización por merges (títulos)
                double penMerges = Math.Min(1.0, merges / Math.Max(1.0, totalCols / 4.0));

                // Score final (ponderaciones afinadas empíricamente)
                double score =
                    0.35 * cobertura +
                    0.20 * fracTexto +
                    0.15 * distintividad +
                    0.10 * fracTextoCorto +
                    0.15 * contraste +
                    0.05 * bonusTokens
                    - 0.20 * penMerges
                    - 0.10 * fracTextoLargo;

                // Reglas de elegibilidad mínimas
                bool elegible =
                    (cobertura >= 0.40 && fracTexto >= 0.40)   // suficiente texto y cobertura
                    || (contraste >= 0.30);                     // o buen contraste con la fila siguiente

                if (!HayContinuidadDatos(r, minFilasDatos)) elegible = false;

                if (elegible && score > MejorScore)
                {
                    MejorScore = score;
                    mejorFila = r;
                }
            }

            // Fallback si nada fue elegible
            if (MejorScore == double.NegativeInfinity) return 1;

            return mejorFila;
        }
        // --------------------------------------------------------------------


        private string DetectarTipoDato(ExcelWorksheet worksheet, int columna, int filasAAnalizar)
        {
            var tipos = new Dictionary<string, int>
            {
                ["Número"] = 0,
                ["Texto"] = 0,
                ["Fecha"] = 0,
                ["Vacío"] = 0
            };

            for (int fila = 2; fila <= filasAAnalizar + 1; fila++)
            {
                var valor = worksheet.Cells[fila, columna].Value;

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
                    tipos["Número"]++;
                }
                else
                {
                    tipos["Texto"]++;
                }
            }

            return tipos.OrderByDescending(x => x.Value).First().Key;
        }

        private int ContarValoresNoVacios(ExcelWorksheet worksheet, int columna,int filaDatosInicio, int totalFilas)
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
            if(DatosParaImportacion.Count== 0)
            {
                ObtenerDatosParaImportacion(_impServicio).GetAwaiter();
            }
        }
    }
}
