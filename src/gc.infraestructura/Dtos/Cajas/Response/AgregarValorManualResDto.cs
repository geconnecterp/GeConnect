namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta al agregar un valor manual
    /// </summary>
    public class AgregarValorManualResDto
    {
        /// <summary>
        /// ID único del valor generado (temporal o definitivo)
        /// </summary>
        public string valor_id { get; set; } = string.Empty;

        /// <summary>
        /// Descripción del valor agregado
        /// </summary>
        public string descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de valor (EFECTIVO, VALE, CHEQUE, etc.)
        /// </summary>
        public string tipo_valor { get; set; } = string.Empty;

        /// <summary>
        /// Monto del valor
        /// </summary>
        public decimal monto { get; set; }

        /// <summary>
        /// Indica si el valor puede ser eliminado posteriormente
        /// </summary>
        public bool eliminable { get; set; } = true;

        /// <summary>
        /// Fecha del valor (si aplica)
        /// </summary>
        public DateTime? fecha { get; set; }

        /// <summary>
        /// Estado del valor (ACTIVO, PENDIENTE, etc.)
        /// </summary>
        public string estado { get; set; } = "ACTIVO";

        /// <summary>
        /// Mensaje informativo (opcional)
        /// </summary>
        public string? mensaje { get; set; }
    }
}
