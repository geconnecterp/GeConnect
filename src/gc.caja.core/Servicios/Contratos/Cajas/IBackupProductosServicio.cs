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
        Task<bool> InicializarBackup(string cajaId, string usuarioId);
        Task<bool> GuardarProducto(ProductoDatosResponseDto producto, string cajaId, string usuarioId);
        Task<List<ProductoDatosResponseDto>> RecuperarBackup(string cajaId, string usuarioId);
        Task<bool> LimpiarBackup(string cajaId, string usuarioId);
        Task<bool> ExisteBackup(string cajaId, string usuarioId);
    }
}
