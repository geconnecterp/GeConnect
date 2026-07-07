using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using log4net.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class NotaCreditoServicio : Servicio<Dto>, INotaCreditoServicio
    {
        private const string RutaAPI = "/api/ApiNotaCredito";

        private const string POST_VALIDAR_NC = "/ValidarNC";
        private const string POST_BUSCAR_PRODUCTO = "/BuscarProducto";
        
        private const string RutaAPI_TC = "/api/tipocomprobante";
        
        private const string GET_TIPO_COMPROBANTE = "/GetTipoComprobanteListaPorTipoAfipOptId";
        public NotaCreditoServicio(IOptions<AppSettings> options, ILogger<NotaCreditoServicio> logger) : base(options, logger)
        {

        }

        /// <summary>
        /// SPGECO_CAJA_NC_Valida puede devolver ninguna, una o varias filas.
        /// Las filas múltiples representan una repetición numérica del comprobante.
        /// </summary>
        public async Task<RespuestaGenerica<NCValidaResponseDto>> ValidarNC(
    NCValidaRequestDto req,
    string token)
        {
            try
            {
                var helper = new HelperAPI();

                var client = helper.InicializaCliente(
                    req,
                    token,
                    out StringContent contentData
                );

                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_VALIDAR_NC}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);

                    _logger.LogWarning(
                        "{Clase}-{Metodo} - Error API. StatusCode={StatusCode}. Mensaje={Mensaje}",
                        GetType().Name,
                        MethodBase.GetCurrentMethod()?.Name,
                        response.StatusCode,
                        mensaje
                    );

                    return new RespuestaGenerica<NCValidaResponseDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }

                var stringData = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    _logger.LogWarning(
                        "{Clase}-{Metodo} - La API respondió sin contenido.",
                        GetType().Name,
                        MethodBase.GetCurrentMethod()?.Name
                    );

                    return new RespuestaGenerica<NCValidaResponseDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibió una respuesta válida de la API."
                    };
                }

                var apiResponse = JsonConvert.DeserializeObject<
                    ApiResponse<List<NCValidaResponseDto>>
                >(stringData);

                if (apiResponse == null || apiResponse.Data == null)
                {
                    _logger.LogWarning(
                        "{Clase}-{Metodo} - No fue posible interpretar la respuesta de validación.",
                        GetType().Name,
                        MethodBase.GetCurrentMethod()?.Name
                    );

                    return new RespuestaGenerica<NCValidaResponseDto>
                    {
                        Ok = false,
                        Mensaje = "La respuesta de validación no contiene datos válidos."
                    };
                }

                return new RespuestaGenerica<NCValidaResponseDto>
                {
                    Ok = true,
                    Mensaje = "OK",
                    ListaEntidad = apiResponse.Data
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Clase}-{Metodo} - Error al validar el comprobante original de NC.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name
                );

                return new RespuestaGenerica<NCValidaResponseDto>
                {
                    Ok = false,
                    Mensaje =
                        "Ocurrió un error al validar el comprobante para la Nota de Crédito."
                };
            }
        }
     

        public async Task<RespuestaGenerica<TipoComprobanteDto>> GetTipoComprobante(
            string afipId,
            string optId,
            string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(token);

                // '%' debe viajar codificado como '%25' para que la URL sea válida.
                var url =
                    $"{_appSettings.RutaBase}{RutaAPI_TC}{GET_TIPO_COMPROBANTE}" +
                    $"?afip_id={Uri.EscapeDataString(afipId)}" +
                    $"&opt_id={Uri.EscapeDataString(optId)}";

                using var response = await client.GetAsync(url);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);

                    _logger.LogWarning(
                        "GetTipoComprobante devolvió HTTP {StatusCode}: {Mensaje}",
                        response.StatusCode,
                        mensaje
                    );

                    return new RespuestaGenerica<TipoComprobanteDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }

                var body = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(body))
                {
                    return new RespuestaGenerica<TipoComprobanteDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibió una respuesta válida de la API."
                    };
                }

                var apiResponse =
                    JsonConvert.DeserializeObject<ApiResponse<List<TipoComprobanteDto>>>(body);

                if (apiResponse?.Data is null)
                {
                    return new RespuestaGenerica<TipoComprobanteDto>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar los tipos de comprobante."
                    };
                }

                return new RespuestaGenerica<TipoComprobanteDto>
                {
                    Ok = true,
                    ListaEntidad = apiResponse.Data,
                    Mensaje = "OK"
                };
            }
            catch (NegocioException ex)
            {
                _logger.LogWarning(
                    ex,
                    "{Servicio}-{Metodo}: error de negocio obteniendo tipos de comprobante.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name
                );

                return new RespuestaGenerica<TipoComprobanteDto>
                {
                    Ok = false,
                    Mensaje = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Servicio}-{Metodo}: error obteniendo tipos de comprobante.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name
                );

                return new RespuestaGenerica<TipoComprobanteDto>
                {
                    Ok = false,
                    Mensaje = "No fue posible obtener los tipos de comprobante."
                };
            }
        }

        public async Task<RespuestaGenerica<NCProductoBuscarResponseDto>> BuscarProducto(
     NCProductoBuscarRequestDto request, string token)
        {
            var correlationId = Guid.NewGuid().ToString("N");

            if (request == null)
            {
                _logger.LogWarning(
                    "NC Devolución - BuscarProducto recibió una solicitud nula. CorrelationId={CorrelationId}",
                    correlationId
                );

                return new RespuestaGenerica<NCProductoBuscarResponseDto>
                {
                    Ok = false,
                    Mensaje = "No se recibieron datos para buscar el producto."
                };
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning(
                    "NC Devolución - BuscarProducto sin token válido. CorrelationId={CorrelationId}",
                    correlationId
                );

                return new RespuestaGenerica<NCProductoBuscarResponseDto>
                {
                    Ok = false,
                    Mensaje = "La sesión actual no posee un token válido."
                };
            }

            try
            {
                _logger.LogInformation(
                    "NC Devolución - BuscarProducto iniciada. " +
                    "Tipo={Tipo}, Comprobante={Comprobante}, Repetido={Repetido}, " +
                    "Adm={Administracion}, Valor={Valor}, Cantidad={Cantidad}, JsonProductosLength={JsonProductosLength}, " +
                    "CorrelationId={CorrelationId}",
                    request.tco_id,
                    request.cm_compte,
                    request.cm_repetido,
                    request.adm_id,
                    request.valor,
                    request.cantidad,
                    request.json_p?.Length ?? 0,
                    correlationId
                );

                var helper = new HelperAPI();

                var client = helper.InicializaCliente(
                    request,
                    token,
                    out StringContent contentData
                );

                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_BUSCAR_PRODUCTO}";

                _logger.LogInformation(
                    "NC Devolución - Enviando BuscarProducto a {Url}. CorrelationId={CorrelationId}",
                    link,
                    correlationId
                );

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);

                    _logger.LogWarning(
                        "NC Devolución - BuscarProducto respondió con error HTTP. " +
                        "StatusCode={StatusCode}, Mensaje={Mensaje}, CorrelationId={CorrelationId}",
                        response.StatusCode,
                        mensaje,
                        correlationId
                    );

                    return new RespuestaGenerica<NCProductoBuscarResponseDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }

                var stringData = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(stringData))
                {
                    _logger.LogWarning(
                        "NC Devolución - BuscarProducto respondió sin contenido. CorrelationId={CorrelationId}",
                        correlationId
                    );

                    return new RespuestaGenerica<NCProductoBuscarResponseDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibió una respuesta válida al buscar el producto."
                    };
                }

                ApiResponse<List<NCProductoBuscarResponseDto>>? apiResponse;

                try
                {
                    apiResponse = JsonConvert.DeserializeObject<
                        ApiResponse<List<NCProductoBuscarResponseDto>>
                    >(stringData);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "NC Devolución - Error deserializando respuesta de BuscarProducto. CorrelationId={CorrelationId}",
                        correlationId
                    );

                    return new RespuestaGenerica<NCProductoBuscarResponseDto>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar la respuesta de búsqueda de producto."
                    };
                }

                if (apiResponse?.Data == null)
                {
                    _logger.LogWarning(
                        "NC Devolución - BuscarProducto no devolvió una lista de productos. CorrelationId={CorrelationId}",
                        correlationId
                    );

                    return new RespuestaGenerica<NCProductoBuscarResponseDto>
                    {
                        Ok = false,
                        Mensaje = "La respuesta de búsqueda de producto no contiene datos válidos."
                    };
                }

                var productos = apiResponse.Data;

                _logger.LogInformation(
                    "NC Devolución - BuscarProducto finalizada. " +
                    "CantidadRegistros={CantidadRegistros}, Respuestas={Respuestas}, CorrelationId={CorrelationId}",
                    productos.Count,
                    string.Join(
                        ", ",
                        productos
                            .Select(x => x.respuesta?.ToString() ?? "null")
                            .Distinct()
                    ),
                    correlationId
                );

                return new RespuestaGenerica<NCProductoBuscarResponseDto>
                {
                    Ok = true,
                    Mensaje = "OK",
                    ListaEntidad = productos
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "NC Devolución - Error inesperado en BuscarProducto. CorrelationId={CorrelationId}",
                    correlationId
                );

                return new RespuestaGenerica<NCProductoBuscarResponseDto>
                {
                    Ok = false,
                    Mensaje = "Ocurrió un error al buscar el producto para la devolución."
                };
            }
        }     
    }
}
