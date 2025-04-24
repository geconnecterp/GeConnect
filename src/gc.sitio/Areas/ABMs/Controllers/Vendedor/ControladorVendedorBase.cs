using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.ABMs.Controllers.Vendedor
{
    public class ControladorVendedorBase:ControladorBase
    {
        private readonly AppSettings _setting;
        public ControladorVendedorBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
        }

        public List<ABMVendedorDto> VendedoresBuscardo
        {

            get
            {
                var json = _context.HttpContext?.Session.GetString("VendedoresBuscardo");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<ABMVendedorDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("VendedoresBuscardo", json);
            }
        }

        protected ABMVendedorDatoDto VendedorSeleccionado
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString("VendedorSeleccionado");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new ABMVendedorDatoDto();
                }
                return JsonConvert.DeserializeObject<ABMVendedorDatoDto>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("VendedorSeleccionado", json);
            }
        }
    }
}
