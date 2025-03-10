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
        private readonly ILogger _logger;
        public ConsultaControladorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
            _logger = logger;
        }

        public List<ConsCtaCteDto> CuentaCorrienteBuscada
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("CuentaCorrienteBuscada");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ConsCtaCteDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("CuentaCorrienteBuscada", json);
            }
        }
    }
}
