using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Almacen;
using gc.infraestructura.Dtos.Almacen.Info;
using gc.infraestructura.Dtos.Almacen.Rpr;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Productos;
using gc.sitio.core.Servicios.Contratos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

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
        private const string PROD_BARRADO = "/ObtenerBarradoDeProd";
        private const string LIMITE_STK = "/ObtenerLimiteStk";

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

        public async Task<RespuestaGenerica<RespuestaDto>> AJ_CargaConteosPrevios(List<ProductoGenDto> lista,string admid,string depo,string box,string token)
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

                HttpClient client = helper.InicializaCliente(ent, token,out StringContent contentData);
                HttpResponseMessage response;
             
                var link = $"{_appSettings.RutaBase}{RutaAPI}{AJ_CARGA_CONTEO_PREVIOS}";

                response = await client.PostAsync(link,contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    var resp = apiResponse.Data;
                    if (resp.resultado == 0)
                    {
                        return new RespuestaGenerica<RespuestaDto> { Ok = true, Mensaje = "OK", Entidad = resp};
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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

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
                var d=fecD.Ticks;
                var h=fecH.Ticks;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{UL_CONSULTA}?tipo={tipo}&fecD={d}&fecH={h}&admId={admId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ConsULDto>>>(stringData);

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
                _logger.LogError($"{this.GetType().Name}-{MethodBase.GetCurrentMethod().Name} - {ex}");

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MedidaDto>>>(stringData);

                    return new RespuestaGenerica<MedidaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<MedidaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<MedidaDto> { Ok = false, Mensaje = error.Detail };
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IVASituacionDto>>>(stringData);

                    return new RespuestaGenerica<IVASituacionDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<IVASituacionDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<IVASituacionDto> { Ok = false, Mensaje = error.Detail };
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<IVAAlicuotaDto>>>(stringData);

                    return new RespuestaGenerica<IVAAlicuotaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<IVAAlicuotaDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<IVAAlicuotaDto> { Ok = false, Mensaje = error.Detail };
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

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PROD_BARRADO}?p_id={p_id}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoBarradoDto>>>(stringData);

                    return new RespuestaGenerica<ProductoBarradoDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<ProductoBarradoDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<ProductoBarradoDto> { Ok = false, Mensaje = error.Detail };
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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<LimiteStkDto>>>(stringData);

                    return new RespuestaGenerica<LimiteStkDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

                }
                else
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Algo no fue bien. Error de API {stringData}");
                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(stringData);
                    if (error.TypeException.Equals(nameof(NegocioException)))
                    {
                        return new RespuestaGenerica<LimiteStkDto> { Ok = false, Mensaje = error.Detail };
                    }
                    else if (error.TypeException.Equals(nameof(NotFoundException)))
                    {
                        return new RespuestaGenerica<LimiteStkDto> { Ok = false, Mensaje = error.Detail };
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

                return new RespuestaGenerica<LimiteStkDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener los Limites de Stock." };
            }
        }
    }
}
