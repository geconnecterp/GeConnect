using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Users;
using gc.sitio.Controllers;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Usuarios.Controllers
{
    public class ControladorUsuariosBase:ControladorBase
    {
        private readonly AppSettings _setting;
        private readonly ILogger _logger;
        public ControladorUsuariosBase(IOptions<AppSettings> options, IHttpContextAccessor accessor, ILogger logger) : base(options, accessor, logger)
        {
            _setting = options.Value;
            _logger = logger;
        }

        public int PaginaActual
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("PaginaActual");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return 0;
                }
                return txt.ToInt();
            }
            set
            {
                var valor = value.ToString();
                _context.HttpContext.Session.SetString("PaginaActual", valor);
            }
        }

        public List<PerfilDto> PerfilesBuscados
        {
            get
            {
                var json = _context.HttpContext.Session.GetString("PerfilesBuscados");
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return [];
                }
                return JsonConvert.DeserializeObject<List<PerfilDto>>(json);
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("PerfilesBuscados", json);
            }
        }

        public MetadataGrid MetadataPerfil
        {
            get
            {
                var txt = _context.HttpContext.Session.GetString("MetadataPerfil");
                if (string.IsNullOrEmpty(txt) || string.IsNullOrWhiteSpace(txt))
                {
                    return new MetadataGrid();
                }
                return JsonConvert.DeserializeObject<MetadataGrid>(txt); ;
            }
            set
            {
                var valor = JsonConvert.SerializeObject(value);
                _context.HttpContext.Session.SetString("MetadataPerfil", valor);
            }

        }
    }
}
