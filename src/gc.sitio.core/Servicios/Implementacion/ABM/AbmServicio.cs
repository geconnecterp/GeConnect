using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos.ABM;
using log4net.Filter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class AbmServicio : Servicio<Dto>, IAbmServicio
    {
        private const string RutaAPI = "/api/abm";


        private readonly AppSettings _appSettings;

        public AbmServicio(IOptions<AppSettings> options,ILogger<AbmServicio> logger):base(options,logger)
        {
             _appSettings = options.Value;
        }
        public async Task<RespuestaGenerica<RespuestaDto>> AbmConfirmar(AbmGenDto abmGen, string token)
        {
            try
            {
                ApiResponse<RespuestaDto>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(abmGen, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    return new RespuestaGenerica<RespuestaDto> { Ok = true, Entidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        throw new NegocioException(error.Detail);
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        throw new NegocioException(error.Detail);
                    }
                    else
                    {
                        throw new Exception(error.Detail);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar confirmar los datos del ABM.");
            }
        }
    }
}
