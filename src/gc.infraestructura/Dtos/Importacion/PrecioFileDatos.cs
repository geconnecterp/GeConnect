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
    }
}
