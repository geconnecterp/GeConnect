using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Contabilidad;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos.ABM;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Asn1.Cms;
using Org.BouncyCastle.Bcpg;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class ABMPlanCuentaServicio : Servicio<PlanCuentaDto>, IABMPlanCuentaServicio
    {
        private const string RutaAPI = "/api/ABMPlanCuenta";
        //private const string BUSCAR_VENDEDORES = "/ObtenerPlanCuentaes";
        //private const string BUSCAR_VENDEDOR = "/ObtenerPlanCuentaPorId";

        private readonly AppSettings _appSettings;
        public ABMPlanCuentaServicio(IOptions<AppSettings> options, ILogger<ABMClienteServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public async Task<RespuestaGenerica<PlanCuentaDto>> ObtenerCuentaPorId(string id, string token)
        {

            try
            {
                ApiResponse<PlanCuentaDto> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}?ccb_id={id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<PlanCuentaDto>>(stringData)
                        ?? throw new Exception("La deserialización devolvió un valor nulo."); 

                    return new RespuestaGenerica<PlanCuentaDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException))==true)
                    {
                        return new RespuestaGenerica<PlanCuentaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException))==true)
                    {
                        return new RespuestaGenerica<PlanCuentaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if(error != null )
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        return new RespuestaGenerica<PlanCuentaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener la cuenta del plan de cuenta." };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<PlanCuentaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener la cuenta del plan de cuenta." };
            }
        }

        public async Task<(List<PlanCuentaDto>, MetaData?)> ObtenerPlanCuentas(QueryFilters filtro, string token)
        {

            try
            {
                ApiResponse<List<PlanCuentaDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filtro, token, out StringContent contentData);
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PlanCuentaDto>>>(stringData);
                    if (apiResponse == null)
                    {
                        throw new NegocioException("La deserialización devolvió un valor nulo.");
                    }
                    return (apiResponse.Data,null);//mandamos en metadata null ya que no se especifico el filtro y el paginado. 
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error !=null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if(error.Detail!=null)
                        {
                            throw new NegocioException(error.Detail);
                        }
                        else
                        {
                            throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                        }
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error.Detail != null)
                        {
                            throw new NegocioException(error.Detail);
                        }
                        else
                        {
                            throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                        }
                    }
                    else if(error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
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

       
      
    }
}
