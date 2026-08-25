using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen.Tr.Remito;
using gc.infraestructura.Dtos.Financieros;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Users;
using gc.infraestructura.Dtos.Users.Request;
using gc.infraestructura.Dtos.Seguridad;
using gc.sitio.core.Servicios.Contratos.Users;
using log4net.Filter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.Users
{
    public class UserServicio : Servicio<UserDto>, IUserServicio
    {
        private const string RutaAPI = "/api/usuarios";
        private const string BUSCAR_USUARIOS = "/BuscarUsuarios";
        private const string BUSCAR_USUARIOS_DATOS = "/BuscarUsuarioDatos";
        private const string OBTENER_PERFILES_USUARIO = "/ObtenerPerfilesDelUsuario";
        private const string OBTENER_ADM_USUARIO = "/ObtenerAdministracionesDelUsuario";
        private const string OBTENER_DER_USUARIO = "/ObtenerDerechosDelUsuario";
		private const string BUSCAR_USUARIOS_LISTA = "/BuscarUsuariosParaLista";
		private const string OBTENER_USUARIOS_LISTA = "/BuscarUsuarioLista";
        private const string OPERACIONES_SEGURIDAD = "/OperacionesSeguridadDisponibles";
        private const string BLANQUEAR_CLAVE = "/BlanquearClave";
        private const string DESBLOQUEAR_USUARIO = "/DesbloquearUsuario";

		private readonly AppSettings _appSettings;

        public UserServicio(IOptions<AppSettings> options, ILogger<UserServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }


        public async Task<RespuestaGenerica<UserDto>> BuscarUsuarioDatos(string usuId, string token)
        {
            try
            {
                ApiResponse<UserDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_USUARIOS_DATOS}?userId={usuId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<UserDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<UserDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Algo no fue bien con la API de Usuarios");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<UserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Perfil de Usuarios." };
            }
        }

        public async Task<(List<UserDto>, MetadataGrid)> BuscarUsuarios(QueryFilters filtro, string token)
        {
            try
            {
                ApiResponse<List<UserDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_USUARIOS}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UserDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new());
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException))==true)
                    {
                        throw new NegocioException(error.Detail?? "Algo no fue bien con la API de Usuarios");
                    }
                    else if(error!=null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Algo no fue bien con la API de Usuarios");
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

        public async Task<RespuestaGenerica<AdmUserDto>> ObtenerAdministracionesDelUsuario(string usuId, string token)
        {
            try
            {
                ApiResponse<List<AdmUserDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_ADM_USUARIO}?userId={usuId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AdmUserDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<AdmUserDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Algo no fue bien con la API de Usuarios");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<AdmUserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las administraciones del Usuario." };
            }
        }

        public async Task<RespuestaGenerica<DerUserDto>> ObtenerDerechosDelUsuario(string usuId, string token)
        {
            try
            {
                ApiResponse<List<DerUserDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_DER_USUARIO}?userId={usuId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<DerUserDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<DerUserDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Algo no fue bien con la API de Usuarios");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<DerUserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los derechos del Usuario." };
            }
        }

        public async Task<RespuestaGenerica<PerfilUserDto>> ObtenerPerfilesDelUsuario(string usuId, string token)
        {
            try
            {
                ApiResponse<List<PerfilUserDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_PERFILES_USUARIO}?userId={usuId}";

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
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Algo no fue bien con la API de Usuarios");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<PerfilUserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los perfiles del Usuario." };
            }
        }

		public List<UserDto> ObtenerUsuarioParaLista(BuscarUsuarioRequest request, string token)
		{
			ApiResponse<List<UserDto>> apiResponse;

			HelperAPI helper = new();
			HttpClient client = helper.InicializaCliente(request, token, out StringContent contentData);
			HttpResponseMessage response;

			var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_USUARIOS_LISTA}";

			response = client.PostAsync(link, contentData).Result;

			if (response.StatusCode == HttpStatusCode.OK)
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				if (string.IsNullOrEmpty(stringData))
				{
					_logger.LogWarning($"La API devolvió error.");
					return [];
				}
				apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UserDto>>>(stringData) ?? throw new Exception("Error al deserializar la respuesta de la API.");
				return apiResponse.Data;
			}
			else
			{
				string stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
				return new();
			}
		}

		public async Task<RespuestaGenerica<UserDto>> BuscarUsuarioLista(string admId, string token)
		{
			try
			{
				ApiResponse<List<UserDto>> apiResponse;

				HelperAPI helper = new();

				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;

				var link = $"{_appSettings.RutaBase}{RutaAPI}{OBTENER_USUARIOS_LISTA}?adm_id={admId}";

				response = await client.GetAsync(link);

				if (response.StatusCode == HttpStatusCode.OK)
				{
					string stringData = await response.Content.ReadAsStringAsync();
					if (string.IsNullOrEmpty(stringData))
					{

						return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
					}
					apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<UserDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

					return new RespuestaGenerica<UserDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

				}
				else
				{
					string stringData = await response.Content.ReadAsStringAsync();
					_logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
					var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
					if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
					{
						throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios.");
					}
					else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
					{
						throw new NegocioException(error.Detail ?? "Algo no fue bien con la API de Usuarios");
					}
					else if (error != null)
					{
						throw new Exception(error.Detail);
					}
					else
					{
						throw new Exception("Algo no fue bien con la API de Usuarios");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

				return new RespuestaGenerica<UserDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los derechos del Usuario." };
			}
		}

        public async Task<OperacionesSeguridadUsuarioDto> ObtenerOperacionesSeguridad(string token)
        {
            using var client = new HelperAPI().InicializaCliente(token);
            using var response = await client.GetAsync($"{_appSettings.RutaBase}{RutaAPI}{OPERACIONES_SEGURIDAD}");
            if (!response.IsSuccessStatusCode)
                return new OperacionesSeguridadUsuarioDto();

            var body = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<ApiResponse<OperacionesSeguridadUsuarioDto>>(body)?.Data
                ?? new OperacionesSeguridadUsuarioDto();
        }

        public Task<CambioClaveResultadoDto> BlanquearClave(string usuarioObjetivo, string token, string? ip) =>
            EjecutarOperacionSeguridad(BLANQUEAR_CLAVE, usuarioObjetivo, token, ip);

        public Task<CambioClaveResultadoDto> DesbloquearUsuario(string usuarioObjetivo, string token, string? ip) =>
            EjecutarOperacionSeguridad(DESBLOQUEAR_USUARIO, usuarioObjetivo, token, ip);

        private async Task<CambioClaveResultadoDto> EjecutarOperacionSeguridad(string ruta,
            string usuarioObjetivo, string token, string? ip)
        {
            var request = new OperacionUsuarioSeguridadRequestDto { UsuarioObjetivo = usuarioObjetivo };
            using var client = new HelperAPI().InicializaCliente(request, token, out StringContent content);
            if (!string.IsNullOrWhiteSpace(ip))
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-ClientUsr", ip);

            using var response = await client.PostAsync($"{_appSettings.RutaBase}{RutaAPI}{ruta}", content);
            var body = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new CambioClaveResultadoDto
                {
                    resultado = response.StatusCode == HttpStatusCode.Forbidden ? (short)2 : (short)1,
                    resultado_id = response.StatusCode == HttpStatusCode.Forbidden ? "SIN_DERECHO" : "SOLICITUD_INVALIDA",
                    resultado_msj = response.StatusCode == HttpStatusCode.Forbidden
                        ? "No posee el derecho requerido para realizar esta operación."
                        : (string.IsNullOrWhiteSpace(body) ? "No se pudo procesar la operación." : body.Trim('"'))
                };
            }

            return JsonConvert.DeserializeObject<ApiResponse<CambioClaveResultadoDto>>(body)?.Data
                ?? new CambioClaveResultadoDto
                {
                    resultado = -1,
                    resultado_id = "SIN_RESPUESTA",
                    resultado_msj = "La API no devolvió una respuesta válida."
                };
        }
	}
}
