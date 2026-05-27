namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta con un instrumento disponible
    /// </summary>
    public class InstrumentoDto
    {
        /// <summary>
        /// ID del instrumento
        /// </summary>
        public string InsId { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del instrumento
        /// </summary>
        public string InsDesc { get; set; } = string.Empty;

        /// <summary>
        /// ID del tipo de cuenta financiera padre
        /// </summary>
        public string TcfId { get; set; } = string.Empty;

        /// <summary>
        /// Importe disponible (para vales, mutuales, etc.)
        /// </summary>
        public decimal? ImporteDisponible { get; set; }

        /// <summary>
        /// Código adicional (para bancos, mutuales, etc.)
        /// </summary>
        public string? Codigo { get; set; }

        /// <summary>
        /// Datos extra (JSON con información adicional)
        /// </summary>
        public string? DatosExtra { get; set; }
    }
}