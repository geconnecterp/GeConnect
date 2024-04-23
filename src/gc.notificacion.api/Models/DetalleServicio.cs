using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Options;

namespace gc.notificacion.api.Models
{
    public class DetalleServicio
    {
        private readonly ClaveSettings _settings;
        private readonly ILogger<DetalleServicio> _logger;
        public DetalleServicio(IOptions<ClaveSettings> options, ILogger<DetalleServicio> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }
        public async Task<bool> ObtenerDetalleVenta(string idVenta)
        {
            try
            {

            }
            catch (Exception ex)
            {

                throw;
            }

            return true;
        }
    }
}
