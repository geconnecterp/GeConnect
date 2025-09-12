using gc.infraestructura.Core.EntidadesComunes;
using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos;
using gc.infraestructura.Dtos.ABM;
using gc.infraestructura.Dtos.Box;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.Dtos.Importacion;
using gc.infraestructura.Dtos.Productos;
using gc.infraestructura.Dtos.Productos.Actualiza;
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Ocsp;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static OfficeOpenXml.ExcelErrorValue;

namespace gc.sitio.core.Servicios.Implementacion.Importacion
{
    public class ImportarServicio : Servicio<Dto>, IImportarServicio
    {
        private const string RutaAPI = "/api/apiimportar";

        private const string PRECIO_FILE_DATOS = "/precio-file-dato";
        private const string PRECIO_FILE_PERFIL = "/precio-file-perfil";
        private const string PRECIO_FILE_CARGAR = "/cargar-perfil-precio";

        private const string ACTUALIZA_PROVEEDORES = "/proveedores-actualizar";
        private const string ACTUALIZA_PRODUCTOS = "/productos-actualizar";
        private const string ACTUALIZA_CONFIRMAR = "/confirmar-actualizacion-precio";

        //private readonly AppSettings _appSettings;
        private readonly IHttpContextAccessor _contexto;
        public ImportarServicio(IOptions<AppSettings> options, ILogger<ImportarServicio> logger,
            IHttpContextAccessor contexto) : base(options, logger)
        {
            //_appSettings = options.Value;
            _contexto = contexto;
        }

        #region Métodos de Importación

        public async Task<RespuestaGenerica<RespuestaCPDto>> CargarImportacionPrecio(AbmPlusGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PRECIO_FILE_CARGAR}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaCPDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<RespuestaCPDto>>>(stringData);

                    return new RespuestaGenerica<RespuestaCPDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse?.Data ?? new List<RespuestaCPDto>(),
                        Mensaje = "Importación procesada exitosamente"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaCPDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CargarImportacionPrecio");

                return new RespuestaGenerica<RespuestaCPDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando la importación"
                };
            }
        }
        
        public async Task<RespuestaGenerica<MapeoColumnaDto>> ObtenerPerfilDeProveedor(string ctaId, string token)
        {
            try
            {
                ApiResponse<List<MapeoColumnaDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PRECIO_FILE_PERFIL}?ctaId={ctaId}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<MapeoColumnaDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<MapeoColumnaDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<MapeoColumnaDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el perfil del Cliente" };
            }
        }

        public async Task<RespuestaGenerica<PrecioFileDatos>> ObtenerPrecioFileDatos(string token)
        {
            try
            {
                ApiResponse<List<PrecioFileDatos>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PRECIO_FILE_DATOS}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<PrecioFileDatos>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<PrecioFileDatos> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<PrecioFileDatos> { Ok = false, Mensaje = "Algo no fue bien al intentar obtee" };
            }
        }
        #endregion


        #region Método de Actualización

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarActualizacionPrecioProductosDeProveedor(AbmGenDto req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ACTUALIZA_CONFIRMAR}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<RespuestaDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<RespuestaDto>>(stringData);

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = true,
                        Entidad = apiResponse?.Data ?? new RespuestaDto(),
                        Mensaje = "Importación procesada exitosamente"
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<RespuestaDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CargarImportacionPrecio");

                return new RespuestaGenerica<RespuestaDto>
                {
                    Ok = false,
                    Mensaje = "Error interno procesando la importación"
                };
            }
        }

        public async Task<RespuestaGenerica<ProductoDetalleDto>> ObtenerProductosDelProveedorParaActualizar(QueryFilters req, string token)
        {
            try
            {
                var helper = new HelperAPI();
                var client = helper.InicializaCliente(req, token, out StringContent contentData);

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ACTUALIZA_PRODUCTOS}";

                using var response = await client.PostAsync(link, contentData);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var stringData = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrEmpty(stringData))
                    {
                        return new RespuestaGenerica<ProductoDetalleDto>
                        {
                            Ok = false,
                            Mensaje = "No se recibió respuesta válida de la API"
                        };
                    }

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProductoDetalleDto>>>(stringData);
                    var valor = JsonConvert.SerializeObject(apiResponse?.Meta);
                    _contexto.HttpContext?.Session.SetString("MetadataGeneral", valor);
                    return new RespuestaGenerica<ProductoDetalleDto>
                    {
                        Ok = true,
                        ListaEntidad = apiResponse?.Data ?? [],
                        Mensaje = "Se obtuvieron los productos exitosamente."
                    };
                }
                else
                {
                    var errorData = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Error API ({response.StatusCode}): {errorData}");

                    var error = JsonConvert.DeserializeObject<ExceptionValidation>(errorData);
                    var mensaje = error?.Detail ?? "Error desconocido en la API";

                    return new RespuestaGenerica<ProductoDetalleDto>
                    {
                        Ok = false,
                        Mensaje = mensaje
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CargarImportacionPrecio");

                return new RespuestaGenerica<ProductoDetalleDto>
                {
                    Ok = false,
                    Mensaje = "Error interno al obtener los productos"
                };
            }
        }

        public async Task<RespuestaGenerica<ActualizaProveedorDto>> ObtenerProveedoresConProductosParaActualizar(string token)
        {
            try
            {
                ApiResponse<List<ActualizaProveedorDto>> apiResponse;

                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(token);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{ACTUALIZA_PROVEEDORES}";

                response = await client.GetAsync(link);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string stringData = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrEmpty(stringData))
                    {

                        return new() { Ok = false, Mensaje = "No se recepcionó una respuesta válida. Intente de nuevo más tarde." };
                    }
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ActualizaProveedorDto>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ActualizaProveedorDto> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<ActualizaProveedorDto> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el perfil del Cliente" };
            }
        }
        #endregion
    }
}
