using gc.infraestructura.Dtos.Gen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.api.core.Contratos.Servicios.Reportes
{
    public interface IPdfReportService
    {
        string GenerateReportAsBase64(ReporteSolicitudDto request);
    }
}
