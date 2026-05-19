using gc.infraestructura.Dtos.Cajas.Request;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    /// <summary>
    /// ✅ ACTUALIZADO v2.0: Servicio de backup de productos en archivo plano
    /// Usa la ruta configurada en AppSettings.RutaFileCaja
    /// Permite recuperar productos en caso de cortes de comunicación
    /// </summary>
    public interface IBackupProductosServicio
    {
        /// <summary>
        /// Inicializa un nuevo archivo de backup (vacío)
        /// </summary>
        Task<bool> InicializarBackup(string cajaId, string usuarioId);

        /// <summary>
        /// ✅ NUEVO v2.2: Reinicia el backup eliminando archivos previos y creando uno nuevo
        /// Se invoca cuando se carga el primer producto (item = 1) de una nueva sesión
        /// </summary>
        Task<bool> ReiniciarBackup(string cajaId, string usuarioId);

        /// <summary>
        /// Guarda un producto en el archivo de backup
        /// </summary>
        Task<bool> GuardarProducto(ProductoDatosResponseDto producto, string cajaId, string usuarioId);

        /// <summary>
        /// Recupera todos los productos del backup más reciente
        /// </summary>
        Task<List<ProductoDatosResponseDto>> RecuperarBackup(string cajaId, string usuarioId);

        /// <summary>
        /// Limpia todos los archivos de backup de la caja/usuario
        /// </summary>
        Task<bool> LimpiarBackup(string cajaId, string usuarioId);

        /// <summary>
        /// Verifica si existe un backup pendiente
        /// </summary>
        Task<bool> ExisteBackup(string cajaId, string usuarioId);

        /// <summary>
        /// ✅ NUEVO v1.1: Elimina un producto específico del backup
        /// </summary>
        /// <param name="item">Número de item correlativo del producto</param>
        /// <param name="cajaId">ID de la caja</param>
        /// <param name="usuarioId">ID del usuario</param>
        /// <returns>True si se eliminó correctamente, False en caso contrario</returns>
        Task<bool> EliminarProducto(int item, string cajaId, string usuarioId);
    }
}
