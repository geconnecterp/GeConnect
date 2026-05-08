namespace gc.infraestructura.Dtos.Cajas
{
    public class ReporteConfig
    {
        /// <summary>
        /// ID del reporte en la API (Ej: "67" para Factura A)
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Clave identificadora (Ej: "A", "B", "C")
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Nombre descriptivo (Ej: "FACTURA A")
        /// </summary>
        public string Nombre { get; set; } = string.Empty;
    }
}
