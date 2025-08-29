using gc.infraestructura.Core.EntidadesComunes.Options;
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
                var json = _context.HttpContext?.Session.GetString("ProvedoresParaActualizar") ?? string.Empty;
                if (string.IsNullOrEmpty(json) || string.IsNullOrWhiteSpace(json))
                {
                    return new List<ActualizaProveedorDto>();
                }
                return JsonConvert.DeserializeObject<List<ActualizaProveedorDto>>(json) ?? [];
            }
            set
            {
                var json = JsonConvert.SerializeObject(value);
                _context.HttpContext?.Session.SetString("ProvedoresParaActualizar", json);
            }
        }
    }
}
