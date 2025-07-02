using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Consultas;
using gc.infraestructura.Dtos.CuentaComercial;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

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
		private const string CONS_CERT_RETEN_IB = "/ConsultaCertRetenIB";
		private const string CONS_CERT_RETEN_IVA = "/ConsultaCertRetenIVA";
		private const string CONS_CERT_RETEN_GAN = "/ConsultaCertRetenGAN";


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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCompDetDto>>>(stringData)
                        ?? throw new NegocioException("No se pudo deserializar los datos de la Consulta Comprobantes del Mes.");
                    return new RespuestaGenerica<ConsCompDetDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<ConsCompDetDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<ConsCompDetDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCompTotDto>>>(stringData)
                        ?? throw new NegocioException("Hubo un problema al deserializar datos de Consulta de Comprobantes.");

                    return new RespuestaGenerica<ConsCompTotDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsCompTotDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los comprobantes de la cuenta." };
            }
        }



        public async Task<(List<ConsCtaCteDto>, MetadataGrid)> ConsultarCuentaCorriente(string ctaId, long fechaD, string userId, int pagina, int registros, string token)
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsCtaCteDto>>>(stringData)
                        ?? throw new NegocioException("Hubo un problema al deserializar los datos de la cuenta corriente.");

                    return (apiResponse.Data ?? [], apiResponse.Meta ?? new MetadataGrid());

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsVtoDto>>>(stringData)
                        ?? throw new NegocioException("Hubo un error al deserializar los datos del Vencimiento de comprobantes.");

                    return new RespuestaGenerica<ConsVtoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsOrdPagosDto>>>(stringData)
                        ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ConsOrdPagosDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsOrdPagosDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las recepciones del proveedor. La Solicitud ha excedido el tiempo de espera." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsOrdPagosDetDto>>>(stringData)
                        ?? throw new NegocioException("Hubo un problema al deserializar los datos"); 

                    return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsOrdPagosDetDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las recepciones del proveedor. La Solicitud ha excedido el tiempo de espera." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                #region Configuración especifica para del timeout en la llamada
                client.Timeout = TimeSpan.FromMinutes(3);
                #endregion

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsRecepcionProveedorDto>>>(stringData) 
                        ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsRecepcionProveedorDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las recepciones del proveedor. La Solicitud ha excedido el tiempo de espera." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                client.Timeout = TimeSpan.FromMinutes(3);

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsRecepcionProveedorDetalleDto>>>(stringData)
                                                ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error != null && error.TypeException?.Equals(nameof(NegocioException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null && error.TypeException?.Equals(nameof(NotFoundException)) == true)
                    {
                        if (error == null)
                        {
                            throw new NegocioException("Error desconocido al procesar la respuesta de la API.");
                        }
                        throw new NegocioException(error.Detail ?? "Error desconocido al procesar la respuesta de la API.");
                    }
                    else if (error != null)
                    {
                        throw new Exception(error.Detail);
                    }
                    else
                    {
                        throw new Exception("Error desconocido al procesar la respuesta de la API.");
                    }
                }
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las recepciones del proveedor. La Solicitud ha excedido el tiempo de espera." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsRecepcionProveedorDetalleDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el detalle de las recepciones del proveedor." };
            }
        }

		public List<CertRetenGananDto> ConsultaCertRetenGA(string op_compte, string token)
		{
			ApiResponse<List<CertRetenGananDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_CERT_RETEN_GAN}?opCompte={op_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CertRetenGananDto>>>(stringData)
							?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta directa. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta directa: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta directa");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta directa.");
				throw;
			}
		}

		public List<CertRetenIBDto> ConsultaCertRetenIB(string op_compte, string token)
		{
			ApiResponse<List<CertRetenIBDto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_CERT_RETEN_IB}?opCompte={op_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CertRetenIBDto>>>(stringData)
							?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta directa. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta directa: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta directa");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta directa.");
				throw;
			}
		}

		public List<CertRetenIVADto> ConsultaCertRetenIVA(string op_compte, string token)
		{
			ApiResponse<List<CertRetenIVADto>> respuesta;
			string stringData;
			try
			{
				HelperAPI helper = new();
				HttpClient client = helper.InicializaCliente(token);
				HttpResponseMessage response;
				var link = $"{_appSettings.RutaBase}{RutaAPI}{CONS_CERT_RETEN_IVA}?opCompte={op_compte}";
				response = client.GetAsync(link).GetAwaiter().GetResult();
				if (response.StatusCode == HttpStatusCode.OK)
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					if (!string.IsNullOrEmpty(stringData))
					{
						respuesta = JsonConvert.DeserializeObject<ApiResponse<List<CertRetenIVADto>>>(stringData)
							?? throw new NegocioException("Hubo un problema al deserializar los datos");
					}
					else
					{
						throw new Exception("No se logro obtener la respuesta de la API con los datos de la cuenta directa. Verifique.");
					}
					return respuesta.Data;
				}
				else
				{
					stringData = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
					_logger.LogError($"Error al intentar obtener los datos de la cuenta directa: {stringData}");
					throw new NegocioException("Hubo un error al intentar obtener los datos de la cuenta directa");
				}

			}
			catch (NegocioException)
			{
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error al intentar obtener los datos de la cuenta directa.");
				throw;
			}
		}
	}
}
