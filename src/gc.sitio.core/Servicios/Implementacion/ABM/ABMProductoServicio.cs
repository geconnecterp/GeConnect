using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class ABMProductoServicio : Servicio<ProductoListaDto>,IABMProductoServicio
    {
        private const string RUTABASE = "/api/abmproducto";
        public ABMProductoServicio(IOptions<AppSettings> options, ILogger<ABMProductoServicio> logger):base(options,logger,RUTABASE)
        {
            
        }
    }
}
