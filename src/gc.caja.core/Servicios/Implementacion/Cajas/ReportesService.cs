using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class ReportesService : IReportesService
    {
        private const string RutaAPI = "/api/reportes";
        private const string RutaGenerar = "/generate";

        private readonly IConfiguration _configuration;
        private readonly ILogger<ReportesService> _logger;

        public ReportesService(IConfiguration configuration, ILogger<ReportesService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<RespuestaReportDto> ObtenerPdfDesdeAPI(ReporteSolicitudDto reporteSolicitud, string token)
        {
            try
            {
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("📡 INVOCAR API DE REPORTES");
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"   Reporte ID: {reporteSolicitud.Reporte}");
                _logger.LogInformation($"   Parámetros: {reporteSolicitud.Parametros.Count}");

                // ❶ Obtener URL de la API de Reportes desde configuración
                var apiReporteUrl = _configuration.GetValue<string>("DocsManager:ApiReporteUrl");

                if (string.IsNullOrWhiteSpace(apiReporteUrl))
                {
                    throw new InvalidOperationException("No se encontró configuración 'DocsManager:ApiReporteUrl' en appsettings.json");
                }

                var urlCompleta = $"{apiReporteUrl}{RutaAPI}{RutaGenerar}";
                _logger.LogInformation($"   URL: {urlCompleta}");

                // ❷ Inicializar cliente HTTP
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(reporteSolicitud, token, out StringContent content);

                _logger.LogInformation("   ⏳ Enviando solicitud HTTP POST...");

                // ❸ Enviar solicitud
                var response = await client.PostAsync(urlCompleta, content);

                _logger.LogInformation($"   📥 Status Code: {response.StatusCode}");

                // ❹ Validar respuesta
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"❌ Error en API de Reportes: {errorContent}");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Error al comunicarse con la API de Reportes",
                        Base64 = string.Empty
                    };
                }

                // ❺ Leer contenido
                var stringData = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    _logger.LogError("❌ La API de Reportes devolvió respuesta vacía");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "La API de Reportes no devolvió datos",
                        Base64 = string.Empty
                    };
                }

                // ❻ Deserializar respuesta
                var respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(stringData);

                if (respuesta == null || respuesta.Data == null)
                {
                    _logger.LogError("❌ Error al deserializar respuesta de API de Reportes");

                    return new RespuestaReportDto
                    {
                        resultado = -1,
                        resultado_msj = "Error al procesar respuesta de la API",
                        Base64 = string.Empty
                    };
                }

                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("✅ PDF OBTENIDO EXITOSAMENTE");
                _logger.LogInformation($"   Tamaño Base64: {respuesta.Data.Base64?.Length ?? 0} caracteres");
                _logger.LogInformation($"   Resultado: {respuesta.Data.resultado}");
                _logger.LogInformation($"   Mensaje: {respuesta.Data.resultado_msj}");
                _logger.LogInformation("═══════════════════════════════════════════════════");

                return respuesta.Data;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "❌ Error de conexión HTTP con la API de Reportes");

                return new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "No se pudo conectar con el servicio de reportes. Verifique la conexión.",
                    Base64 = string.Empty
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error inesperado al obtener PDF desde API de Reportes");

                return new RespuestaReportDto
                {
                    resultado = -1,
                    resultado_msj = "Error interno al generar el reporte",
                    Base64 = string.Empty
                };
            }
        }
    }
}
