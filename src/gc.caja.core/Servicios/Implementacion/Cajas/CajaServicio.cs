using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Seguridad
{
    public class CajaServicio : Servicio<Dto>, ICajaServicio
    {
        private const string RutaAPI = "/api/apicaja";

        private const string POST_VALIDA_INTEGRIDAD = "/ValidaIntegridadUsuarioCaja";
        private const string POST_APERTURA_CAJA = "/AperturaCaja";


        public CajaServicio(IOptions<AppSettings> options, ILogger<CajaServicio> logger):base(options,logger)
        {
        }

        public async Task<RespuestaGenerica<RespuestaDto>> AperturaCaja(CajaValidaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_APERTURA_CAJA}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }
                    var resp = apiResponse.Data;
                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                    else if (resp.resultado > 0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            EsWarn = true,
                            EsError = false,
                            Mensaje = resp.resultado_msj,
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            EsWarn = false,
                            EsError = true,
                            Mensaje = resp.resultado_msj,
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Hubo un error al intentar realizar la apertura de caja." };
            }
        }

        public async Task<CajaSettings> ObtenerAsync(string ruta)
        {
            CajaSettings c = new();

            if (string.IsNullOrWhiteSpace(ruta))
            {
                throw new InvalidOperationException("No se recepcionó la ruta de acceso a la configuración de CAJA.");
            }

            if (!File.Exists(ruta))
            {
                throw new FileNotFoundException("No se encontró el archivo de configuración de caja .");
            }

            string json = await File.ReadAllTextAsync(ruta);

            var cajaSettings = JsonConvert.DeserializeObject<CajaSettings>(json);

            if (cajaSettings is null)
            {
                throw new InvalidOperationException("El contenido del JSON no pudo convertirse a CajaSettings.");
            }

            return cajaSettings;
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ValidarIntegridadUsuarioCaja(CajaValidaReqDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_VALIDA_INTEGRIDAD}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }
                    var resp = apiResponse.Data;
                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                    else if (resp.resultado > 0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            EsWarn = true,
                            EsError = false,
                            Mensaje = resp.resultado_msj,
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            EsWarn = false,
                            EsError = true,
                            Mensaje = resp.resultado_msj,
                            Entidad = apiResponse.Data
                            // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                        };
                    }
                }
                else
                {
                    var msg = await ReadApiErrorAsync(response);
                    _logger.LogWarning($"Error API ({response.StatusCode}): {msg}");
                    return new() { Ok = false, Mensaje = msg };
                }
            }
            catch (Exception ex)
            {              
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Hubo un error al intentar validar la integridad del usuario en la caja" };
            }
        }
    }
}
