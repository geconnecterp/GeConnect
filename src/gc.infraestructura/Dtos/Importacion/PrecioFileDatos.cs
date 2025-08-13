namespace gc.infraestructura.Dtos.Importacion
{
    public class PrecioFileDatos
    {
        public string Campo { get; set; } = string.Empty;
        public string dato { get; set; }=string.Empty;
        public char Tipo { get; set; }
    }

    public class AnalisisExcelDto
    {
        public string NombreArchivo { get; set; }= string.Empty;
        public string NombreHoja { get; set; } = string.Empty;
        public int TotalFilas { get; set; }
        public int TotalColumnas { get; set; }
        public List<ColumnaExcelDto> Columnas { get; set; } = [];
        // ✅ NUEVO: Lista de campos disponibles para mapeo
        public List<PrecioFileDatos> CamposDisponibles { get; set; } = [];
    }

    public class ColumnaExcelDto
    {
        public int Indice { get; set; }
        public string Letra { get; set; } = string.Empty;
        public string Encabezado { get; set; } = string.Empty;
        public string TipoDetectado { get; set; } = string.Empty;
        public int ValoresNoVacios { get; set; }
        public double PorcentajeLlenado { get; set; }
        public List<string> EjemplosValores { get; set; } = [];

        // ✅ NUEVAS: Propiedades para mapeo automático
        public string CampoMapeado { get; set; } = string.Empty; // Código BD (ej: "p_ean")
        public string DescripcionMapeado { get; set; } = string.Empty; // Descripción (ej: "EAN")
        public int ConfianzaMapeo { get; set; } = 0; // Porcentaje de confianza (0-100)
        public bool MapeadoAutomatico { get; set; } = false; // Si fue mapeado automáticamente
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
}
