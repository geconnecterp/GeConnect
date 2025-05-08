using gc.infraestructura.Dtos.Gen;

namespace gc.api.core.Contratos.Servicios.Reportes
{
    public interface IReportService
    {
        string GenerarReporteFormatoExcel(ReporteSolicitudDto request);
        string GenerarReporteFormatoTxt(ReporteSolicitudDto request);
        string GenerateReportAsBase64(ReporteSolicitudDto request);
    }
}
