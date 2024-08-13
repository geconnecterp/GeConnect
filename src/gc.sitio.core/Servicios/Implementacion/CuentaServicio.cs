using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class CuentaServicio : Servicio<CuentaDto>, ICuentaServicio
    {
        private const string RutaAPI = "/api/apicuenta";
        private const string ProveedorLista = "/GetProveedorLista";
        private readonly AppSettings _appSettings;
        public CuentaServicio(IOptions<AppSettings> options,ILogger<CuentaServicio> logger):base(options,logger)
        {
                _appSettings = options.Value;
        }
        public List<ProveedorListaDto> ObtenerListaProveedores(string token)
        {
            ApiResponse<List<ProveedorListaDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response ;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{ProveedorLista}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorListaDto>>>(stringData);
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API con los datos de las cuentas de proveedores. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener los Proveedores: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener los proveedores");
                }

            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener los Proveedores.");
                throw;
            }
        }
    }
}
