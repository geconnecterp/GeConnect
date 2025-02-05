using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Administracion;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Almacen.Tr;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class AdministracionServicio : Servicio<AdministracionDto>, IAdministracionServicio
    {
        private const string RutaAPI = "/api/administracion";
        private const string AccionLogin = "/GetAdministraciones4Login";
        private const string TI_VALIDA_USU = "/TIValidarUsuario";
        private const string ObtenerAdministracionesUrl = "/ObtenerAdministraciones";


		private readonly AppSettings _appSettings;

        public AdministracionServicio(IOptions<AppSettings> options, ILogger<AdministracionServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

		public List<AdministracionDto> ObtenerAdministraciones(string adm_activa, string token)
		{
			ApiResponse<List<AdministracionDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{ObtenerAdministracionesUrl}?adm_activa={adm_activa}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<AdministracionDto>>>(stringData);
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta administrativa. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta administrativa: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta administrativa");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta administrativa.");
				throw;
			}
		}

		public List<AdministracionLoginDto> GetAdministracionLogin()
        {
            ApiResponse<List<AdministracionLoginDto>> respuesta;
            string stringData;
            try
            {
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente("");
                HttpResponseMessage response ;
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

		public async Task<ResponseBaseDto> ValidarUsuario(string userId, string tipo,string tiId, string token)
        {
            ApiResponse<ResponseBaseDto> apiResponse;

            HelperAPI helper = new HelperAPI();
            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{TI_VALIDA_USU}?tipo={tipo}&id={userId}&usu={tiId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.Unauthorized) {
                throw new UnauthorizedException("No se autorizó el acceso al servidor. Salga y vuelva a ingresar en el sistema.");
            }
        
            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {JsonConvert.SerializeObject(response)}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<ResponseBaseDto>>(stringData) ?? new ApiResponse<ResponseBaseDto>(new ResponseBaseDto());
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");

                try
                {                
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
                    return new();
                }
                catch
                {
                    return new();
                }
            }
        }
    }    
}
