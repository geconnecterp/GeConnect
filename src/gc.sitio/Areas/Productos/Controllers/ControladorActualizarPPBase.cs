using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos.Actualiza;
using gc.sitio.Controllers;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace gc.sitio.Areas.Productos.Controllers
{
    public class ControladorActualizarPPBase:ControladorBase
    {
        private const string ProveedoresSessionKey = "ProvedoresParaActualizar";
        private const string MetadataSessionKey = "ActualizarPP.Metadata";

        public ControladorActualizarPPBase(IOptions<AppSettings> options,
           IHttpContextAccessor contexto,ILogger logger) :base(options,contexto,logger)
        {
            
        }

        /// <summary>
        /// Contendrá los Proveedores que tiene productos para actualizar.
        /// </summary>
        public List<ActualizaProveedorDto> ProvedoresParaActualizar
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString(ProveedoresSessionKey) ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new List<ActualizaProveedorDto>();
                }
                return JsonConvert.DeserializeObject<List<ActualizaProveedorDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString(ProveedoresSessionKey, json);
            }
        }

        protected MetadataGrid MetadataActualizacionPrecios
        {
            get
            {
                var json = _context.HttpContext?.Session.GetString(MetadataSessionKey) ?? string.Empty;
                return string.IsNullOrWhiteSpace(json)
                    ? new MetadataGrid()
                    : JsonConvert.DeserializeObject<MetadataGrid>(json) ?? new MetadataGrid();
            }
            set
            {
                var json = JsonConvert.SerializeObject(value ?? new MetadataGrid());
                _context.HttpContext?.Session.SetString(MetadataSessionKey, json);
            }
        }

        protected void ReiniciarEstadoActualizacionPrecios()
        {
            _context.HttpContext?.Session.Remove(ProveedoresSessionKey);
            _context.HttpContext?.Session.Remove(MetadataSessionKey);
        }

        protected void ReiniciarDetalleActualizacionPrecios()
        {
            _context.HttpContext?.Session.Remove(MetadataSessionKey);
        }
    }
}
