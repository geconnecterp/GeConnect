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

            // Analizar primera fila (encabezados)
            for (int col = 1; col <= analisis.TotalColumnas; col++)
            {
                var valorEncabezado = worksheet.Cells[1, col].Value?.ToString()?.Trim() ?? $"Columna {col}";

                // Analizar tipo de datos en las primeras 10 filas (muestra)
                var tipoDetectado = DetectarTipoDato(worksheet, col, Math.Min(10, analisis.TotalFilas));

                // Contar valores no vacíos en esta columna
                var valoresNoVacios = ContarValoresNoVacios(worksheet, col, analisis.TotalFilas);

                var columna = new ColumnaExcelDto
                {
                    Indice = col,
                    Letra = GetColumnName(col),
                    Encabezado = valorEncabezado,
                    TipoDetectado = tipoDetectado,
                    ValoresNoVacios = valoresNoVacios,
                    PorcentajeLlenado = analisis.TotalFilas > 1 ?
                        Math.Round((double)valoresNoVacios / (analisis.TotalFilas - 1) * 100, 1) : 0,
                    EjemplosValores = ObtenerEjemplosValores(worksheet, col, 3)
                };

                analisis.Columnas.Add(columna);
            }

            return analisis;
        }

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

        private int ContarValoresNoVacios(ExcelWorksheet worksheet, int columna, int totalFilas)
        {
            int count = 0;
            for (int fila = 2; fila <= totalFilas; fila++)
            {
                var valor = worksheet.Cells[fila, columna].Value;
                if (valor != null && !string.IsNullOrWhiteSpace(valor.ToString()))
                {
                    count++;
                }
            }
            return count;
        }

        private List<string> ObtenerEjemplosValores(ExcelWorksheet worksheet, int columna, int cantidad)
        {
            var ejemplos = new List<string>();
            int encontrados = 0;

            for (int fila = 2; fila <= worksheet.Dimension?.End.Row && encontrados < cantidad; fila++)
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
