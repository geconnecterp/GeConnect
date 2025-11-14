using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Presupuestos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class EtiquetaServicio :Servicio<Dto>, IEtiquetaServicio
    {
        private const string RutaAPI = "/api/apiEtiqueta";
        private const string OBTENER_CARGA_PREVIA = "/ObtenerCargaPreviaUsuario/";

        public EtiquetaServicio(IOptions<AppSettings> options, ILogger<EtiquetaServicio> logger) : base(options, logger)
        {

        }

        public async Task<RespuestaGenerica<CargaPreviaDto>> ObtenerCargaPrevia(string adm_id, string token)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(adm_id))
                {
                    return new() { Ok = false, Mensaje = "Debe indicar la sucursal actual." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_CARGA_PREVIA}{adm_id}";
                return await GetListaAsync<CargaPreviaDto>(link, token, "Error al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener el Presupuesto" };
            }
        }
    }
}
