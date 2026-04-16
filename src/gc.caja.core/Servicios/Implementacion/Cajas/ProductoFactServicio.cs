using gc.caja.core.Servicios.Contratos.Cajas;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.Cajas.Request;
using gc.infraestructura.Dtos.Gen;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.caja.core.Servicios.Implementacion.Cajas
{
    public class ProductoFactServicio: Servicio<Dto>, IProductoFactServicio
    {
        private const string RutaAPI = "/api/apiproductofact";

        private const string POST_OBTENER_PRODUCTO_DATOS = "/ObtenerProductoDatos";


        public ProductoFactServicio(IOptions<AppSettings> options, ILogger<ProductoFactServicio> logger) : base(options, logger)
        {
        }

        public async Task<RespuestaGenerica<ProductoDatosResponseDto>> ObtenerProductoDatos(ProductoDatosRequestDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{POST_OBTENER_PRODUCTO_DATOS}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoDatosResponseDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    var resp = apiResponse.Data;
                    if (!resp.Any())
                    {
                        return new() { Ok = false, Mensaje = "No se encontraron productos según el criterio." };
                    }
                    else
                    {
                        return new RespuestaGenerica<ProductoDatosResponseDto>
                        {
                            Ok = true,
                            Mensaje = "OK",
                            ListaEntidad = apiResponse.Data
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
                return new() { Ok = false, Mensaje = "Error al obtener los datos del producto" };
            }
        }

    }
}
