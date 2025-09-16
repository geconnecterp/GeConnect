using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorOfertaBase:ControladorBase
    {
        public ControladorOfertaBase(IOptions<AppSettings> options, IHttpContextAccessor contexto, ILogger logger)
            :base(options,contexto,logger)
        {
            
        }
    }
}
