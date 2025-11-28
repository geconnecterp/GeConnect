using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class PrecioListaServicio : Servicio<Dto>,IPrecioListaServicio
    {
        private const string RutaAPI = "/api/apipreciolista";
        private const string OBTENER_LISTA_PRECIOS = "/ObtenerListaPrecios/";
        public PrecioListaServicio(IOptions<AppSettings> options, ILogger<EtiquetaServicio> logger) : base(options, logger)
        {

        }

        public async Task<RespuestaGenerica<PrecioListaDto>> ObtenerListaPrecios(string token)
        {
            try
            {               

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_LISTA_PRECIOS}";
                return await GetListaAsync<PrecioListaDto>(link, token, "Error al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener la Lista de Precios" };
            }
        }
    }
}
