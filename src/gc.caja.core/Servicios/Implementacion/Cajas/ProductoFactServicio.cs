using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class ProductoFactServicio: Servicio<Dto>, IProductoFactServicio
    {
        private const string RutaAPI = "/api/apiproductofact";

        public ProductoFactServicio(IOptions<AppSettings> options, ILogger<ProductoFactServicio> logger) : base(options, logger)
        {
        }


    }
}
