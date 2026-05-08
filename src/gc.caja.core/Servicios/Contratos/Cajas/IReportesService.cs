using gc.infraestructura.Dtos.Gen;

namespace gc.caja.core.Servicios.Contratos.Cajas
{
    /// <summary>
    /// ✅ NUEVO v10.0: Servicio para invocar la API de Reportes
    /// Extraído y adaptado de DocManagerServicio para uso en gc.caja
    /// </summary>
    public interface IReportesService
    {
        /// <summary>
        /// Obtiene un PDF desde la API de Reportes en formato Base64
        /// </summary>
        Task<RespuestaReportDto> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token);
    }
}
