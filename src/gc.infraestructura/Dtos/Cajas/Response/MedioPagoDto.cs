namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta con un medio de pago configurado
    /// </summary>
    public class MedioPagoDto
    {
        /// <summary>
        /// ID del tipo de cuenta financiera (EF, CH, VA, etc.)
        /// </summary>
        public string TcfId { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del medio de pago
        /// </summary>
        public string TcfDesc { get; set; } = string.Empty;

        /// <summary>
        /// Categoría del medio de pago
        /// </summary>
        public string? Categoria { get; set; }

        /// <summary>
        /// Días máximos de vencimiento (para cheques)
        /// </summary>
        public int? DiasMaximosVencimiento { get; set; }

        /// <summary>
        /// Indica si requiere selección de instrumento
        /// </summary>
        public bool RequiereInstrumento { get; set; }

        /// <summary>
        /// Indica si permite carga múltiple (como cheques)
        /// </summary>
        public bool PermiteCargaMultiple { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int? Orden { get; set; }
    }
}