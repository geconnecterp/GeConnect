using gc.api.core.Contratos.Servicios.Reportes;
using gc.infraestructura.Dtos.Gen;
using Microsoft.AspNetCore.Mvc;

namespace gc.api.report.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly IPdfReportService _pdfReportService;
        private readonly ILogger<ReportesController> _logger;
        private readonly IWebHostEnvironment _env;

        public ReportesController(IPdfReportService pdfReportService, ILogger<ReportesController> logger, IWebHostEnvironment env)
        {
            _pdfReportService = pdfReportService;
            _logger = logger;
            _env = env;
        }

        [HttpPost("generate")]
        public IActionResult GenerateReport([FromBody] ReporteSolicitudDto request)
        {
            _logger.LogInformation("Generating report with ID: {ReportId}", request.Reporte);

            try
            {
                string logoPath = Path.Combine(_env.ContentRootPath, "img", "gc.png");
                request.LogoPath = logoPath;
                var base64 = _pdfReportService.GenerateReportAsBase64(request);
                return Ok(new { fileBase64 = base64 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report with ID: {ReportId}", request.Reporte);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
