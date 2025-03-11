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
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ConsultasServicio : Servicio<ConsultasDto>, IConsultasServicio
    {
        private const string RutaAPI = "/api/ConsultaCC";
        private const string CONS_CTACTE = "/ConsultarCuentaCorriente";
        private const string CONS_VTO = "/ConsultaVencimientoComprobantesNoImputados";
        private const string CONS_CMTE_TOT = "/ConsultaComprobantesMeses";
        private const string CONS_CMTE_DET = "/ConsultaComprobantesMesDetalle";
        private const string CONS_OP_PROV = "/ConsultaOrdenesDePagoProveedor";
        private const string CONS_OP_PROV_DET = "/ConsultaOrdenesDePagoProveedorDetalle";
        private const string CONS_REC_PROV = "/ConsultaRecepcionProveedor";
        private const string CONS_REC_PROV_DET = "/ConsultaRecepcionProveedorDetalle";




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

        

        public async Task<(List<ConsCtaCteDto>,MetadataGrid)> ConsultarCuentaCorriente(string ctaId, long fechaD, string userId,int pagina, int registros, string token)
        {
            try
            {
                ApiResponse<List<ConsCtaCteDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}?ctaId={ctaId}&fechaD={fechaD}&userId={userId}&pagina={pagina}&registros={registros}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCtaCteDto>>>(stringData);

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

                throw new Exception($"Algo no fue bien al intentar obtener los registros de la Cuenta Corriente de la cuenta {ctaId}");
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

        public async Task<RespuestaGenerica<ConsOrdPagosDto>> ConsultaOrdenesDePagoProveedor(string ctaId, DateTime fd, DateTime fh, string tipoOP, string userId, string token)
        {
            try
            {
                ApiResponse<List<ConsOrdPagosDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var fechaD = fd.Ticks;
                var fechaH = fh.Ticks;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_OP_PROV}?ctaId={ctaId}&fecD={fechaD}&fecH={fechaH}&tipoOP={tipoOP}&userId={userId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsOrdPagosDto>>>(stringData);

                    return new RespuestaGenerica<ConsOrdPagosDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsOrdPagosDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsOrdPagosDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsOrdPagosDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las ordenes de pago del proveedor." };
            }
        }

        public async Task<RespuestaGenerica<ConsOrdPagosDetDto>> ConsultaOrdenesDePagoProveedorDetalle(string cmptId, string token)
        {
            try
            {
                ApiResponse<List<ConsOrdPagosDetDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_OP_PROV_DET}?cmptId={cmptId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsOrdPagosDetDto>>>(stringData);

                    return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el detalle de las ordenes de pago del proveedor." };
            }
        }
        public async Task<RespuestaGenerica<ConsRecepcionProveedorDto>> ConsultaRecepcionProveedor(string ctaId, DateTime fd, DateTime fh, string admId, string token)
        {
            try
            {
                ApiResponse<List<ConsRecepcionProveedorDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var fechaD = fd.Ticks;
                var fechaH = fh.Ticks;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_REC_PROV}?ctaId={ctaId}&fecD={fechaD}&fecH={fechaH}&admId={admId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsRecepcionProveedorDto>>>(stringData);

                    return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las recepciones del proveedor." };
            }
        }

        public async Task<RespuestaGenerica<ConsRecepcionProveedorDetalleDto>> ConsultaRecepcionProveedorDetalle(string cmptId, string token)
        {
            try
            {
                ApiResponse<List<ConsRecepcionProveedorDetalleDto>> apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_REC_PROV_DET}?cmptId={cmptId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsRecepcionProveedorDetalleDto>>>(stringData);

                    return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el detalle de las recepciones del proveedor." };
            }
        }
    }
}
