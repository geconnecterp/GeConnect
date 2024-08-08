using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class ProductoServicio : Servicio<ProductoDto>, IProductoServicio
    {
        private const string RutaAPI = "/api/producto";
        private const string BUSCAR_PROD = "/ProductoBuscar";
        private const string BUSCAR_LISTA = "/ProductoListaBuscar";
        private const string INFOPROD_STKD = "/InfoProductoStkD";
        private const string INFOPROD_StkBoxes = "/InfoProductoStkA";
        private const string INFOPROD_STKA = "/InfoProductoStkA";
        private const string INFOPROD_MovStk = "/InfoProductoMovStk";
        private const string INFOPROD_LP = "/InfoProductoLP";

        private readonly AppSettings _appSettings;

        public ProductoServicio(IOptions<AppSettings> options, ILogger<ProductoServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public async Task<ProductoBusquedaDto> BusquedaBaseProductos(BusquedaBase busqueda, string token)
        {
            ApiResponse<ProductoBusquedaDto> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            string parametros = EvaluarEntidad4Link(busqueda);
            var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PROD}?{parametros}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
                    return new ProductoBusquedaDto();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBusquedaDto>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new ProductoBusquedaDto();

            }
        }

        public async Task<List<ProductoListaDto>> BusquedaListaProductos(BusquedaProducto busqueda, string token)
        {
            ApiResponse<List<ProductoListaDto>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            string parametros = EvaluarEntidad4Link(busqueda);
            var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_LISTA}?{parametros}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametro de busqueda {parametros}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoListaDto>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkD>> InfoProductoStkD(string id, string admId,string token)
        {
            ApiResponse<List<InfoProdStkD>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;
            
            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_STKD}?id={id}&admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkD>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkBox>> InfoProductoStkBoxes(string id, string adm, string depo, string token)
        {
            ApiResponse<List<InfoProdStkBox>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_StkBoxes}?id={id}&adm={adm}&depo={depo}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkBox>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdStkA>> InfoProductoStkA(string id, string admId, string token)
        {
            ApiResponse<List<InfoProdStkA>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_StkBoxes}?id={id}&admId={admId}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-admId:{admId}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdStkA>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdMovStk>> InfoProductoMovStk(string id, string adm, string depo, string tmov, DateTime desde, DateTime hasta, string token)
        {
            ApiResponse<List<InfoProdMovStk>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_MovStk}?id={id}&adm={adm}&depo={depo}&tmov={tmov}&desde={desde}&hasta={hasta}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}-adm:{adm}-depo:{depo}-tmov:{tmov}-desde:{desde}-hasta:{hasta}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdMovStk>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }

        public async Task<List<InfoProdLP>> InfoProductoLP(string id, string token)
        {
            ApiResponse<List<InfoProdLP>> apiResponse;

            HelperAPI helper = new HelperAPI();

            HttpClient client = helper.InicializaCliente(token);
            HttpResponseMessage response;

            var link = $"{_appSettings.RutaBase}{RutaAPI}{INFOPROD_StkBoxes}?id={id}";

            response = await client.GetAsync(link);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                string stringData = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(stringData))
                {
                    _logger.LogWarning($"La API no devolvió dato alguno. Parametros de busqueda id:{id}");
                    return new();
                }
                apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<InfoProdLP>>>(stringData);
                return apiResponse.Data;
            }
            else
            {
                string stringData = await response.Content.ReadAsStringAsync();
                _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                return new();
            }
        }
    }
}
