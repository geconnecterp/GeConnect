using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ProveedorServicio : Servicio<ProveedorDto>, IProveedorServicio
    {
        
        public ProveedorServicio(IOptions<AppSettings> options,ILogger logger):base(options,logger)
        {
            
        }
    }
}
