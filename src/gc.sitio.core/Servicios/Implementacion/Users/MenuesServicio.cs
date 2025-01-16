using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Users;
using gc.sitio.core.Servicios.Contratos.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Users
{
    public class MenuesServicio : Servicio<PerfilDto>, IMenuesServicio
    {
        private const string RutaAPI = "/api/menues";
        private const string BUSCAR_PERFILES = "/GetPerfiles";
        private const string BUSCAR_PERFIL = "/GetPerfil";
        private const string BUSCAR_PERFILES_USERS = "/GetPerfilUsers";


        private readonly AppSettings _appSettings;
        public MenuesServicio(IOptions<AppSettings> options, ILogger<MenuesServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

   
        public async Task<(List<PerfilDto>, MetadataGrid)> GetPerfiles(QueryFilters filters, string token)
        {
            try
            {
                ApiResponse<List<PerfilDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filters, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PERFILES}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PerfilDto>>>(stringData);

                    return (apiResponse.Data, apiResponse.Meta);
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

                throw new Exception("Algo no fue bien al intentar obtener los Perfiles solicitados según el filtro actual.");
            }
        }

        public async Task<RespuestaGenerica<PerfilDto>> GetPerfil(string id, string token)
        {
            try
            {
                ApiResponse<PerfilDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PERFIL}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<PerfilDto>>(stringData);

                    return new RespuestaGenerica<PerfilDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<PerfilDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<PerfilDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<PerfilDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Perfil de Usuarios." };
            }
        }


        public async Task<RespuestaGenerica<PerfilUserDto>>  GetPerfilUsers(string perfilId, string token)
        {
            try
            {
                ApiResponse<List<PerfilUserDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PERFIL}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PerfilUserDto>>>(stringData);

                    return new RespuestaGenerica<PerfilUserDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Perfil de Usuarios." };
            }
        }
    }
}
