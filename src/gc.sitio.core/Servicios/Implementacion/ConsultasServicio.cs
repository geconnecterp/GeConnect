using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ConsultasServicio : Servicio<ConsultasDto>, IConsultasServicio
    {
        private const string RutaAPI = "/api/ConsultaCC";
        private const string CONS_CTACTE = "/ConsultarCuentaCorriente";
        private const string CONS_VTO = "/ConsultarCuentaCorriente";
        private const string CONS_CMTE_TOT = "/ConsultaComprobantesMeses";
        private const string CONS_CMTE_DET = "/ConsultaComprobantesMesDetalle";



        private readonly AppSettings _appSettings;
        public ConsultasServicio(IOptions<AppSettings> options, ILogger<ConsultasServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }
        public async Task<RespuestaGenerica<ConsCompDetDto>> ConsultaComprobantesMesDetalle(string ctaId, string mes, bool relCuit, string userId, string token)
        {
            try
            {
                ApiResponse<List<ConsCompDetDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_CMTE_DET}?ctaId={ctaId}&mes={mes}&relCuit={relCuit}&userId={userId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCompDetDto>>>(stringData);

                    return new RespuestaGenerica<ConsCompDetDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsCompDetDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsCompDetDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsCompDetDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el detalle de comprobantes." };
            }
        }
        

        public async Task<RespuestaGenerica<ConsCompTotDto>> ConsultaComprobantesMeses(string ctaId, int meses, bool relCuit, string userId, string token)
        {
            try
            {
                ApiResponse<List<ConsCompTotDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_CMTE_TOT}?ctaId={ctaId}&meses={meses}&relCuit={relCuit}&userId={userId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCompTotDto>>>(stringData);

                    return new RespuestaGenerica<ConsCompTotDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los comprobantes de la cuenta." };
            }
        }

        public async Task<RespuestaGenerica<ConsCtaCteDto>> ConsultarCuentaCorriente(string ctaId, long fechaD, string userId, string token)
        {
            try
            {
                ApiResponse<List<ConsCtaCteDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}?ctaId={ctaId}&fechaD={fechaD}&userId={userId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCtaCteDto>>>(stringData);

                    return new RespuestaGenerica<ConsCtaCteDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsCtaCteDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsCtaCteDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsCtaCteDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener la cuenta corriente de la cuenta." };
            }
        }

        public async Task<RespuestaGenerica<ConsVtoDto>> ConsultaVencimientoComprobantesNoImputados(string ctaId, long fechaD, long fechaH, string userId, string token)
        {
            try
            {
                ApiResponse<List<ConsVtoDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_VTO}?ctaId={ctaId}&fechaD={fechaD}&fechaH={fechaH}&userId={userId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsVtoDto>>>(stringData);

                    return new RespuestaGenerica<ConsVtoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsVtoDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsVtoDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsVtoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los comprobantes vencidos de la cuenta." };
            }
        }
    }
}
