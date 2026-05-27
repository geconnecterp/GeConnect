namespace gc.infraestructura.Dtos.Cajas.Response
{
    /// <summary>
    /// DTO de respuesta al finalizar el proceso de pago
    /// </summary>
    public class FinalizarPagoResDto
    {
        /// <summary>
        /// Indica si el pago se procesó exitosamente
        /// </summary>
        public bool exito { get; set; }

        /// <summary>
        /// Número de comprobante generado (factura, recibo, etc.)
        /// </summary>
        public string? nro_comprobante { get; set; }

        /// <summary>
        /// ID interno del comprobante (dia_movi)
        /// </summary>
        public string? dia_movi { get; set; }

        /// <summary>
        /// Tipo de comprobante generado (FA, FB, FC, etc.)
        /// </summary>
        public string? tipo_comprobante { get; set; }

        /// <summary>
        /// Punto de venta utilizado
        /// </summary>
        public string? punto_venta { get; set; }

        /// <summary>
        /// Fecha y hora del comprobante
        /// </summary>
        public DateTime? fecha_comprobante { get; set; }

        /// <summary>
        /// Total del comprobante generado
        /// </summary>
        public decimal total_comprobante { get; set; }

        /// <summary>
        /// Mensaje informativo o de error
        /// </summary>
        public string mensaje { get; set; } = string.Empty;

        /// <summary>
        /// URL o código para imprimir el comprobante
        /// </summary>
        public string? url_impresion { get; set; }

        /// <summary>
        /// Datos adicionales (CAE, vencimiento CAE, etc.)
        /// </summary>
        public Dictionary<string, string>? datos_adicionales { get; set; }
    }
}
