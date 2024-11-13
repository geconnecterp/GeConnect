using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion
{
    public  class Producto2Servicio: Servicio<ProductoDto>, IProducto2Servicio
    {
        private const string RutaAPI = "/api/apiproducto";

        //BOX
        private const string BOX_INFO = "/ObtenerBoxInfo";
        private const string BOX_INFO_STK = "/ObtenerBoxInfoStk";
        private const string BOX_INFO_MOV_STK = "/ObtenerBoxInfoMovStk";

        private readonly AppSettings _appSettings;

        public Producto2Servicio(IOptions<AppSettings> options, ILogger<ProductoServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public async Task<RespuestaGenerica<BoxInfoDto>> ObtenerBoxInfo(string boxId, string token)
        {
            try
            {
                ApiResponse<BoxInfoDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BOX_INFO}?box_id={boxId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<BoxInfoDto>>(stringData);

                    return new RespuestaGenerica<BoxInfoDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                return new RespuestaGenerica<BoxInfoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar " };
            }
        }

        public async Task<RespuestaGenerica<BoxInfoStkDto>> ObtenerBoxInfoStk(string box_id, string token)
        {
            try
            {
                ApiResponse<List<BoxInfoStkDto>>? apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BOX_INFO_STK}?box_id={box_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxInfoStkDto>>>(stringData);

                    return new RespuestaGenerica<BoxInfoStkDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                return new RespuestaGenerica<BoxInfoStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el Stk del BOX" };
            }
        }

        public async Task<RespuestaGenerica<BoxInfoMovStkDto>> ObtenerBoxInfoMovStk(string box_id, string sm_tipo, DateTime desde, DateTime hasta, string token)
        {
            try
            {
                ApiResponse<List<BoxInfoMovStkDto>>? apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;
                var d = desde.Ticks;
                var h = hasta.Ticks;
                var link = $"{_appSettings.RutaBase}{RutaAPI}{BOX_INFO_MOV_STK}?box_id={box_id}&sm_tipo={sm_tipo}&desde={d}&hasta={h}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxInfoMovStkDto>>>(stringData);

                    return new RespuestaGenerica<BoxInfoMovStkDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                { 
                    return new() { Ok = false, Mensaje = $"No se encontraron movimientos para el box {box_id}" };
                }
                else                
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    return new() { Ok = false, Mensaje = "Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema." };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                return new RespuestaGenerica<BoxInfoMovStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los movimientos del BOX" };
            }
        }
    }
}
