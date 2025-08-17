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
using gc.sitio.core.Servicios.Contratos.Importacion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace gc.sitio.core.Servicios.Implementacion.Importacion
{
    public class ImportarServicio : Servicio<Dto>, IImportarServicio
    {
        private const string RutaAPI = "/api/apiimportar";

        private const string PRECIO_FILE_DATOS = "/precio-file-dato";
        private const string PRECIO_FILE_PERFIL = "/precio-file-perfil";
        private const string PRECIO_FILE_CONFIRMAR = "/confirmar-perfil-precio";

        private readonly AppSettings _appSettings;
        public ImportarServicio(IOptions<AppSettings> options, ILogger<ImportarServicio> logger) : base(options, logger)
        {
            _appSettings = options.Value;
        }

        public async Task<RespuestaGenerica<RespuestaDto>> ConfirmarPerfilPrecio(AbmGenDto req, string token)
        {
            try
            {
                ApiResponse<RespuestaDto>? apiResponse;
                HelperAPI helper = new();

                HttpClient client = helper.InicializaCliente(req, token, out StringContent contentData);
                HttpResponseMessage response;

                var link = $"{_appSettings.RutaBase}{RutaAPI}{PRECIO_FILE_CONFIRMAR}";

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

                    return new RespuestaGenerica<RespuestaDto> { Ok = true, Entidad = resp };
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

                throw new Exception("Algo no fue bien al intentar confirmar los precios de la Importación.");
            }
        }

        public async Task<RespuestaGenerica<ProveedorPerfilDB>> ObtenerPerfilPrecioProveedor(string ctaId, string token)
        {
            try
            {
                ApiResponse<List<ProveedorPerfilDB>> apiResponse;

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
                    apiResponse = JsonConvert.DeserializeObject<ApiResponse<List<ProveedorPerfilDB>>>(stringData) ?? throw new NegocioException("Hubo un problema al deserializar los datos");

                    return new RespuestaGenerica<ProveedorPerfilDB> { Ok = true, Mensaje = "OK", ListaEntidad = apiResponse.Data };

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

                return new RespuestaGenerica<ProveedorPerfilDB> { Ok = false, Mensaje = "Algo no fue bien al intentar obtener el perfil del Cliente" };
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
    }
}
