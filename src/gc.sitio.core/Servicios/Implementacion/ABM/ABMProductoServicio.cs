using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.sitio.core.Servicios.Contratos.ABM;
using log4net.Filter;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

namespace gc.sitio.core.Servicios.Implementacion.ABM
{
    public class ABMProductoServicio : Servicio<ProductoListaDto>, IABMProductoServicio
    {
        private const string RutaAPI = "/api/abmproducto";
        private const string BUSCAR_PRODUCTOS = "/BuscaProductos";
        private const string BUSCAR_PRODUCTO = "/BuscarProducto";

        private readonly AppSettings _appSettings;
        public ABMProductoServicio(IOptions<AppSettings> options, ILogger<ABMProductoServicio> logger) : base(options, logger, RutaAPI)
        {
            _appSettings = options.Value;
        }

        public async Task<(List<ProductoListaDto>, MetadataGrid)> BuscarProducto(QueryFilters filters, string token)
        {
            try
            {
                ApiResponse<List<ProductoListaDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filters, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PRODUCTOS}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoListaDto>>>(stringData);

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

                throw new Exception("Algo no fue bien al intentar obtener los productos solicitados según el filtro actual.");
            }
        }

        public async Task<ProductoDto> BuscarProducto(string p_id, string token)
        {
            try
            {
                ApiResponse<ProductoDto>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{BUSCAR_PRODUCTO}?p_id={p_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoDto>>(stringData);

                    return apiResponse.Data;
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

                    //throw new NegocioException("Algo no fue bien y el proceso no se completó. Intente de nuevo más tarde. Si el problema persiste informe al Administrador del sistema.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar obtener los datos del producto buscado.");
            }
        }
    }
}
