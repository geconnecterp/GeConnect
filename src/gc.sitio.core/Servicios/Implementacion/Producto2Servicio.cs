using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Etiqueta;
using gc.infraestructura.Dtos.Productos.Impositivo;
using gc.infraestructura.Dtos.Productos.Precio;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Ocsp;
using System.Drawing.Printing;
using System.Net;
using System.Reflection;
using System.Security.Policy;

namespace gc.sitio.core.Servicios.Implementacion
{
    public class Producto2Servicio : Servicio<ProductoDto>, IProducto2Servicio
    {
        private const string RutaAPI = "/api/apiproducto";

        //BOX
        private const string BOX_INFO = "/ObtenerBoxInfo";
        private const string BOX_INFO_STK = "/ObtenerBoxInfoStk";
        private const string BOX_INFO_MOV_STK = "/ObtenerBoxInfoMovStk";
        private const string UL_CONSULTA = "/ConsultaUL";

        private const string AJ_CARGA_CONTEO_PREVIOS = "/AJ_CargaConteosPrevios";
        private const string DV_CARGA_CONTEO_PREVIOS = "/DV_CargaConteosPrevios";

        private const string UP_MEDIDAS = "/ObtenerMedidas";
        private const string IVA_SITUACION = "/ObtenerIVASituacion";
        private const string IVA_ALICUOTAS = "/ObtenerIVAAlicuotas";
        private const string PROD_BARRADOS = "/ObtenerBarradoDeProd";
        private const string PROD_BARRADO = "/BuscarBarrado";
        private const string LIMITE_STK = "/ObtenerLimitesStkLista";
        private const string PROD_LIMITE = "/BuscaLimite";

        private const string PROD_DETALLE = "/obtener-producto-detalle";
        private const string PROD_DETALLE_LISTAS = "/obtener-producto-detalle-lista";

        private const string FX_PVTA_BASE = "/obtener-precio-pvta-base";
        private const string FX_PVTA_MARGEN = "/obtener-precio-pvta-margen";
        private const string FX_PVTA_LISTA = "/obtener-precio-pvta-lista";
        private const string PRODUCTO_CONF_PRECIO_TEMP = "/confirmar-precio-temporal";
        private const string PROD_DATO_IMPOSITIVO = "/ObtenerDatoImpositivo";
        private const string PROD_trace = "/obtener-producto-trace";
        private const string PROV_SMP = "/proveedor-sin-mod-precio";


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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<BoxInfoDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxInfoStkDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<BoxInfoMovStkDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<BoxInfoMovStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los movimientos del BOX" };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> AJ_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token)
        {
            try
            {
                ApiResponse<RespuestaDto>? apiResponse;
                HelperAPI helper = new();

                #region Armado de Json
                var j = lista.Select(x => new { depo_id = depo, box_id = box, x.at_id, x.usu_id, x.p_id, x.p_desc, x.up_id, x.unidad_pres, x.bulto, x.us, x.cantidad });
                var json = JsonConvert.SerializeObject(j);
                #endregion
                var ent = new CargarJsonGenRequest { json_str = json, admid = admid };

                HttpClient client = helper.InicializaCliente(ent, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{AJ_CARGA_CONTEO_PREVIOS}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    var resp = apiResponse.Data;
                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK", Entidad = resp };
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = resp.resultado_msj, Entidad = resp };
                    }
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar cargar los conteos previso de ajustes." };
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> DV_CargaConteosPrevios(List<ProductoGenDto> lista, string admid, string depo, string box, string token)
        {
            try
            {
                ApiResponse<RespuestaDto>? apiResponse;
                HelperAPI helper = new();

                #region Armado de Json
                var j = lista.Select(x => new { depo_id = depo, box_id = box, x.usu_id, x.p_id, x.p_desc, x.up_id, x.unidad_pres, x.bulto, x.us, x.cantidad });
                var json = JsonConvert.SerializeObject(j);
                #endregion
                var ent = new CargarJsonGenRequest { json_str = json, admid = admid };

                HttpClient client = helper.InicializaCliente(ent, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{DV_CARGA_CONTEO_PREVIOS}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    var resp = apiResponse.Data;
                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK", Entidad = resp };
                    }
                    else
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = resp.resultado_msj, Entidad = resp };
                    }
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<RespuestaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar cargar conteos previos de devolución de proveedores" };
            }
        }

        public async Task<RespuestaGenerica<ConsULDto>> ConsultaUL(string tipo, DateTime fecD, DateTime fecH, string admId, string token)
        {
            try
            {
                ApiResponse<List<ConsULDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;
                var d = fecD.Ticks;
                var h = fecH.Ticks;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{UL_CONSULTA}?tipo={tipo}&fecD={d}&fecH={h}&admId={admId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsULDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ConsULDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                return new RespuestaGenerica<ConsULDto> { Ok = false, Mensaje = "Algo no fue bien al intentar consultar la UL" };
            }
        }

        public async Task<RespuestaGenerica<MedidaDto>> ObtenerMedidas(string token)
        {
            try
            {
                ApiResponse<List<MedidaDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{UP_MEDIDAS}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MedidaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<MedidaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<MedidaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las medidas de productos." };
            }
        }

        public async Task<RespuestaGenerica<IVASituacionDto>> ObtenerIVASituacion(string token)
        {
            try
            {
                ApiResponse<List<IVASituacionDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{IVA_SITUACION}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IVASituacionDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<IVASituacionDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<IVASituacionDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las situaciones ante el IVA." };
            }
        }

        public async Task<RespuestaGenerica<IVAAlicuotaDto>> ObtenerIVAAlicuotas(string token)
        {
            try
            {
                ApiResponse<List<IVAAlicuotaDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{IVA_ALICUOTAS}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IVAAlicuotaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<IVAAlicuotaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<IVAAlicuotaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener las Alicuotas del IVA." };
            }
        }

        public async Task<RespuestaGenerica<ProductoBarradoDto>> ObtenerBarradoDeProd(string p_id, string token)
        {
            try
            {
                ApiResponse<List<ProductoBarradoDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_BARRADOS}?p_id={p_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoBarradoDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ProductoBarradoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<ProductoBarradoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los barrados de productos." };
            }
        }

        public async Task<RespuestaGenerica<LimiteStkDto>> ObtenerLimiteStk(string p_id, string token)
        {
            try
            {
                ApiResponse<List<LimiteStkDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{LIMITE_STK}?p_id={p_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<LimiteStkDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<LimiteStkDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<LimiteStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los Limites de Stock." };
            }
        }

        public async Task<RespuestaGenerica<ProductoBarradoDto>> ObtenerBarrado(string p_id, string barradoId, string token)
        {
            try
            {
                ApiResponse<ProductoBarradoDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_BARRADO}?p_id={p_id}&barradoId={barradoId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoBarradoDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ProductoBarradoDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };
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

                return new RespuestaGenerica<ProductoBarradoDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el barrado de producto." };
            }
        }

        public async Task<RespuestaGenerica<LimiteStkDto>> BuscarLimite(string p_id, string admId, string token)
        {
            try
            {
                ApiResponse<LimiteStkDto> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_LIMITE}?p_id={p_id}&admId={admId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<LimiteStkDto>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<LimiteStkDto> { Ok = true, Mensaje = "OK", Entidad = apiResponse.Data };
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

                return new RespuestaGenerica<LimiteStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los limites de stock del producto!!." };
            }
        }

        public async Task<RespuestaGenerica<ProductoDetalleDto>> Obtener_ProductoDetalle(QueryFilters filtro, string token)
        {
            try
            {
                ApiResponse<List<ProductoDetalleDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_DETALLE}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoDetalleDto>>>(stringData);

                    var listado = apiResponse.Data;

                    return new RespuestaGenerica<ProductoDetalleDto> { Ok = true, ListaEntidad = listado };

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar obtener el detalle de productos.");
            }
        }

        public async Task<RespuestaGenerica<ProductoDetalleDto>> Obtener_ProductoDetalleListas(QueryFilters filtro, string token)
        {
            try
            {
                ApiResponse<List<ProductoDetalleDto>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(filtro, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_DETALLE_LISTAS}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoDetalleDto>>>(stringData);

                    var listado = apiResponse.Data;

                    return new RespuestaGenerica<ProductoDetalleDto> { Ok = true, ListaEntidad = listado };

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar obtener el detalle de productos según las Listas.");
            }
        }

        public async Task<RespuestaGenerica<ProductoResponsePVta>> ObtenerPrecioVentaBase(ProductoRequestPvtaBase req, string token)
        {
            try
            {
                ApiResponse<ProductoResponsePVta>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(req, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{FX_PVTA_BASE}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoResponsePVta>>(stringData);

                    var listado = apiResponse.Data;

                    return new RespuestaGenerica<ProductoResponsePVta> { Ok = true, Entidad = listado };

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar recalcular el precio de venta del producto.");
            }
        }

        public async Task<RespuestaGenerica<ProductoResponsePVtaMargen>> ObtenerPrecioVentaMargen(ProductoRequestPVtaMargen req, string token)
        {
            try
            {
                ApiResponse<ProductoResponsePVtaMargen>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(req, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{FX_PVTA_MARGEN}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<ProductoResponsePVtaMargen>>(stringData);

                    var listado = apiResponse.Data;

                    return new RespuestaGenerica<ProductoResponsePVtaMargen> { Ok = true, Entidad = listado };

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar recalcular el precio de venta del producto x margen.");
            }
        }

        public async  Task<RespuestaGenerica<ProductoResponsePVta>> ObtenerPrecioVentaLista(ProductoRequestPvtaLista req, string token)
        {
            try
            {
                ApiResponse<List<ProductoResponsePVta>>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(req, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{FX_PVTA_LISTA}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoResponsePVta>>>(stringData);

                    var listado = apiResponse?.Data;

                    return new RespuestaGenerica<ProductoResponsePVta> { Ok = true, ListaEntidad = listado };

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar recalcular el precio de venta de las listas.");
            }
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarPreciosTemporales(AbmGenDto req, string token)
        {
            try
            {
                ApiResponse<RespuestaDto>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(req, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PRODUCTO_CONF_PRECIO_TEMP}";

                response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        throw new NegocioException("No se recepcionó una respuesta válida. Intente de nuevo más tarde.");
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    var resp = apiResponse?.Data;
                    if (resp == null)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            EsError = true,
                            Mensaje = "No se recibió el resultado de la confirmación de precios."
                        };
                    }

                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = true,
                            Entidad = resp,
                            Mensaje = string.IsNullOrWhiteSpace(resp.resultado_msj) ? "OK" : resp.resultado_msj
                        };
                    }

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        EsWarn = resp.resultado == 1,
                        EsError = resp.resultado < 0 || resp.resultado > 1,
                        Entidad = resp,
                        Mensaje = string.IsNullOrWhiteSpace(resp.resultado_msj)
                            ? "GECO no pudo confirmar los precios temporales."
                            : resp.resultado_msj
                    };
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");

                throw new Exception("Algo no fue bien al intentar confirmar los precios temporales.");
            }
        }

        public async Task<RespuestaGenerica<ImpositivoDatoDto>> ObtenerDatoImpositivo(QueryFilters filters, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(filters, token, out StringContent contentData);
                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_DATO_IMPOSITIVO}";

                using var response = await client.PostAsync(link, contentData);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recibió respuesta válida de la API" };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ImpositivoDatoDto>>>(stringData);
                    if (apiResponse == null || apiResponse.Data == null)
                    {
                        return new() { Ok = false, Mensaje = "Error deserializando la respuesta de la API" };
                    }

                    return new RespuestaGenerica<ImpositivoDatoDto>
                    {
                        Ok = true,
                        Mensaje = "OK",
                        ListaEntidad = apiResponse.Data
                        // Nota: si necesitas la metadata (apiResponse.Meta), amplía RespuestaGenerica para incluirla.
                    };
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
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al buscar la lista de precios" };
            }
            ;
        }

        public async Task<RespuestaGenerica<ProductoTraceDto>> ObtenerProductoTrace(DateTime desde, DateTime hasta, string token)
        {
            try
            {
                if (desde == default || hasta == default)
                {
                    return new() { Ok = false, Mensaje = "Debe indicar fechas válidas." };
                }

                if(desde > hasta)
                {
                    return new() { Ok = false, Mensaje = "La fecha 'desde' no puede ser mayor a la fecha 'hasta'." };
                }

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_trace}?desde={desde}&hasta={hasta}";
                return await GetListaAsync<ProductoTraceDto>(link, token, "Error " +
                    "al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener la Carga Previa" };
            }
        }

        public async Task<RespuestaGenerica<ProvSinModPrecioDto>> ProvSinModPrecio(DateTime desde, string token)
        {
            try
            {
                if (desde == default )
                {
                    return new() { Ok = false, Mensaje = "Debe indicar una fecha válida." };
                }

               
                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROV_SMP}?desde={desde}";
                return await GetListaAsync<ProvSinModPrecioDto>(link, token, "Error " +
                    "al indicar la administración");
            }
            catch (Exception ex)
            {
                _logger.LogError($"{GetType().Name}-{MethodBase.GetCurrentMethod()?.Name} - {ex}");
                return new() { Ok = false, Mensaje = "Error al obtener la Carga Previa" };
            }
        }
    }
}
