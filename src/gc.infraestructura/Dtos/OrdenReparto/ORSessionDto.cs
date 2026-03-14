namespace gc.infraestructura.Dtos.OrdenReparto
{
    /// <summary>
    /// DTO que encapsula TODOS los datos de sesión relacionados con Órdenes de Reparto
    /// </summary>
    public class ORSessionDto
    {
        /// <summary>
        /// Comprobante de la OR actualmente en proceso
        /// </summary>
        public string? ORComprobanteActual { get; set; }

        /// <summary>
        /// BOX seleccionado en el filtro actual
        /// </summary>
        public string? ORBoxSeleccionado { get; set; }

        /// <summary>
        /// RUBRO seleccionado en el filtro actual
        /// </summary>
        public string? ORRubroSeleccionado { get; set; }

        /// <summary>
        /// ID del producto actualmente en proceso de validación
        /// </summary>
        public string? ORProductoSeleccionado { get; set; }

        /// <summary>
        /// Lista completa de productos de la OR (filtrados o no)
        /// </summary>
        public List<ORProductoDto> ORListaProductosActual { get; set; } = new List<ORProductoDto>();

        /// <summary>
        /// ✅ NUEVO: Indica si el filtro actual es por BOX (true) o RUBRO (false)
        /// </summary>
        public bool FiltroEsBox { get; set; }

        /// <summary>
        /// ✅ NUEVO: Fecha/hora de última actualización de la sesión
        /// </summary>
        public DateTime UltimaActualizacion { get; set; } = DateTime.Now;

        /// <summary>
        /// Verifica si la sesión tiene datos válidos
        /// </summary>
        public bool EsValida()
        {
            return !string.IsNullOrEmpty(ORComprobanteActual);
        }

        /// <summary>
        /// Limpia todos los datos de la sesión
        /// </summary>
        public void Limpiar()
        {
            ORComprobanteActual = null;
            ORBoxSeleccionado = null;
            ORRubroSeleccionado = null;
            ORProductoSeleccionado = null;
            ORListaProductosActual = new List<ORProductoDto>();
            FiltroEsBox = false;
            UltimaActualizacion = DateTime.Now;
        }
    }
}
