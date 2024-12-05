using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class ABMClienteServicio : Servicio<ABMClienteSearchDto>, IABMClienteServicio
    {
        private const string RUTABASE = "/api/abmcliente";
        public ABMClienteServicio(IOptions<AppSettings> options, ILogger<ABMClienteServicio> logger) : base(options, logger, RUTABASE)
        {

        }
    }
}
