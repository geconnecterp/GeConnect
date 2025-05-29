using gc.api.core.Contratos.Servicios.Reportes;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace gc.api.report.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportesController : ControllerBase
    {
        private readonly IReportService _pdfReportService;
        private readonly ILogger<ReportesController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly EmpresaGeco _empresaGeco;

        public ReportesController(IReportService pdfReportService, ILogger<ReportesController> logger,
            IWebHostEnvironment env,IOptions<EmpresaGeco> emp)
        {
            _pdfReportService = pdfReportService;
            _logger = logger;
            _env = env;
            _empresaGeco = emp.Value;
        }

        [HttpPost("generate")]
        public IActionResult GenerateReport([FromBody] ReporteSolicitudDto request)
        {
            _logger.LogInformation("Generating report with ID: {ReportId}", request.Reporte);

            try
            {
                string logoPath = Path.Combine(_env.ContentRootPath, "img", _empresaGeco.Logo);
                request.LogoPath = logoPath;
                var base64 = _pdfReportService.GenerateReportAsBase64(request);

                //devolver apiresponse entidad 
                return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = base64, resultado = 0, resultado_msj = $"{request.Reporte.ToString()}.pdf" }));
            }
            catch (NegocioException ex)
            {
                return  Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = string.Empty, resultado = -1, resultado_msj = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report with ID: {ReportId}", request.Reporte);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("GenFileFormat")]
        public IActionResult GeneraReporteFormato([FromBody] ReporteSolicitudDto request)
        {
            _logger.LogInformation("Generating report with ID: {ReportId}", request.Reporte);

            try
            {
                switch (request.Formato)
                {
                    case "P":
                        string logoPath = Path.Combine(_env.ContentRootPath, "img", "gc.png");
                        request.LogoPath = logoPath;
                        var base64 = _pdfReportService.GenerateReportAsBase64(request);

                        //devolver apiresponse entidad 
                        return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = base64, resultado = 0, resultado_msj = $"{request.Reporte.ToString()}.pdf" }));          
                    case "X":
                        string base64x = _pdfReportService.GenerarReporteFormatoExcel(request);
                        //devolver apiresponse entidad 
                        return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = base64x, resultado = 0, resultado_msj = $"{request.Reporte.ToString()}.xls" }));
                    case "T":
                        string base64t = _pdfReportService.GenerarReporteFormatoTxt(request);
                        //devolver apiresponse entidad 
                        return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = base64t, resultado = 0, resultado_msj =$"{request.Reporte.ToString()}.txt"  }));
                    default:
                        throw new NegocioException("No se ha especificado un formato correcto de archivo");
                }
                
            }
            catch (NegocioException ex)
            {
                return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = string.Empty, resultado = -1, resultado_msj = ex.Message }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating report with ID: {ReportId}", request.Reporte);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
