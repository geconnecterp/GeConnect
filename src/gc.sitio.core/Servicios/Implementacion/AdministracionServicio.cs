using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Administracion;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class AdministracionServicio : Servicio<AdministracionDto>, IAdministracionServicio
    {
        private const string RutaAPI = "/api/administracion";
        private const string AccionLogin = "/GetAdministraciones4Login";
        private readonly AppSettings _appSettings;

        public AdministracionServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public List<AdministracionLoginDto> GetAdministracionLogin()
        {
            ApiResponse<List<AdministracionLoginDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente("");
                HttpResponseMessage response = null;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{AccionLogin}";
                response = client.GetAsync(link).GetAwaiter().GetResult();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    respuesta = JsonConvert.DeserializeObject<ApiResponse<List<AdministracionLoginDto>>>(stringData);

                    return respuesta.Data;
                }
                else
                {
                    stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    _logger.LogError($"Error al intentar obtener Administarciones para el Combo: {stringData}");
                    throw new NegocioException("Hubo un error al intentar obtener las Administraciones para el Combo");
                }

            }
            catch (NegocioException )
            {
                throw ;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al intentar obtener Administarciones para el Combo.");
                throw;
            }
        }
    }
}
