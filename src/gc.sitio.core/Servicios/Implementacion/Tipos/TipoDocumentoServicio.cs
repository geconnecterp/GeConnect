using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class TipoDocumentoServicio :Servicio<TipoDocumentoDto>, ITipoDocumentoServicio
    {
        private const string RutaAPI = "/api/tipodocumento";
        private readonly AppSettings _appSettings;

        public TipoDocumentoServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        
    }
}
