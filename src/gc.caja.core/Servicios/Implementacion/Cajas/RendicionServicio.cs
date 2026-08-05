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
    public class RendicionServicio : Servicio<Dto>, IRendicionServicio
    {
        private const string RutaAPI = "/api/ApiRendicion";
        private const string POST_CARGAR_RENDICIONES = "/CargarRendiciones";
        private const string POST_CARGAR_NOMINACIONES = "/CargarNominaciones";
        private const string POST_CONFIRMAR_RENDICION = "/ConfirmarRendicion";

        public RendicionServicio(IOptions<AppSettings> options, ILogger<RendicionServicio> logger)
            : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<RendicionResponseDto>> CargarRendiciones(RendicionRequestDto request, string token)
        {
            return await PostListaAsync<RendicionRequestDto, RendicionResponseDto>(
                request,
                token,
                POST_CARGAR_RENDICIONES,
                "No fue posible obtener los instrumentos de rendicion.");
        }

        public async Task<RespuestaGenerica<RendicionNominalResponseDto>> CargarNominaciones(RendicionNominalRequestDto request, string token)
        {
            return await PostListaAsync<RendicionNominalRequestDto, RendicionNominalResponseDto>(
                request,
                token,
                POST_CARGAR_NOMINACIONES,
                "No fue posible obtener las nominaciones del instrumento.");
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarRendicion(RendicionCargaRequestDto request, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_CONFIRMAR_RENDICION}";

                _logger.LogInformation(
                    "Rendiciones: caja.core invoca API confirmar. Url={Url}; Caja={Caja}; Proceso={Proceso}; Cierre={Cierre}; Adm={Adm}; Usuario={Usuario}; JsonRendiciones={JsonRendiciones}",
                    link,
                    request.caja_id,
                    request.caja_nro_proceso,
                    request.caja_nro_cierre,
                    request.adm_id,
                    request.usu_id,
                    request.json_rendiciones);

                using var response = await client.PostAsync(link, contentData);
                _logger.LogInformation(
                    "Rendiciones: caja.core recibio response HTTP confirmar. StatusCode={StatusCode}; Reason={Reason}",
                    (int)response.StatusCode,
                    response.ReasonPhrase);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);
                    return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = mensaje };
                }

                var stringData = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Rendiciones: caja.core body response confirmar. Body={Body}", stringData);
                if (string.IsNullOrWhiteSpace(stringData))
                {
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = "No se recibio una respuesta valida al confirmar la rendicion."
                    };
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                if (apiResponse?.Data == null)
                {
                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar la confirmacion de la rendicion."
                    };
                }

                var respuesta = apiResponse.Data;
                _logger.LogInformation(
                    "Rendiciones: caja.core response deserializada. Resultado={Resultado}; ResultadoId={ResultadoId}; Mensaje={Mensaje}; SetFocus={SetFocus}",
                    respuesta.resultado,
                    respuesta.resultado_id,
                    respuesta.resultado_msj,
                    respuesta.resultado_setfocus);

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
                    "{Servicio}-{Metodo}: error confirmando rendicion parcial.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name);

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Ocurrio un error al confirmar la rendicion parcial."
                };
            }
        }

        private async Task<RespuestaGenerica<TResponse>> PostListaAsync<TRequest, TResponse>(
            TRequest request,
            string token,
            string action,
            string mensajeError)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(request, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{action}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    var mensaje = await ReadApiErrorAsync(response);
                    return new RespuestaGenerica<TResponse> { Ok = false, Mensaje = mensaje };
                }

                var stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(stringData))
                {
                    return new RespuestaGenerica<TResponse>
                    {
                        Ok = false,
                        Mensaje = "No se recibio una respuesta valida de la API."
                    };
                }

                var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<TResponse>>>(stringData);
                if (apiResponse?.Data == null)
                {
                    return new RespuestaGenerica<TResponse>
                    {
                        Ok = false,
                        Mensaje = "No fue posible interpretar la respuesta de la API."
                    };
                }

                return new RespuestaGenerica<TResponse>
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
                    "{Servicio}-{Metodo}: error invocando {Action}.",
                    GetType().Name,
                    MethodBase.GetCurrentMethod()?.Name,
                    action);

                return new RespuestaGenerica<TResponse>
                {
                    Ok = false,
                    Mensaje = mensajeError
                };
            }
        }
    }
}
