using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Dtos.Cajas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class ReportesConfigService : IReportesConfigService
    {
        private readonly List<ReporteConfig> _reportes;
        private readonly ILogger<ReportesConfigService> _logger;

        public ReportesConfigService(IConfiguration configuration, ILogger<ReportesConfigService> logger)
        {
            _logger = logger;

            try
            {
                // ❶ Leer sección "Reportes" del appsettings.json
                _reportes = configuration.GetSection("Reportes").Get<List<ReporteConfig>>() ?? new List<ReporteConfig>();

                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation("📋 CONFIGURACIÓN DE REPORTES CARGADA");
                _logger.LogInformation("═══════════════════════════════════════════════════");
                _logger.LogInformation($"   Total de reportes configurados: {_reportes.Count}");

                foreach (var reporte in _reportes)
                {
                    _logger.LogInformation($"   ✅ [{reporte.Key}] {reporte.Nombre} → ID: {reporte.Id}");
                }

                _logger.LogInformation("═══════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error al cargar configuración de reportes");
                _reportes = new List<ReporteConfig>();
            }
        }

        public List<ReporteConfig> ObtenerTodos()
        {
            return _reportes;
        }

        public ReporteConfig? ObtenerPorKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("⚠️ ObtenerPorKey: Key vacía o nula");
                return null;
            }

            var keyNormalizada = key.Trim().ToUpperInvariant();
            var reporte = _reportes.FirstOrDefault(r => r.Key.Trim().ToUpperInvariant() == keyNormalizada);

            if (reporte == null)
            {
                _logger.LogWarning($"⚠️ No se encontró reporte con Key: '{key}'");
            }
            else
            {
                _logger.LogInformation($"✅ Reporte encontrado: [{reporte.Key}] {reporte.Nombre}");
            }

            return reporte;
        }

        public string? ObtenerIdPorKey(string key)
        {
            var reporte = ObtenerPorKey(key);
            return reporte?.Id;
        }
    }
}
