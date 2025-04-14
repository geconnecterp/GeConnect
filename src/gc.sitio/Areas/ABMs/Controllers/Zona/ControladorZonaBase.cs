using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Zona
{
    public class ControladorZonaBase : ControladorBase
    {
        private readonly AppSettings _setting;
        public ControladorZonaBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
        }

        public List<ABMZonaDto> ZonasBuscardo
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ZonasBuscardo");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ABMZonaDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ZonasBuscardo", json);
            }
        }

        protected ZonaDto ZonaSeleccionada
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("ZonaSeleccionada");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new ZonaDto();
                }
                if(!string.IsNullOrEmpty(json))
                {
                    return JsonConvert.DeserializeObject<ZonaDto>(json);
                }
                else
                {
                    return new ZonaDto();
                }
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ZonaSeleccionada", json);
            }
        }
    }
}
