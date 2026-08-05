using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Cajas.Response;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class AnulacionCobranzaServicio : Servicio<Dto>, IAnulacionCobranzaServicio
    {
        private const string RutaAPI = "/api/ApiAnulacion";
        private const string POST_BUSCAR_COBRANZAS = "/BuscarCobranzas";
        private const string POST_ANULAR_COBRANZA = "/AnularCobranza";

        public AnulacionCobranzaServicio(IOptions<AppSettings> options, ILogger<AnulacionCobranzaServicio> logger)
            : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<AnulacionCobranzaResponseDto>> BuscarCobranzas(AnulacionCobranzaBuscarRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_BUSCAR_COBRANZAS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);
                    return new RespuestaGenerica<AnulacionCobranzaResponseDto> { Ok = false, Mensaje = mensaje };
                }

                var stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(stringData))
                {
                    return new RespuestaGenerica<AnulacionCobranzaResponseDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibio una respuesta valida de la API."
                    };
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<AnulacionCobranzaResponseDto>>>(stringData);
                if (apiResponse?.Data == null)
                {
                    return new RespuestaGenerica<AnulacionCobranzaResponseDto>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar la respuesta de cobranzas."
                    };
                }

                return new RespuestaGenerica<AnulacionCobranzaResponseDto>
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
                    "{Servicio}-{Metodo}: error buscando cobranzas para anulacion.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name);

                return new RespuestaGenerica<AnulacionCobranzaResponseDto>
                {
                    Ok = false,
                    Mensaje = "No fue posible obtener las cobranzas del cliente."
                };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> AnularCobranza(AnulacionCobranzaConfirmarRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_ANULAR_COBRANZA}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);
                    return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = mensaje };
                }

                var stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(stringData))
                {
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibio una respuesta valida al anular la cobranza."
                    };
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                if (apiResponse?.Data == null)
                {
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar la confirmacion de anulacion."
                    };
                }

                var respuesta = apiResponse.Data;
                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = respuesta.resultado == 0,
                    EsWarn = respuesta.resultado > 0,
                    EsError = respuesta.resultado < 0,
                    Mensaje = respuesta.resultado == 0 ? "OK" : respuesta.resultado_msj,
                    Entidad = respuesta
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "{Servicio}-{Metodo}: error confirmando anulacion de cobranza.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name);

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Ocurrio un error al anular la cobranza."
                };
            }
        }
    }
}
