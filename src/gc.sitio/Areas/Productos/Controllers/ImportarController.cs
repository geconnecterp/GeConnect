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
