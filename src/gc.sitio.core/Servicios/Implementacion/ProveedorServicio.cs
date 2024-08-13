using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ProveedorServicio : Servicio<ProveedorDto>, IProveedorServicio
    {
        private const string RutaAPI = "/api/apiproveedor";
        private readonly AppSettings _appSettings;

        public ProveedorServicio(IOptions<AppSettings> options,ILogger<ProveedorServicio> logger):base(options,logger,RutaAPI)
        {
            _appSettings = options.Value;
        }
    }
}
