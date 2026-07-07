using gc.infraestructura.Dtos.Cajas.Response;

namespace gc.caja.Models.NotaCredito
{
    /// <summary>
    /// Datos enviados desde la interfaz para validar
    /// un comprobante origen de Nota de Crédito por Devolución.
    /// </summary>
    public sealed class ValidarComprobanteOrigenRequest
    {
        public string TcoId { get; set; } = string.Empty;
        public string PuntoVenta { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
    }

    /// <summary>
    /// Selección de un comprobante cuando SPGECO_CAJA_NC_Valida
    /// devuelve más de una coincidencia.
    ///
    /// El índice se valida exclusivamente contra los candidatos
    /// guardados en sesión.
    /// </summary>
    public sealed class SeleccionarComprobanteRepetidoRequest
    {
        public int Indice { get; set; } = -1;
    }

    /// <summary>
    /// Decisión inicial del cajero sobre la modalidad de carga
    /// del detalle a devolver.
    /// </summary>
    public sealed class DefinirModalidadCargaInicialRequest
    {
        /// <summary>
        /// true  = cargar todo el detalle del comprobante original.
        /// false = cargar manualmente productos a devolver.
        /// </summary>
        public bool CargarTodoDetalle { get; set; }
    }

    /// <summary>
    /// Datos mínimos enviados por el navegador para agregar un producto
    /// manualmente a una Nota de Crédito por Devolución.
    ///
    /// El comprobante original, administración y productos existentes
    /// se obtienen exclusivamente desde la sesión del servidor.
    /// </summary>
    public sealed class AgregarProductoManualRequest
    {
        /// <summary>
        /// Código EAN, código de barras o identificador interno del producto.
        /// No debe contener el prefijo cantidad+código.
        /// Ejemplo: 7790070036599 o 004627.
        /// </summary>
        public string Valor { get; set; } = string.Empty;

        /// <summary>
        /// Cantidad solicitada. Puede tener hasta tres decimales.
        /// </summary>
        public decimal Cantidad { get; set; } = 1m;
    }

    /// <summary>
    /// Contexto aislado de una NC por Devolución en curso.
    ///
    /// No utiliza ClienteActual, FacturaProductos ni FacturaSubtotales,
    /// para no mezclar la operación con Facturación o Cobranzas.
    /// </summary>
    public sealed class NCDevolucionContextoSesion
    {
        /// <summary>
        /// Comprobante de venta original validado y seleccionado.
        /// </summary>
        public NCValidaResponseDto ComprobanteOrigen { get; set; } = new();

        /// <summary>
        /// true  = el cajero desea cargar todo el detalle original.
        /// false = el cajero realizará carga manual de productos.
        /// null  = todavía no se definió la modalidad.
        /// </summary>
        public bool? CargarTodoDetalle { get; set; }

        /// <summary>
        /// Productos de la devolución en curso.
        /// Este estado es exclusivo del módulo NC por Devolución.
        /// No utiliza FacturaProductos.
        /// </summary>
        public List<NCProductoBuscarResponseDto> ProductosDevolucion { get; set; } = new();

        /// <summary>
        /// Fecha de la última actualización de productos de devolución.
        /// </summary>
        public DateTime? FechaUltimaCargaProductosUtc { get; set; }

        public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;
    }
}
