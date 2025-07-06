using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Consultas.Controllers
{
    public class ConsultaControladorBase:ControladorBase
    {
        private readonly AppSettings _setting;
        //private readonly ILogger _logger;
        public ConsultaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
           //_logger = logger;
        }

       
    }
}
