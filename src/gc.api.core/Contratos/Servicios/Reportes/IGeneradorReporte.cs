using gc.infraestructura.Dtos.Gen;
using iTextSharp.text;

namespace gc.api.core.Contratos.Servicios.Reportes
{
    public interface IGeneradorReporte
    {
        string Generar(ReporteSolicitudDto solicitud);
    }
}
