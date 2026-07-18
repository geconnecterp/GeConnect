using gc.api.core.Contratos.Servicios.SolAuth;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace gc.infraestructura.Workers
{
    public class ExpiracionSolicitudWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiracionSolicitudWorker> _logger;

        public ExpiracionSolicitudWorker(IServiceProvider serviceProvider, ILogger<ExpiracionSolicitudWorker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Currently the Expiration SP isn't fully defined in the Spanish script for getting pendings specifically expired
            // But we will leave the structure translated. 
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var servicioAutorizacion = scope.ServiceProvider.GetRequiredService<ISolicitudAuthServicio>();

                    await servicioAutorizacion.ExpirarSolicitudesPendientesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error en ExpiracionSolicitudWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
