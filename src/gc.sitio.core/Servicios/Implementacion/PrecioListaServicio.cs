using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class PrecioListaServicio : Servicio<Dto>,IPrecioListaServicio
    {
        public PrecioListaServicio(IOptions<AppSettings> options, ILogger<EtiquetaServicio> logger) : base(options, logger)
        {

        }
    }
}
