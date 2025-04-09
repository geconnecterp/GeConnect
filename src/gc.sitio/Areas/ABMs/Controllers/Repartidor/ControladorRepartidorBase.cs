using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Repartidor
{
    public class ControladorRepartidorBase : ControladorBase
    {
        private readonly AppSettings _setting;
        public ControladorRepartidorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
        }

        public List<ABMRepartidorDto> RepartidoresBuscardo
        {

            get
            {
                var json = _context.HttpContext.Session.GetString("RepartidoresBuscardo");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ABMRepartidorDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RepartidoresBuscardo", json);
            }
        }

        protected ABMRepartidorDatoDto RepartidorSeleccionado
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("RepartidorSeleccionado");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new ABMRepartidorDatoDto();
                }
                return JsonConvert.DeserializeObject<ABMRepartidorDatoDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("RepartidorSeleccionado", json);
            }
        }
    }
}
