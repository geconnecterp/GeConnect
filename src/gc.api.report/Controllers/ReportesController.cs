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
        public async Task<IActionResult> GenerateReport([FromBody] ReporteSolicitudDto request)
        {
            if ((int)request.Reporte == gc.api.core.Servicios.Reportes.IvaComprasResumenReporte.Id)
                return await GenerarResumenComprasIva(request, "P");
            if ((int)request.Reporte == gc.api.core.Servicios.Reportes.IvaVentasResumenReporte.Id)
                return await GenerarResumenIva(request, "P");
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
        public async Task<IActionResult> GeneraReporteFormato([FromBody] ReporteSolicitudDto request)
        {
            if ((int)request.Reporte == gc.api.core.Servicios.Reportes.IvaComprasResumenReporte.Id)
                return await GenerarResumenComprasIva(request, request.Formato);
            if ((int)request.Reporte == gc.api.core.Servicios.Reportes.IvaVentasResumenReporte.Id)
                return await GenerarResumenIva(request, request.Formato);
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
        // Adaptador exclusivo del nuevo reporte: conserva el contrato del gestor y
        // autoriza también llamadas directas a la API. Los reportes anteriores no cambian.
        private static readonly SemaphoreSlim ResumenesIva = new(2, 2);
        private async Task<IActionResult> GenerarResumenComprasIva(ReporteSolicitudDto request, string? formato)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();
            var adquirido = false;
            try
            {
                var servicio = HttpContext.RequestServices.GetRequiredService<gc.api.core.Servicios.Libros.IvaComprasResumenServicio>();
                var p = request.Parametros ?? new();
                if (!servicio.TieneAcceso(User, p.GetValueOrDefault("Perfil", ""), p.GetValueOrDefault("Menu", ""))) return Forbid();
                if (formato != "P") return BadRequest(new { mensaje = "Utilice Imprimir para PDF y Exportar Excel desde el módulo. TXT no disponible." });
                if (!int.TryParse(p.GetValueOrDefault("Periodo"), out var anio) || !int.TryParse(p.GetValueOrDefault("Mes"), out var mes))
                    return BadRequest(new { mensaje = "Seleccione año y mes válidos." });
                var filtro = new gc.infraestructura.Dtos.Libros.IvaComprasResumenFiltro { Periodo = anio, Mes = mes };
                filtro.Validar();
                adquirido = await ResumenesIva.WaitAsync(0, HttpContext.RequestAborted);
                if (!adquirido) return StatusCode(429, new { mensaje = "Hay otros reportes en curso. Intente nuevamente." });
                var libro = await servicio.Consultar(filtro, HttpContext.RequestAborted);
                var pdf = gc.api.core.Servicios.Reportes.IvaComprasResumenReporte.Pdf(libro, _empresaGeco,
                    Path.Combine(_env.ContentRootPath, "img", _empresaGeco.Logo), HttpContext.RequestAborted);
                Response.Headers.CacheControl = "no-store, private";
                Response.Headers["X-Content-Type-Options"] = "nosniff";
                return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = Convert.ToBase64String(pdf), resultado = 0,
                    resultado_msj = $"Libro_IVA_Compras_Resumen_{filtro.PeriodoTexto}.pdf" }));
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError("PDF IVA compras resumen fallido. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
                return StatusCode(503, new { mensaje = "No se pudo generar el resumen completo. No se entregó un PDF parcial." });
            }
            finally { if (adquirido) ResumenesIva.Release(); }
        }
        private async Task<IActionResult> GenerarResumenIva(ReporteSolicitudDto request, string? formato)
        {
            if (User.Identity?.IsAuthenticated != true) return Unauthorized();
            var adquirido = false;
            try
            {
                var servicio = HttpContext.RequestServices.GetRequiredService<gc.api.core.Servicios.Libros.IvaVentasResumenServicio>();
                var p = request.Parametros ?? new();
                if (!servicio.TieneAcceso(User, p.GetValueOrDefault("Perfil", ""), p.GetValueOrDefault("Menu", ""))) return Forbid();
                if (formato != "P") return BadRequest(new { mensaje = "Este reporte se imprime en PDF; utilice Exportar Excel en Consulta Resumen. TXT no disponible." });
                if (!int.TryParse(p.GetValueOrDefault("Periodo"), out var anio) || !int.TryParse(p.GetValueOrDefault("Mes"), out var mes))
                    return BadRequest(new { mensaje = "Seleccione año y mes válidos." });
                var filtro = new gc.infraestructura.Dtos.Libros.IvaVentasResumenFiltro { Periodo = anio, Mes = mes };
                filtro.Validar();
                adquirido = await ResumenesIva.WaitAsync(0, HttpContext.RequestAborted);
                if (!adquirido) return StatusCode(429, new { mensaje = "Hay otros reportes en curso. Intente nuevamente." });
                var filas = await servicio.Consultar(filtro, HttpContext.RequestAborted);
                var libro = gc.infraestructura.Dtos.Libros.IvaVentasResumenLibro.Crear(filtro, filas);
                var pdf = gc.api.core.Servicios.Reportes.IvaVentasResumenReporte.Pdf(libro, _empresaGeco,
                    Path.Combine(_env.ContentRootPath, "img", _empresaGeco.Logo), HttpContext.RequestAborted);
                Response.Headers.CacheControl = "no-store, private";
                return Ok(new ApiResponse<RespuestaReportDto>(new RespuestaReportDto { Base64 = Convert.ToBase64String(pdf), resultado = 0,
                    resultado_msj = $"Libro_IVA_Ventas_Resumen_{filtro.PeriodoTexto}.pdf" }));
            }
            catch (ArgumentException ex) { return BadRequest(new { mensaje = ex.Message }); }
            catch (OperationCanceledException) when (HttpContext.RequestAborted.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                _logger.LogError("PDF IVA resumen fallido. Tipo {Tipo}; referencia {Ref}", ex.GetType().Name, HttpContext.TraceIdentifier);
                return StatusCode(503, new { mensaje = "No se pudo generar el resumen completo. No se entregó un PDF parcial." });
            }
            finally { if (adquirido) ResumenesIva.Release(); }
        }
    }
}
