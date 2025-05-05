using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
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
        private const string BUSCAR_PERFILES_USERS = "/GetUsuariosxPerfiles";
        private const string GET_MENU = "/GetMenu";
        private const string GET_MENU_ITEMS = "/GetMenuItems";
        private const string DEFINE_PERFIL_DEFAULT = "/DefinePerfilDefault";
        private const string OBTENER_MENU_USU = "/ObtenerMenu";
        


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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PerfilDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return (apiResponse.Data ?? [], apiResponse.Meta??new());
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail?? throw new NegocioException("Algo no fue bien con la API de Menu."));
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? throw new NegocioException("Algo no fue bien con la API de Menu."));
                    }
                    else if(error!=null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PERFIL}?id={id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<PerfilDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<PerfilDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error!=null && error.TypeException?.Equals(nameof(NegocioException))==true)
                    {
                        return new RespuestaGenerica<PerfilDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException))==true)
                    {
                        return new RespuestaGenerica<PerfilDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if(error !=null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PERFILES_USERS}?id={perfilId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PerfilUserDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<PerfilUserDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Perfil de Usuarios." };
            }
        }

        public async Task<RespuestaGenerica<MenuDto>> GetMenu(string token)
        {
            try
            {
                ApiResponse<List<MenuDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_MENU}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MenuDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<MenuDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<MenuDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<MenuDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<MenuDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Menú." };
            }
        }

        public async Task<RespuestaGenerica<MenuItemsDto>> GetMenuItems(string menuId, string perfil, string token)
        {
            try
            {
                ApiResponse<List<MenuItemsDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{GET_MENU_ITEMS}?menuId={menuId}&perfil={perfil}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MenuItemsDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<MenuItemsDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<MenuItemsDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<MenuItemsDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<MenuItemsDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los items del menú." };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> DefinePerfilDefault(PerfilUserDto perfilUser,string token)
        {
            try
            {
                ApiResponse<RespuestaDto >? apiResponse;
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(perfilUser, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{DEFINE_PERFIL_DEFAULT}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<RespuestaDto> { Ok = true, Entidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar definir al usuario actual, un perfil como predeterminado." };
            }
        }


        public async Task<RespuestaGenerica<MenuPpalDto>> ObtenerMenu(string perfilId, string user, string menuId, string adm,string token)
        {
            try
            {
                ApiResponse<List<MenuPpalDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_MENU_USU}?perfilId={perfilId}&user={user}&menuId={menuId}&adm={adm}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MenuPpalDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos") ;

                    return new RespuestaGenerica<MenuPpalDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<MenuPpalDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<MenuPpalDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<MenuPpalDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el menú del usuario." };
            }
        }
    }
}
