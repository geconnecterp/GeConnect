using gc.infraestructura.Core.EntidadesComunes.Options;
using gc.infraestructura.Core.Exceptions;
using gc.infraestructura.Core.Helpers;
using gc.infraestructura.Core.Responses;
using gc.infraestructura.Dtos.Gen;
using gc.infraestructura.EntidadesComunes.Options;
using gc.sitio.core.Servicios.Contratos.DocManager;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System.Net;
using System.Reflection;
using System.Text;

namespace gc.sitio.core.Servicios.Implementacion.DocManager
{
    public class DocMgServicio : IDocMgServicio
    {

        private const string RutaAPI = "/api/reportes";
        private const string RutaGenerar = "/generate";
        private const string RutaGenerarFormato = "/GenFileFormat";

        private readonly AppSettings _appSettings;
        private readonly DocsManager _docManager;
        private readonly ILogger<DocMgServicio> _logger;

        public DocMgServicio(IOptions<AppSettings> options, 
            ILogger<DocMgServicio> logger, IOptions<DocsManager> options1)
        {
            _appSettings = options.Value;
            _logger = logger;
            _docManager = options1.Value;
        }

        /// <summary>
        /// Este metodo se encargara de recepcionar un string en base64 
        /// el cual debera transformarlo en el tipo de clase ReporteSolicitudDto.
        /// Posteriormente decodificado se debe invocar la api para obtener el 
        /// reporte que los parametros indican.
        /// </summary>
        /// <param name="parametros">string Base64</param>
        /// <returns>RespuestaReportDto el archivo en Base64</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<RespuestaReportDto> ObtenerRepoParaUsuario(string parametros)
        {
            try
            {
                string stringData;
                ApiResponse<RespuestaReportDto> respuesta;
                // Decodificar el string Base64 a JSON
                string jsonDecodificado;
                try
                {
                    byte[] bytesDecodificados = Convert.FromBase64String(parametros);
                    jsonDecodificado = Encoding.UTF8.GetString(bytesDecodificados);
                    _logger.LogInformation("Parámetros Base64 decodificados exitosamente.");
                }
                catch (FormatException ex)
                {
                    _logger.LogError(ex, "Error al decodificar el string Base64.");
                    throw new NegocioException("El parámetro proporcionado no es un Base64 válido.");
                }

                // Deserializar JSON a ReporteSolicitudDto
                ReporteSolicitudDto reporteSolicitud;
                try
                {
                    reporteSolicitud = JsonConvert.DeserializeObject<ReporteSolicitudDto>(jsonDecodificado)
                        ?? throw new NegocioException("La deserialización resultó en un objeto nulo.");
                    _logger.LogInformation("Solicitud de reporte deserializada correctamente.");
                }
                catch (JsonException ex)
                {
                    _logger.LogError(ex, "Error al deserializar el JSON a ReporteSolicitudDto.");
                    throw new NegocioException("El JSON proporcionado no tiene el formato correcto para ReporteSolicitudDto.");
                }



                _logger.LogInformation($"Iniciando proceso para obtener el PDF desde la API de Reportes.{MethodBase.GetCurrentMethod().Name}");
                HelperAPI helper = new();
                HttpClient client = helper.InicializaCliente(reporteSolicitud, string.Empty, out StringContent content);
                HttpResponseMessage response;
                var link = $"{_docManager.ApiReporteUrl}{RutaAPI}{RutaGenerar}";
                _logger.LogInformation($"Enviando solicitud a la API de Reportes. URL: {link}");
                response = await client.PostAsync(link, content);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(stringData))
                    {
                        respuesta = JsonConvert.DeserializeObject<ApiResponse<RespuestaReportDto>>(stringData) ??
                            throw new NegocioException("Hubo un problema en la deserialización de los datos de la API.");
                    }
                    else
                    {
                        throw new Exception("No se logro obtener la respuesta de la API de reportes. Verifique.");
                    }
                    return respuesta.Data;
                }
                else
                {
                    stringData = await response.Content.ReadAsStringAsync();
                    string msg = "Hubo un error al intentar obtener el Informe desde la Api de Reportes.";
                    _logger.LogError($"{msg} {stringData}");
                    throw new NegocioException(msg);
                }
            }
            catch (NegocioException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hubo un error al intentar obtener el Informe desde la Api de Reportes.");
                throw;
            }
        }
    }
}
