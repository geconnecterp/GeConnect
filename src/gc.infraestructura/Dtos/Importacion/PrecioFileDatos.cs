namespace gc.infraestructura.Dtos.Importacion
{
    public class PrecioFileDatos
    {
        public string Campo { get; set; } = string.Empty;
        public string Dato { get; set; }=string.Empty;
        public char Tipo { get; set; }
        public bool HasChecked { get; set; } = false;
    }

    public class AnalisisExcelDto
    {
        public string NombreArchivo { get; set; }= string.Empty;
        public string NombreHoja { get; set; } = string.Empty;
        public int TotalFilas { get; set; }
        public int TotalColumnas { get; set; }
        public int TotalColumnasUtiles { get; set; }
        public List<ColumnaExcelDto> Columnas { get; set; } = [];
        // ✅ NUEVO: Lista de campos disponibles para mapeo
        public List<PrecioFileDatos> CamposDisponibles { get; set; } = [];
    }

    // ✅ ASEGURAR: DTOs están correctos
    public class ColumnaExcelDto
    {
        public int Indice { get; set; }
        public string Letra { get; set; } = string.Empty;
        public string Encabezado { get; set; } = string.Empty;
        public string TipoDetectado { get; set; } = string.Empty;
        public int ValoresNoVacios { get; set; }
        public double PorcentajeLlenado { get; set; }
        public List<string> EjemplosValores { get; set; } = [];

        // ✅ VERIFICAR: Estas propiedades existen
        public string CampoMapeado { get; set; } = string.Empty;
        public string DescripcionMapeado { get; set; } = string.Empty;
        public int ConfianzaMapeo { get; set; } = 0;
        public bool MapeadoAutomatico { get; set; } = false;
    }

    // ✅ NUEVO: Estructura para resultado de detección
    public class DeteccionEncabezadosDto
    {
        public int FilaEncabezados { get; set; } = 1; // Fila donde están los encabezados (1-based)
        public int FilaInicioQdatos { get; set; } = 2; // Fila donde inician los datos (1-based)
        public bool TieneEncabezados { get; set; } = true; // Si se detectaron encabezados
        public double ConfianzaDeteccion { get; set; } = 0; // Porcentaje de confianza (0-100)
        public string MotivoDeteccion { get; set; } = string.Empty; // Razón de la detección
        public List<string> IndiciosEncontrados { get; set; } = new(); // Detalles de detección
    }

    public class ProveedorPerfilDto
    {
        public string cta_id { get; set; } = string.Empty;
        public int columnas { get; set; }
        public int columnas_utiles { get; set; }
        public string formato { get; set; } = string.Empty;
        public DateTime fecha_alta { get; set; }
        public string Usuario { get; set; } = string.Empty;

        public List<ProveedorPerfilDetalleDto> detalles { get; set; } = [];
    }

    public class ProveedorPerfilDetalleDto
    {
        public string cta_id { get; set; } = string.Empty;
        public string campo { get; set; } = string.Empty;
        public string dato { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        //identifica la columna del archivo excel
        public string letra { get; set; }= string.Empty;
        public string campoMapeado { get; set; } = string.Empty;
        public string encabezado { get; set; } = string.Empty;
        public int indice { get; set; }
    }

    public class ProveedorPerfilDB
    {
        public string cta_id { get; set; } = string.Empty;
        public int columnas { get; set; }
        public int columnas_utiles { get; set; }
        public string formato { get; set; } = string.Empty;

        public string campo { get; set; } = string.Empty;
        public string dato { get; set; } = string.Empty;
        public string tipo { get; set; } = string.Empty;
        //identifica la columna del archivo excel
        public string letra { get; set; } = string.Empty;
        public string encabezado { get; set; } = string.Empty;
        public int indice { get; set; }

    }

    // ✅ NUEVO: DTO para enviar datos reales del Excel
    public class DatosImportacionDto
    {
        public string ProveedorId { get; set; } = string.Empty;
        public string NombreArchivo { get; set; } = string.Empty;
        public int TotalFilas { get; set; }
        public int TotalColumnas { get; set; }
        public int FilaEncabezados { get; set; }
        public DateTime FechaProceso { get; set; }
        public List<FilaDatosDto> Filas { get; set; } = new();
        public List<MapeoColumnaDto> MapeoColumnas { get; set; } = new();
    }

    // ✅ NUEVO: DTO para cada fila de datos
    public class FilaDatosDto
    {
        public int NumeroFila { get; set; }
        public Dictionary<string, object?> Valores { get; set; } = new(); // Key = CampoBD, Value = Valor de celda
    }

    // ✅ NUEVO: DTO para información de mapeo
    public class MapeoColumnaDto
    {
        public int IndiceColumna { get; set; }
        public string LetraColumna { get; set; } = string.Empty;
        public string EncabezadoOriginal { get; set; } = string.Empty;
        public string CampoBD { get; set; } = string.Empty;
        public string DescripcionCampo { get; set; } = string.Empty;
        public string TipoDato { get; set; } = string.Empty;
        public int ConfianzaMapeo { get; set; }
        public bool MapeadoAutomatico { get; set; }
    }
}
