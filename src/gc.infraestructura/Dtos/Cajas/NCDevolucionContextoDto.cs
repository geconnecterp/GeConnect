using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.infraestructura.Dtos.Cajas
{
    /// <summary>
    /// Estado aislado de una Nota de Crédito por devolución.
    /// No reutiliza ClienteActual, FacturaProductos ni FacturaSubtotales.
    /// </summary>
    public sealed class NCDevolucionContextoDto
    {
        public NCValidaResponseDto ComprobanteOriginal { get; set; } = new();

        /// <summary>
        /// PENDIENTE | TODOS | MANUAL.
        /// El Paso 2 consumirá este valor para decidir cómo invocar el SP de productos.
        /// </summary>
        public string ModoCargaInicial { get; set; } = "PENDIENTE";

        public DateTime FechaCreacionUtc { get; set; }
    }
}
